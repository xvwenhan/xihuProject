namespace Shared.Enums
{
    /// <summary>
    /// 错误代码枚举，用于标识具体的错误情况。
    /// </summary>
    public enum ErrorCode
    {
        None = 0,                 // 无错误（操作成功）
        Unauthorized = 401,       // 未授权
        Forbidden = 403,          // 禁止访问
        NotFound = 404,           // 未找到资源
        ValidationError = 422,    // 数据验证失败
        InternalServerError = 500 // 服务器内部错误
    }
}