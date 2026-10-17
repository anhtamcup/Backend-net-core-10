using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DD.Gateway.Modules.Ekyc.Contracts;

public sealed class CreateDeepLinkRequest
{
    [Required]
    [JsonPropertyName("provider")]
    public string Provider { get; init; } = "NAMABANK";

    [Required]
    [JsonPropertyName("partnerID")]
    public string PartnerId { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("callbackUrl")]
    public string CallbackUrl { get; init; } = string.Empty;
}
