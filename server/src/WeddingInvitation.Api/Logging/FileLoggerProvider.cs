namespace WeddingInvitation.Api.Logging;

/// <summary>
/// 轻量文件日志 Provider：按天写 logs/api-yyyyMMdd.log。
/// 之所以不用 Serilog，是为了在离线/无外网环境下不引入额外 NuGet 依赖。
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _directory;
    private readonly LogLevel _minimumLevel;
    private readonly object _sync = new();
    private readonly bool _enabled;

    public FileLoggerProvider(string directory, LogLevel minimumLevel = LogLevel.Information)
    {
        _directory = directory;
        _minimumLevel = minimumLevel;
        try
        {
            Directory.CreateDirectory(_directory);
            _enabled = true;
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            _enabled = false;
        }
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    public void Dispose()
    {
    }

    private void Write(string categoryName, LogLevel level, string message, Exception? exception)
    {
        if (!_enabled)
        {
            return;
        }

        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {categoryName} - {message}";
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        var path = Path.Combine(_directory, $"api-{DateTime.Now:yyyyMMdd}.log");
        lock (_sync)
        {
            try
            {
                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch (Exception ex) when (IsFileSystemException(ex))
            {
                // 写日志失败不影响主流程。
            }
        }
    }

    private static bool IsFileSystemException(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or System.Security.SecurityException;

    private sealed class FileLogger : ILogger
    {
        private readonly FileLoggerProvider _provider;
        private readonly string _categoryName;

        public FileLogger(FileLoggerProvider provider, string categoryName)
        {
            _provider = provider;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _provider._minimumLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            _provider.Write(_categoryName, logLevel, formatter(state, exception), exception);
        }
    }
}
