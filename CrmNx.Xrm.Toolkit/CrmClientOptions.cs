using System;
using System.Linq;

namespace CrmNx.Xrm.Toolkit
{
    public class CrmClientOptions
    {
        /// <summary>
        ///     ConnectionString name from appsettins
        /// </summary>
        public const string DefaultConnectionStringName = "Crm";

        /// <summary>
        ///     WebApi version
        /// </summary>
        public const string DefaultApiVersion = "v8.2";

        /// <summary>
        ///     Authentication type on AD auth
        /// </summary>
        public const string DefaultAuthType = "NTLM";

        public const int DefaultHandlerLifeTimeMinutes = 9;

        public CrmClientOptions(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            ConnectionString = connectionString;
        }

        public CrmClientOptions()
        {
        }

        public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(DefaultHandlerLifeTimeMinutes);

        public string ConnectionString { get; set; }

        public bool IgnoreSSLErrors { get; set; }

        public string ApiVersion { get; set; } = DefaultApiVersion;

        public string AuthType { get; set; } = DefaultAuthType;

        public bool UseDefaultCredentials
        {
            get
            {
                return string.IsNullOrEmpty(Username);
            }
        }

        private string _userName;
        public string Username
        {
            get
            {
                if (!string.IsNullOrEmpty(_userName))
                {
                    return _userName;
                }

                return GetParameterValueFromConnectionString(ConnectionString, "Username");
            }

            set
            {
                _userName = value;
            }
        }

        private string _password;

        public string Password
        {
            get
            {
                if (!string.IsNullOrEmpty(_password))
                {
                    return _password;
                }

                return GetParameterValueFromConnectionString(ConnectionString, "Password");
            }

            set
            {
                _password = value;
            }
        }

        public string Domain => GetParameterValueFromConnectionString(ConnectionString, "Domain");

        public Uri BaseAddress
        {
            get
            {
                var url = GetParameterValueFromConnectionString(ConnectionString, "Url");

                return new Uri($"{url}/api/data/{ApiVersion}/");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static string GetParameterValueFromConnectionString(string connectionString, string parameter)
        {
            _ = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            try
            {
                return connectionString
                    .Split(';')
                    .FirstOrDefault(s => s.Trim().StartsWith(parameter, StringComparison.InvariantCultureIgnoreCase))
                    ?.Split('=')[1];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}