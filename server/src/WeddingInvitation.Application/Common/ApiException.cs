namespace WeddingInvitation.Application.Common;

/// <summary>业务异常，由异常中间件统一转换为 { code, message } 响应。</summary>
public class ApiException : Exception
{
    public ApiException(string message, int code = 400) : base(message)
    {
        Code = code;
    }

    public int Code { get; }

    public static ApiException NotFound(string message = "资源不存在") => new(message, 404);

    public static ApiException Unauthorized(string message = "未授权") => new(message, 401);

    public static ApiException Forbidden(string message = "没有权限") => new(message, 403);
}
