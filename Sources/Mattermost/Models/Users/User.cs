﻿using System.Text.Json.Serialization;
using Mattermost.Models;

namespace Mattermost.Models.Users
{
    /// <summary>
    /// User information.
    /// </summary>
    public class User
    {
        /// <summary>
        /// User identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// The time in milliseconds a user was created.
        /// </summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a user was last updated.
        /// </summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a user was deleted.
        /// </summary>
        [JsonPropertyName("delete_at")]
        public int DeletedAt { get; set; }

        /// <summary>
        /// Username.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        /// <summary>
        /// User authentication data.
        /// </summary>
        [JsonPropertyName("auth_data")]
        public string? AuthData { get; set; }

        /// <summary>
        /// User authentication service (ex. Gitlab, GMail etc.)
        /// </summary>
        [JsonPropertyName("auth_service")]
        public string? AuthService { get; set; }

        /// <summary>
        /// E-Mail.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// User nickname.
        /// </summary>
        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        /// <summary>
        /// First name.
        /// </summary>
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        /// <summary>
        /// User position.
        /// </summary>
        [JsonPropertyName("position")]
        public string? Position { get; set; }

        /// <summary>
        /// User roles.
        /// </summary>
        [JsonPropertyName("roles")]
        public string? Roles { get; set; }

        /// <summary>
        /// User notify properties.
        /// </summary>
        [JsonPropertyName("notify_props")]
        public NotifyProps? NotifyProps { get; set; }

        /// <summary>
        /// The time in milliseconds a user password was last updated.
        /// </summary>
        [JsonPropertyName("last_password_update")]
        public long LastPasswordUpdate { get; set; }

        /// <summary>
        /// The time in milliseconds a user picture was last updated.
        /// </summary>
        [JsonPropertyName("last_picture_update")]
        public long LastPictureUpdate { get; set; }

        /// <summary>
        /// User locale (ex. en-US, ru-RU etc.)
        /// </summary>
        [JsonPropertyName("locale")]
        public string Locale { get; set; } = null!;

        /// <summary>
        /// User timezone.
        /// </summary>
        [JsonPropertyName("timezone")]
        public Timezone Timezone { get; set; } = null!;

        /// <summary>
        /// Is bot?
        /// </summary>
        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }

        /// <summary>
        /// If is bot, description here.
        /// </summary>
        [JsonPropertyName("bot_description")]
        public string? BotDescription { get; set; }

        /// <summary>
        /// Disable welcome email for the user.
        /// </summary>
        [JsonPropertyName("disable_welcome_email")]
        public bool DisableWelcomeEmail { get; set; }
    }
}