using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;
using System.Globalization;
using static S3.Gateway.Features.Payments.Napas.TransIdCodecHelper;

namespace S3.Gateway.Features.Payments.Napas
{
    public class PaymentNotificationRequest : NpRequestBase<NpPaymentNotificationRequest>, IRequest<NpPaymentNotificationResponse>
    {
    }

    public class PaymentNotificationRequestHandler : IRequestHandler<PaymentNotificationRequest, NpPaymentNotificationResponse>
    {
        private readonly DBContext _dbContext;
        private readonly PaymentConfig _pConfig;
        private readonly BaseApiClient _apiClient;

        public PaymentNotificationRequestHandler(DBContext dbContext, IOptions<PaymentConfig> options, BaseApiClient apiClient)
        {
            _dbContext = dbContext;
            _pConfig = options.Value;
            _apiClient = apiClient;
        }

        public async Task<NpPaymentNotificationResponse> Handle(PaymentNotificationRequest request, CancellationToken cancellationToken)
        {
            var payload = Utility.SerializeObjectLowerCase(request);
            var billNumberEncode = request.Payload.Id[^7..];

            if (billNumberEncode == "NAPGSQR")
            {
                var recorded = await RecordStaticNotificationAsync(request, payload, cancellationToken);
                if (recorded != null)
                    return await ForwardNotificationAsync(recorded.Value.Routing, request, payload,
                        recorded.Value.Transaction, recorded.Value.Notification, cancellationToken);

                return await RecordUnroutedNotificationAsync(request, payload, cancellationToken);
            }

            DecodeResult decodeData;
            try
            {
                decodeData = TransIdCodecHelper.Decode(billNumberEncode);
            }
            catch
            {
                return await _apiClient.PostTAsync<NpPaymentNotificationResponse>(
                    _pConfig.Napas.ForwardNotiPaymentURL, payload);
            }

            switch (decodeData.Type.GetNapasType())
            {
                case NapasTransactionInvoiceType.Forward:
                    var routing = await _dbContext.CallbackRoutings
                        .FirstOrDefaultAsync(item => item.ID == decodeData.Id, cancellationToken);
                    if (routing == null)
                        throw new Exception("Bill number không hợp lệ");

                    var transaction = await _dbContext.PaymentTransactions
                        .FirstOrDefaultAsync(x => x.CallbackRoutingID == routing.ID, cancellationToken);
                    var notification = await RecordNotificationAsync(transaction, request, payload, cancellationToken);
                    return await ForwardNotificationAsync(routing, request, payload,
                        transaction, notification, cancellationToken);
                default:
                    return await _apiClient.PostTAsync<NpPaymentNotificationResponse>(
                        _pConfig.Napas.ForwardNotiPaymentURL, payload);
            }
        }

        private async Task<NpPaymentNotificationResponse> RecordUnroutedNotificationAsync(
            PaymentNotificationRequest request,
            string payload,
            CancellationToken cancellationToken)
        {
            var providerTransactionId = request.Payload.Id ?? string.Empty;
            var providerReferenceId = request.Payload.RefId ?? string.Empty;
            var now = DateTime.Now;
            decimal.TryParse(request.Payload.Amount, NumberStyles.Number,
                CultureInfo.InvariantCulture, out var amount);

            var transaction = new PaymentTransaction
            {
                PartnerID = null,
                Provider = "NAPAS",
                PaymentMethod = "QR",
                GatewayRequestID = RequestContext.RequestID,
                ProviderTransactionID = providerTransactionId,
                ProviderReferenceID = providerReferenceId,
                AccountNumber = providerTransactionId[..^7],
                BankID = request.Payload.BeneficiaryBank ?? string.Empty,
                Amount = amount,
                Currency = "704",
                BillNumber = "NAPGSQR",
                QRType = "STATIC"
            };
            _dbContext.PaymentTransactions.Add(transaction);

            transaction.ProviderStatus = request.Payload.Status ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Payload.CustomerName))
                transaction.CustomerName = request.Payload.CustomerName;
            transaction.Status = IsSuccessful(request.Payload.Status)
                ? PaymentTransactionStatus.SUCCESS
                : PaymentTransactionStatus.PROCESSING;
            transaction.LastNotificationAt = now;
            if (transaction.Status == PaymentTransactionStatus.SUCCESS)
                transaction.PaidAt ??= now;

            await _dbContext.SaveChangesAsync(cancellationToken);
            var notification = await RecordNotificationAsync(transaction, request, payload, cancellationToken);
            if (notification != null)
            {
                notification.ProcessingStatus = PaymentNotificationProcessingStatus.PROCESSED;
                notification.ProcessedAt = now;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return new NpPaymentNotificationResponse
            {
                Code = "success",
                Message = "Notification received"
            };
        }

        private async Task<NpPaymentNotificationResponse> ForwardNotificationAsync(
            CallbackRouting routing,
            PaymentNotificationRequest request,
            string payload,
            PaymentTransaction? transaction,
            PaymentNotification? notification,
            CancellationToken cancellationToken)
        {
            var forwardResponse = await _apiClient.PostTAsync<NpPaymentNotificationResponse>(routing.CallbackUrl, payload);
            var forwardHttpStatus = _apiClient.LastStatusCode.HasValue
                ? (int?)_apiClient.LastStatusCode.Value
                : null;
            var forwardSuccess = forwardHttpStatus == 200 &&
                forwardResponse != null &&
                string.Equals(forwardResponse.Code, "success", StringComparison.OrdinalIgnoreCase);

            var routingLog = new CallbackRoutingLog
            {
                RequestID = RequestContext.RequestID,
                RefID = request.Payload.Id ?? string.Empty,
                Action = forwardSuccess ? "FORWARD COMPLETED" : "FORWARD ERROR",
                RequestPayload = payload,
                ResponsePayload = forwardSuccess
                    ? Utility.SerializeObjectLowerCase(forwardResponse)
                    : string.Empty,
                IsSuccess = forwardSuccess,
            };

            routing.ActionHistory += "  -> " + routingLog.Action;
            routing.Status = forwardSuccess ? CallbackRoutingStatus.COMPLETED : CallbackRoutingStatus.ERROR;
            _dbContext.CallbackRoutingLogs.Add(routingLog);

            await CompleteNotificationAsync(
                transaction,
                notification,
                request,
                forwardSuccess,
                forwardHttpStatus,
                forwardResponse,
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return forwardResponse;
        }

        private async Task<(CallbackRouting Routing, PaymentTransaction Transaction, PaymentNotification Notification)?> RecordStaticNotificationAsync(
            PaymentNotificationRequest request,
            string payload,
            CancellationToken cancellationToken)
        {
            await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var providerTransactionId = request.Payload.Id ?? string.Empty;
            var lockResource = $"napas-static-qr:{providerTransactionId}";
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @lockResult int;
                EXEC @lockResult = sys.sp_getapplock
                    @Resource = {lockResource},
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 30000;
                IF @lockResult < 0
                    THROW 51010, 'Could not lock static QR routing.', 1;", cancellationToken);

            // Claim unused QR creation rows in creation order, regardless of callback status.
            var available = await (
                from candidateRouting in _dbContext.CallbackRoutings
                join candidateOriginal in _dbContext.PaymentTransactions
                    on (int?)candidateRouting.ID equals candidateOriginal.CallbackRoutingID
                where candidateRouting.RefID == providerTransactionId &&
                    candidateRouting.Target == Target3rd.Napas &&
                    candidateOriginal.GatewayRequestID == candidateRouting.RequestID &&
                    candidateOriginal.QRType == "STATIC" &&
                    candidateOriginal.Status == PaymentTransactionStatus.PENDING &&
                    candidateOriginal.NotificationPaymentID == null
                orderby candidateRouting.CreatedAt, candidateRouting.ID
                select new { Routing = candidateRouting, Original = candidateOriginal }
            ).FirstOrDefaultAsync(cancellationToken);

            var routing = available?.Routing ?? await _dbContext.CallbackRoutings
                .Where(item => item.RefID == providerTransactionId && item.Target == Target3rd.Napas)
                .OrderBy(item => item.CreatedAt)
                .ThenBy(item => item.ID)
                .FirstOrDefaultAsync(cancellationToken);
            if (routing == null)
                return null;

            var original = available?.Original ?? await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(x => x.CallbackRoutingID == routing.ID &&
                    x.GatewayRequestID == routing.RequestID, cancellationToken);

            // Each unused Gen QR routing gets one notification before reusing the oldest routing.
            var notificationRequestId = RequestContext.RequestID;
            var useOriginal = available != null && original != null &&
                await _dbContext.PaymentTransactions
                    .Where(x => x.ID == original.ID && x.NotificationPaymentID == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.NotificationPaymentID, notificationRequestId), cancellationToken) == 1;

            PaymentTransaction transaction;
            decimal.TryParse(request.Payload.Amount, NumberStyles.Number,
                CultureInfo.InvariantCulture, out var amount);
            if (useOriginal)
            {
                transaction = original!;
                transaction.NotificationPaymentID = notificationRequestId;
                transaction.Amount = amount;
            }
            else
            {
                transaction = new PaymentTransaction
                {
                    PartnerID = original?.PartnerID ?? routing.PartnerID,
                    Provider = "NAPAS",
                    PaymentMethod = "QR",
                    ClientRequestID = original?.ClientRequestID ?? routing.ClientRequestID ?? string.Empty,
                    GatewayRequestID = notificationRequestId,
                    RequestCreateQRID = routing.RequestID,
                    ProviderTransactionID = providerTransactionId,
                    ProviderReferenceID = request.Payload.RefId ?? string.Empty,
                    CallbackRoutingID = routing.ID,
                    AccountNumber = original?.AccountNumber ?? providerTransactionId[..^7],
                    BankID = request.Payload.BeneficiaryBank ?? original?.BankID ?? string.Empty,
                    Amount = amount,
                    Currency = original?.Currency ?? "704",
                    BillNumber = original?.BillNumber ?? "NAPGSQR",
                    Description = original?.Description ?? string.Empty,
                    QRType = "STATIC",
                    RawCreateRequest = original?.RawCreateRequest ?? routing.RequestPayload
                };
                _dbContext.PaymentTransactions.Add(transaction);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var notification = (await RecordNotificationAsync(transaction, request, payload, cancellationToken))!;
            await dbTransaction.CommitAsync(cancellationToken);
            return (routing, transaction, notification);
        }

        private async Task<PaymentNotification?> RecordNotificationAsync(
            PaymentTransaction? transaction,
            PaymentNotificationRequest request,
            string payload,
            CancellationToken cancellationToken)
        {
            if (transaction == null)
                return null;

            transaction.NotificationPaymentID = RequestContext.RequestID;
            var notification = new PaymentNotification
            {
                PaymentTransactionID = transaction.ID,
                RequestID = RequestContext.RequestID,
                Provider = "NAPAS",
                ProviderTransactionID = request.Payload.Id ?? string.Empty,
                ProviderReferenceID = request.Payload.RefId ?? string.Empty,
                ProviderStatus = request.Payload.Status ?? string.Empty,
                Payload = payload,
                ProcessingStatus = PaymentNotificationProcessingStatus.RECEIVED
            };

            _dbContext.PaymentNotifications.Add(notification);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return notification;
        }

        private async Task CompleteNotificationAsync(
            PaymentTransaction? transaction,
            PaymentNotification? notification,
            PaymentNotificationRequest request,
            bool forwardSuccess,
            int? forwardHttpStatus,
            NpPaymentNotificationResponse? forwardResponse,
            CancellationToken cancellationToken)
        {
            if (transaction == null)
                return;

            transaction.ProviderTransactionID = request.Payload.Id ?? transaction.ProviderTransactionID;
            transaction.ProviderReferenceID = request.Payload.RefId ?? transaction.ProviderReferenceID;
            transaction.ProviderStatus = request.Payload.Status ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Payload.CustomerName))
                transaction.CustomerName = request.Payload.CustomerName;
            transaction.LastNotificationAt = DateTime.Now;
            transaction.Status = IsSuccessful(request.Payload.Status)
                ? PaymentTransactionStatus.SUCCESS
                : PaymentTransactionStatus.PROCESSING;
            if (transaction.Status == PaymentTransactionStatus.SUCCESS)
                transaction.PaidAt = DateTime.Now;

            if (notification != null)
            {
                notification.ProcessedAt = DateTime.Now;
                notification.ProcessingStatus = forwardSuccess
                    ? PaymentNotificationProcessingStatus.PROCESSED
                    : PaymentNotificationProcessingStatus.ERROR;
                notification.ErrorMessage = forwardSuccess ? string.Empty : "Forward callback thất bại";
            }

            transaction.PartnerCallbackStatus = forwardSuccess
                ? PartnerCallbackStatus.SUCCESS
                : PartnerCallbackStatus.FAILED;
            transaction.PartnerCallbackHttpStatus = forwardHttpStatus;
            transaction.PartnerCallbackCode = forwardResponse?.Code ?? string.Empty;
            transaction.PartnerCallbackMessage = forwardResponse?.Message ?? string.Empty;
            transaction.PartnerCallbackAt = DateTime.Now;
            transaction.PartnerCallbackError = forwardSuccess ? string.Empty : "HTTP status hoặc response.code không hợp lệ";

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static bool IsSuccessful(string? status)
        {
            return string.Equals(status, "00", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "ACSP", StringComparison.OrdinalIgnoreCase);
        }
    }
}
