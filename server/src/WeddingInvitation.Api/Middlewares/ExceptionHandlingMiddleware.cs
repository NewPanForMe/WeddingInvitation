using System.Text.Json;
using WeddingInvitation.Application.Common;

namespace WeddingInvitation.Api.Middlewares;

/// <summary>把业务异常转换为统一的 { code, message } 响应。</summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("业务异常 {Path}：{Message}", context.Request.Path, ex.Message);
            await WriteAsync(context, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未处理异常 {Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "服务器内部错误，请稍后再试");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        var payload = JsonSerializer.Serialize(new { code = statusCode, message }, JsonOptions);
        await context.Response.WriteAsync(payload);
    }
}
