using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VideoService.DTOs
{
    /// <summary>
    /// 视频数据传输对象
    /// </summary>
    public class VideoDTO
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 视频标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 视频描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 视频转写文本
        /// </summary>
        public string? Transcription { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 视频时长
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件格式
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// 处理进度（0-100）
        /// </summary>
        public int ProcessingProgress { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 最后处理时间
        /// </summary>
        public DateTime? LastProcessedTime { get; set; }
    }

    /// <summary>
    /// 视频上传数据传输对象
    /// </summary>
    public class VideoUploadDTO
    {
        /// <summary>
        /// 视频标题
        /// </summary>
        [Required(ErrorMessage = "标题不能为空")]
        [MaxLength(200, ErrorMessage = "标题长度不能超过200个字符")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 视频文件
        /// </summary>
        [Required(ErrorMessage = "请选择视频文件")]
        public IFormFile VideoFile { get; set; } = null!;
    }

    /// <summary>
    /// 视频处理状态数据传输对象
    /// </summary>
    public class VideoProcessingStatusDTO
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 处理进度（0-100）
        /// </summary>
        [Range(0, 100)]
        public int ProcessingProgress { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 最后处理时间
        /// </summary>
        public DateTime? LastProcessedTime { get; set; }
    }

    public class VideoSummaryResponse
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string OriginalText { get; set; }
        public string Summary { get; set; }
    }
} 