using System.Text.Json;
using WeddingInvitation.Infrastructure.ConnectionStrings;

// 连接串取串/解密验证工具：
//   1) 离线验证解密算法：ConnProbe --payload <base64密文> --key <base64密钥>
//   2) 端到端验证取串：  ConnProbe [--appsettings <appsettings.json 路径>]
// 默认读取 src/WeddingInvitation.Api/appsettings.json。

var arguments = ParseArgs(args);

var payloadArgument = arguments.TryGetValue("payload", out var payloadArg) ? payloadArg : null;
if (arguments.TryGetValue("payload-file", out var payloadFile) && !string.IsNullOrWhiteSpace(payloadFile))
{
    payloadArgument = File.ReadAllText(payloadFile!).Trim();
}

if (!string.IsNullOrWhiteSpace(payloadArgument))
{
    if (!arguments.TryGetValue("key", out var key) || string.IsNullOrWhiteSpace(key))
    {
        Console.Error.WriteLine("缺少 --key（Base64 的 32 字节密钥）");
        return 2;
    }

    try
    {
        var decrypted = PostgreSqlConnectionStringResolver.DecryptAesGcm(payloadArgument!, key!);
        Console.WriteLine(decrypted);
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"解密失败：{ex.Message}");
        return 1;
    }
}

var appSettingsPath = arguments.TryGetValue("appsettings", out var path) && !string.IsNullOrWhiteSpace(path)
    ? Path.GetFullPath(path!)
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "WeddingInvitation.Api", "appsettings.json"));

if (!File.Exists(appSettingsPath))
{
    Console.Error.WriteLine($"找不到配置文件：{appSettingsPath}");
    return 2;
}

using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
var root = document.RootElement;
var endpoint = root.GetProperty("PostgreSql").GetProperty("ConnectionStringEndpoint").GetString() ?? string.Empty;
var deliveryKey = root.GetProperty("Security").GetProperty("DeliveryKey").GetString() ?? string.Empty;

Console.WriteLine($"配置文件：{appSettingsPath}");
Console.WriteLine($"取串接口：{endpoint}");

using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
try
{
    using var response = await httpClient.GetAsync(endpoint);
    var body = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"HTTP {(int)response.StatusCode}");
    using var payloadDocument = JsonDocument.Parse(body);
    var data = payloadDocument.RootElement.GetProperty("data");
    var cipher = data.GetProperty("connectionStringEncrypted").GetString() ?? string.Empty;
    var algorithm = data.GetProperty("algorithm").GetString() ?? string.Empty;
    Console.WriteLine($"算法：{algorithm}（期望 {PostgreSqlConnectionStringResolver.Algorithm}）");
    var decrypted = PostgreSqlConnectionStringResolver.DecryptAesGcm(cipher, deliveryKey);
    Console.WriteLine("解密结果：");
    Console.WriteLine(MaskPassword(decrypted));
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"取串/解密失败：{ex.GetType().Name}: {ex.Message}");
    return 1;
}

static Dictionary<string, string?> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var name = args[i][2..];
        var value = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal) ? args[++i] : "true";
        result[name] = value;
    }

    return result;
}

static string MaskPassword(string connectionString)
{
    var parts = connectionString.Split(';');
    for (var i = 0; i < parts.Length; i++)
    {
        if (parts[i].TrimStart().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
        {
            var prefix = parts[i][..(parts[i].IndexOf('=') + 1)];
            parts[i] = prefix + "******";
        }
    }

    return string.Join(';', parts);
}
