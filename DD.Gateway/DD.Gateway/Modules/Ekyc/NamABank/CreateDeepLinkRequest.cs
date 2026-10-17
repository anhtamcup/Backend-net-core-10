using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DD.Gateway.Modules.Ekyc.NamABank;

public sealed class CreateDeepLinkRequest
{
    [Required]
    [JsonPropertyName("partnerID")]
    public string PartnerId { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("callbackUrl")]
    public string CallbackUrl { get; init; } = string.Empty;
}
