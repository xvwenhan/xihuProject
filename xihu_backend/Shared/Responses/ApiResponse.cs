namespace Shared.Responses
{
    /// <summary>
    /// 通用 API 响应类，用于统一后端返回的数据结构。
    /// </summary>
    /// <typeparam name="T">响应的数据类型。</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 响应是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 返回的数据本体。
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 错误或信息消息。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 可选的错误代码，用于描述特定的错误情况。
        /// </summary>
        public string ErrorCode { get; set; }
    }

    /// <summary>
    /// 静态工具类，用于快速生成标准化的 API 响应。
    /// </summary>
    public static class ApiResponse
    {
        /// <summary>
        /// 生成一个成功的 API 响应。
        /// </summary>
        /// <typeparam name="T">返回数据的类型。</typeparam>
        /// <param name="data">响应的数据。</param>
        /// <param name="message">可选的提示信息，默认为 "操作成功"。</param>
        /// <returns>标准化的 API 响应。</returns>
        public static ApiResponse<T> Success<T>(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// 生成一个失败的 API 响应。
        /// </summary>
        /// <typeparam name="T">错误时的数据类型（通常为 null）。</typeparam>
        /// <param name="message">错误信息。</param>
        /// <param name="errorCode">可选的错误代码。</param>
        /// <returns>标准化的 API 响应。</returns>
        public static ApiResponse<T> Fail<T>(string message, string errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}