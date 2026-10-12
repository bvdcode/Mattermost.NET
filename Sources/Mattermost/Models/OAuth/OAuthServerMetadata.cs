using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.OAuth
{
    /// <summary>Public OAuth authorization-server discovery metadata.</summary>
    public class OAuthServerMetadata
    {
        /// <summary>Configured issuer URL, including any server subpath.</summary>
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;

        /// <summary>Authorization endpoint URL, or an empty string when absent.</summary>
        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; } = string.Empty;

        /// <summary>Token endpoint URL, or an empty string when absent.</summary>
        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; } = string.Empty;

        /// <summary>Dynamic-registration endpoint URL; absent when dynamic registration is disabled.</summary>
        [JsonPropertyName("registration_endpoint")]
        public string RegistrationEndpoint { get; set; } = string.Empty;

        /// <summary>Supported OAuth response-type names.</summary>
        [JsonPropertyName("response_types_supported")]
        public IList<string> ResponseTypesSupported { get; set; } = new List<string>();

        /// <summary>Supported OAuth scope names.</summary>
        [JsonPropertyName("scopes_supported")]
        public IList<string> ScopesSupported { get; set; } = new List<string>();

        /// <summary>Supported OAuth grant-type names.</summary>
        [JsonPropertyName("grant_types_supported")]
        public IList<string> GrantTypesSupported { get; set; } = new List<string>();

        /// <summary>Supported token-endpoint authentication method names.</summary>
        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public IList<string> TokenEndpointAuthMethodsSupported { get; set; } = new List<string>();

        /// <summary>Supported PKCE code-challenge method names.</summary>
        [JsonPropertyName("code_challenge_methods_supported")]
        public IList<string> CodeChallengeMethodsSupported { get; set; } = new List<string>();
    }
}
