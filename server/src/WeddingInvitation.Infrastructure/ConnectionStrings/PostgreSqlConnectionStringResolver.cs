using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using WeddingInvitation.Application.Options;

namespace WeddingInvitation.Infrastructure.ConnectionStrings;

/// <summary>
/// 从远程取串接口获取加密的 PostgreSQL 连接串，并按接口描述解密：
/// 算法 AES-256-GCM（密钥 32 字节，来自 Security:DeliveryKey，Base64），
/// 载荷 Base64(nonce[12] + ciphertext + tag[16])。
/// </summary>
public static class PostgreSqlConnectionStringResolver
{
    public const string Algorithm = "AES-256-GCM";

    /// <summary>
    /// 解析连接串：优先远程取串解密，失败时可回退到本地 Base64(两层) 连接串。
    /// </summary>
    public static async Task<string> ResolveAsync(
        IConfiguration configuration,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        var options = configuration.GetSection(PostgreSqlOptions.SectionName).Get<PostgreSqlOptions>() ?? new PostgreSqlOptions();

        if (options.UseRemote && !string.IsNullOrWhiteSpace(options.ConnectionStringEndpoint))
        {
            try
            {
                return await ResolveRemoteAsync(configuration, httpClient, options.ConnectionStringEndpoint, options.ConnectionName, cancellationToken);
            }
            catch (Exception ex) when (HasLocalConnectionString(configuration))
            {
                Console.WriteLine($"[取串] 远程取串失败（{ex.Message}），回退本地连接串。");
                return ResolveLocal(configuration);
            }
        }

        return ResolveLocal(configuration);
    }

    private static async Task<string> ResolveRemoteAsync(
        IConfiguration configuration,
        HttpClient httpClient,
        string endpoint,
        string? connectionName,
        CancellationToken cancellationToken)
    {
        var requestUri = BuildEndpoint(endpoint, connectionName);
        var response = await httpClient.GetFromJsonAsync<ConnectionStringResponse>(requestUri, cancellationToken)
            ?? throw new InvalidOperationException("取串接口未返回内容。");

        if (response.Code != 0)
        {
            throw new InvalidOperationException($"取串接口返回失败：{response.Message}");
        }

        if (!string.Equals(response.Data.Algorithm, Algorithm, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"取串接口返回了不支持的算法：{response.Data.Algorithm}");
        }

        var key = configuration["Security:DeliveryKey"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("未配置 Security:DeliveryKey，无法解密连接串。");
        }

        var connectionString = DecryptAesGcm(response.Data.ConnectionStringEncrypted, key);
        Console.WriteLine($"[取串] 已从 {requestUri} 获取并解密连接串（数据库 {response.Data.DbType}）。");
        return connectionString;
    }

    private static string BuildEndpoint(string endpoint, string? connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName) ||
            endpoint.Contains("connName=", StringComparison.OrdinalIgnoreCase))
        {
            return endpoint;
        }

        var separator = endpoint.Contains('?') ? '&' : '?';
        return $"{endpoint}{separator}connName={Uri.EscapeDataString(connectionName.Trim())}";
    }

    private static bool HasLocalConnectionString(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration["PostgreSql:ConnectionString"]);

    private static string ResolveLocal(IConfiguration configuration)
    {
        var raw = configuration["PostgreSql:ConnectionString"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException("未配置 PostgreSql:ConnectionStringEndpoint 或 PostgreSql:ConnectionString。");
        }

        return NormalizeConnectionString(DecodeBase64Twice(raw));
    }

    /// <summary>按接口描述解密连接串密文。</summary>
    public static string DecryptAesGcm(string payloadBase64, string keyBase64)
    {
        if (string.IsNullOrWhiteSpace(payloadBase64))
        {
            throw new InvalidOperationException("连接串密文为空。");
        }

        var key = Convert.FromBase64String(keyBase64);
        if (key.Length != 32)
        {
            throw new InvalidOperationException("Security:DeliveryKey 必须是 Base64 编码的 32 字节密钥。");
        }

        var payload = Convert.FromBase64String(payloadBase64);
        if (payload.Length <= 28)
        {
            throw new InvalidOperationException("连接串密文长度非法。");
        }

        var nonce = payload[..12];
        var cipher = payload[12..^16];
        var tag = payload[^16..];
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(key, 16);
        aes.Decrypt(nonce, cipher, tag, plain);
        return NormalizeConnectionString(Encoding.UTF8.GetString(plain));
    }

    private static string NormalizeConnectionString(string value) =>
        value
            .Replace("SSL Model=Disabled", "SSL Mode=Disable", StringComparison.OrdinalIgnoreCase)
            .Replace("SSL Mode=Disabled", "SSL Mode=Disable", StringComparison.OrdinalIgnoreCase);

    private static string DecodeBase64Twice(string value)
    {
        try
        {
            var first = Encoding.UTF8.GetString(Convert.FromBase64String(value.Trim()));
            return Encoding.UTF8.GetString(Convert.FromBase64String(first.Trim()));
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("PostgreSql:ConnectionString 必须是两层 Base64 编码的连接串。", ex);
        }
    }

    private sealed class ConnectionStringResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("data")]
        public ConnectionStringData Data { get; init; } = new();
    }

    private sealed class ConnectionStringData
    {
        [JsonPropertyName("connName")]
        public string ConnName { get; init; } = string.Empty;

        [JsonPropertyName("dbType")]
        public string DbType { get; init; } = string.Empty;

        [JsonPropertyName("connectionStringEncrypted")]
        public string ConnectionStringEncrypted { get; init; } = string.Empty;

        [JsonPropertyName("algorithm")]
        public string Algorithm { get; init; } = string.Empty;
    }
}
