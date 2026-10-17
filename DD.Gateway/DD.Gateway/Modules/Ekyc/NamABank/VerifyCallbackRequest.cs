using System.ComponentModel.DataAnnotations;

namespace DD.Gateway.Modules.Ekyc.NamABank;

public sealed class VerifyCallbackRequest
{
    [Required]
    public string Result { get; init; } = string.Empty;
}

public sealed record VerifiedCallbackResult(
    string SessionId,
    string Status,
    string Code,
    string? MessageVi,
    string? MessageEn);
