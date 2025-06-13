using System.Threading.Tasks;
using VideoService.DTOs;

namespace VideoService.Services
{
    public interface IVideoService
    {
        /// <summary>
        /// 上传视频并进行语音识别
        /// </summary>
        /// <param name="uploadDTO">视频上传信息</param>
        /// <param name="userId">用户ID</param>
        /// <returns>上传结果</returns>
        Task<VideoDTO> UploadVideoAsync(VideoUploadDTO uploadDTO, int userId);

        /// <summary>
        /// 获取视频
        /// </summary>
        /// <param name="videoId">视频ID</param>
        /// <returns>视频信息</returns>
        Task<VideoDTO> GetVideoAsync(int videoId);

        /// <summary>
        /// 处理视频转写
        /// </summary>
        /// <param name="videoId">视频ID</param>
        /// <returns>转写结果</returns>
        Task<string> ProcessVideoTranscriptionAsync(int videoId);

        /// <summary>
        /// 获取视频处理状态
        /// </summary>
        /// <param name="videoId">视频ID</param>
        /// <returns>处理状态</returns>
        Task<VideoProcessingStatusDTO> GetVideoStatusAsync(int videoId);

        /// <summary>
        /// 获取用户视频列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户视频列表</returns>
        Task<List<VideoDTO>> GetUserVideosAsync(int userId);

        /// <summary>
        /// 删除视频
        /// </summary>
        /// <param name="videoId">视频ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteVideoAsync(int videoId, int userId);
    }
} 