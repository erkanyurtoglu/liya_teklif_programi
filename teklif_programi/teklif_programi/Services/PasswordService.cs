using System;
using System.Configuration;

namespace teklif_programi.Services
{
    /// <summary>
    /// Provides centralized password validation to remove duplicated hard-coded
    /// checks across the application. The expected password can be supplied via
    /// the <c>LIYA_APP_PASSWORD</c> environment variable or App.config.
    /// </summary>
    public static class PasswordService
    {
        private static readonly string? _configuredPassword;

        static PasswordService()
        {
            // Environment variable takes precedence to avoid storing
            // secrets in configuration files.
            _configuredPassword = Environment.GetEnvironmentVariable("LIYA_APP_PASSWORD");
            if (string.IsNullOrEmpty(_configuredPassword))
            {
                _configuredPassword = ConfigurationManager.AppSettings["AdminPassword"];
            }
        }

        /// <summary>
        /// Validates the user supplied password against the configured secret.
        /// </summary>
        /// <param name="enteredPassword">Password provided by the user.</param>
        /// <returns><c>true</c> if the password matches; otherwise <c>false</c>.</returns>
        public static bool Verify(string? enteredPassword)
            => !string.IsNullOrEmpty(enteredPassword)
               && !string.IsNullOrEmpty(_configuredPassword)
               && enteredPassword == _configuredPassword;
    }
}