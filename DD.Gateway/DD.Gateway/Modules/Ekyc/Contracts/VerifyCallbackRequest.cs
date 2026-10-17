using System.ComponentModel.DataAnnotations;

namespace DD.Gateway.Modules.Ekyc.Contracts;

public sealed class VerifyCallbackRequest
{
    [Required]
    public string Provider { get; init; } = "NAMABANK";

    [Required]
    public string Result { get; init; } = string.Empty;
}

