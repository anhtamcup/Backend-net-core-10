using MediatR;
using Microsoft.EntityFrameworkCore;
using S3.Gateway.Data;
using S3.Gateway.Entities;

namespace S3.Gateway.Features.Payments
{
    public class PaymentHistoryRequest : IRequest<PaymentHistoryResponse>
    {
        public int? PartnerID { get; set; }
        public string? PartnerCode { get; set; }
        public string? Provider { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public string? ProviderStatus { get; set; }
        public string? PartnerCallbackStatus { get; set; }
        public string? TransactionID { get; set; }
        public string? ClientRequestID { get; set; }
        public string? AccountNumber { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class PaymentHistoryResponse
    {
        public IReadOnlyList<PaymentHistoryItem> Items { get; set; } = Array.Empty<PaymentHistoryItem>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaymentHistoryItem
    {
        public int ID { get; set; }
        public int? PartnerID { get; set; }
        public string PartnerCode { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
        public string ReferenceID { get; set; } = string.Empty;
        public string ClientRequestID { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BankID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string BillNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string QRType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ProviderStatus { get; set; } = string.Empty;
        public string PartnerCallbackStatus { get; set; } = string.Empty;
        public int? PartnerCallbackHttpStatus { get; set; }
        public string PartnerCallbackCode { get; set; } = string.Empty;
        public string PartnerCallbackMessage { get; set; } = string.Empty;
        public DateTime? PartnerCallbackAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? LastNotificationAt { get; set; }
    }

    public class PaymentHistoryHandler : IRequestHandler<PaymentHistoryRequest, PaymentHistoryResponse>
    {
        private readonly DBContext _dbContext;

        public PaymentHistoryHandler(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaymentHistoryResponse> Handle(PaymentHistoryRequest request, CancellationToken cancellationToken)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 200);
            var query = _dbContext.PaymentTransactions.AsNoTracking();

            if (request.PartnerID.HasValue)
                query = query.Where(x => x.PartnerID == request.PartnerID.Value);
            if (!string.IsNullOrWhiteSpace(request.PartnerCode))
            {
                var partnerCodes = request.PartnerCode
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                if (partnerCodes.Length == 1)
                    query = query.Where(x => x.Partner != null && x.Partner.Code == partnerCodes[0]);
                else if (partnerCodes.Length > 1)
                    query = query.Where(x => x.Partner != null && partnerCodes.Contains(x.Partner.Code));
            }
            if (!string.IsNullOrWhiteSpace(request.Provider))
                query = query.Where(x => x.Provider == request.Provider);
            if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
                query = query.Where(x => x.PaymentMethod == request.PaymentMethod);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<PaymentTransactionStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.ProviderStatus))
                query = query.Where(x => x.ProviderStatus == request.ProviderStatus);
            if (!string.IsNullOrWhiteSpace(request.PartnerCallbackStatus) &&
                Enum.TryParse<PartnerCallbackStatus>(request.PartnerCallbackStatus, true, out var partnerCallbackStatus))
                query = query.Where(x => x.PartnerCallbackStatus == partnerCallbackStatus);
            if (!string.IsNullOrWhiteSpace(request.TransactionID))
                query = query.Where(x => x.ProviderTransactionID == request.TransactionID);
            if (!string.IsNullOrWhiteSpace(request.ClientRequestID))
                query = query.Where(x => x.ClientRequestID == request.ClientRequestID);
            if (!string.IsNullOrWhiteSpace(request.AccountNumber))
                query = query.Where(x => x.AccountNumber == request.AccountNumber);
            if (request.FromDate.HasValue)
                query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
            if (request.ToDate.HasValue)
                query = query.Where(x => x.CreatedAt < request.ToDate.Value.AddDays(1));
            if (request.MinAmount.HasValue)
                query = query.Where(x => x.Amount >= request.MinAmount.Value);
            if (request.MaxAmount.HasValue)
                query = query.Where(x => x.Amount <= request.MaxAmount.Value);

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PaymentHistoryItem
                {
                    ID = x.ID,
                    PartnerID = x.PartnerID,
                    PartnerCode = x.Partner == null ? string.Empty : x.Partner.Code,
                    Provider = x.Provider,
                    PaymentMethod = x.PaymentMethod,
                    TransactionID = x.ProviderTransactionID,
                    ReferenceID = x.ProviderReferenceID,
                    ClientRequestID = x.ClientRequestID,
                    AccountNumber = x.AccountNumber,
                    BankID = x.BankID,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    BillNumber = x.BillNumber,
                    Description = x.Description,
                    QRType = x.QRType,
                    CustomerName = x.CustomerName ?? string.Empty,
                    Status = x.Status.ToString(),
                    ProviderStatus = x.ProviderStatus,
                    PartnerCallbackStatus = x.PartnerCallbackStatus.ToString(),
                    PartnerCallbackHttpStatus = x.PartnerCallbackHttpStatus,
                    PartnerCallbackCode = x.PartnerCallbackCode,
                    PartnerCallbackMessage = x.PartnerCallbackMessage,
                    PartnerCallbackAt = x.PartnerCallbackAt,
                    CreatedAt = x.CreatedAt,
                    PaidAt = x.PaidAt,
                    LastNotificationAt = x.LastNotificationAt
                })
                .ToListAsync(cancellationToken);

            return new PaymentHistoryResponse
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }
}
