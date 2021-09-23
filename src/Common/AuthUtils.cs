using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Yc;

public static class AuthUtils {

    public static async Task<ICredentialsProvider> MakeCredentialsFromEnv(
        bool fallbackAnonymous = false,
        ILoggerFactory? loggerFactory = null)
    {
        var saFileValue = Environment.GetEnvironmentVariable("YDB_SERVICE_ACCOUNT_KEY_FILE_CREDENTIALS");
        if (!string.IsNullOrEmpty(saFileValue)) {
            var saProvider = new ServiceAccountProvider(
                saFilePath: saFileValue,
                loggerFactory: loggerFactory);
            await saProvider.Initialize();
            return saProvider;
        }

        var anonymousValue = Environment.GetEnvironmentVariable("YDB_ANONYMOUS_CREDENTIALS");
        if (anonymousValue != null && IsTrueValue(anonymousValue)) {
            return new AnonymousProvider();
        }

        var tokenValue = Environment.GetEnvironmentVariable("YDB_ACCESS_TOKEN_CREDENTIALS");
        if (!string.IsNullOrEmpty(tokenValue)) {
            return new TokenProvider(tokenValue);
        }

        if (fallbackAnonymous) {
            return new AnonymousProvider();
        }

        throw new InvalidOperationException("Failed to parse credentials from environmet, no valid options found.");
    }

    public static X509Certificate? GetCustomServerCertificate() {
        return YcCerts.GetDefaultServerCertificate();
    }

    private static bool IsTrueValue(string value) {
        return
            value == "1" ||
            value.ToLower() == "yes" ||
            value.ToLower() == "true";

    }
}
