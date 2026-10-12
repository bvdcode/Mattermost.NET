using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>Generated multi-factor authentication secret and its QR image. Treat both values as credentials.</summary>
    public class MfaSecret
    {
        /// <summary>Base32-encoded TOTP secret, already stored by the server for this account.</summary>
        [JsonPropertyName("secret")]
        public string Secret { get; set; } = string.Empty;

        /// <summary>Base64-encoded PNG containing the authenticator setup URI.</summary>
        [JsonPropertyName("qr_code")]
        public string QRCode { get; set; } = string.Empty;
    }
}
