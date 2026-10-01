using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;
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
                var routing = await _dbContext.CallbackRoutings
                    .FirstOrDefaultAsync(item => item.RefID == request.Payload.Id, cancellationToken);
                if (routing != null)
                    return await ForwardNotificationAsync(routing, request, payload, cancellationToken);
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

                    return await ForwardNotificationAsync(routing, request, payload, cancellationToken);
                default:
                    return await _apiClient.PostTAsync<NpPaymentNotificationResponse>(
                        _pConfig.Napas.ForwardNotiPaymentURL, payload);
            }
        }

        private async Task<NpPaymentNotificationResponse> ForwardNotificationAsync(
            CallbackRouting routing,
            PaymentNotificationRequest request,
            string payload,
            CancellationToken cancellationToken)
        {
            var transaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(x =>
                    x.CallbackRoutingID == routing.ID ||
                    (x.Provider == "NAPAS" && x.ProviderTransactionID == request.Payload.Id),
                    cancellationToken);

            var notification = await RecordNotificationAsync(transaction, request, payload, cancellationToken);
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

        private async Task<PaymentNotification?> RecordNotificationAsync(
            PaymentTransaction? transaction,
            PaymentNotificationRequest request,
            string payload,
            CancellationToken cancellationToken)
        {
            if (transaction == null)
                return null;

            var isDuplicate = await _dbContext.PaymentNotifications.AnyAsync(x =>
                x.PaymentTransactionID == transaction.ID &&
                x.ProviderTransactionID == request.Payload.Id &&
                x.Payload == payload, cancellationToken);

            var notification = new PaymentNotification
            {
                PaymentTransactionID = transaction.ID,
                RequestID = RequestContext.RequestID,
                Provider = "NAPAS",
                ProviderTransactionID = request.Payload.Id ?? string.Empty,
                ProviderReferenceID = request.Payload.RefId ?? string.Empty,
                ProviderStatus = request.Payload.Status ?? string.Empty,
                Payload = payload,
                IsDuplicate = isDuplicate,
                ProcessingStatus = isDuplicate
                    ? PaymentNotificationProcessingStatus.DUPLICATE
                    : PaymentNotificationProcessingStatus.RECEIVED
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
