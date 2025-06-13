using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using VideoService.Data;
using VideoService.DTOs;
using VideoService.Models;
using System.Text.Json;
using System.Linq;

namespace VideoService.Services
{
    public class VideoService : IVideoService
    {
        private readonly VideoDbContext _context;
        private readonly string _uploadPath;
        private readonly string _tempPath;
        private readonly string _ffmpegPath;
        private readonly string _pythonPath;
        private readonly string _asrScriptPath;
        private readonly ILogger<VideoService> _logger;

        public VideoService(VideoDbContext context, IConfiguration configuration, ILogger<VideoService> logger)
        {
            _context = context;
            _uploadPath = configuration["VideoSettings:UploadPath"] ?? throw new ArgumentNullException(nameof(configuration), "UploadPath is not configured");
            _tempPath = configuration["VideoSettings:TempPath"] ?? throw new ArgumentNullException(nameof(configuration), "TempPath is not configured");
            _ffmpegPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "ffmpeg.exe"));
            _pythonPath = configuration["VideoSettings:PythonPath"] ?? throw new ArgumentNullException(nameof(configuration), "PythonPath is not configured");
            _asrScriptPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonScripts", "asr.py"));
            _logger = logger;
            
            // 确保目录存在
            Directory.CreateDirectory(_uploadPath);
            Directory.CreateDirectory(_tempPath);
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools")));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonScripts")));

            // 记录路径信息
            _logger.LogInformation("FFmpeg path: {FFmpegPath}", _ffmpegPath);
            _logger.LogInformation("Python path: {PythonPath}", _pythonPath);
            _logger.LogInformation("ASR script path: {AsrScriptPath}", _asrScriptPath);
            _logger.LogInformation("FFmpeg exists: {FFmpegExists}", File.Exists(_ffmpegPath));
            _logger.LogInformation("ASR script exists: {AsrScriptExists}", File.Exists(_asrScriptPath));

            // 清理临时文件
            CleanupTempFiles();
        }

        private void CleanupTempFiles()
        {
            try
            {
                if (Directory.Exists(_tempPath))
                {
                    var files = Directory.GetFiles(_tempPath);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            _logger.LogInformation("Cleaned up temp file: {File}", file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete temp file: {File}", file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during temp files cleanup");
            }
        }

        public async Task<VideoDTO> UploadVideoAsync(VideoUploadDTO uploadDto, int userId)
        {
            try
            {
                // 验证文件
                if (uploadDto.VideoFile == null || uploadDto.VideoFile.Length == 0)
                {
                    throw new ArgumentException("No file uploaded");
                }

                // 验证文件类型
                var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
                var fileExtension = Path.GetExtension(uploadDto.VideoFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException($"不支持的文件类型: {fileExtension}");
                }

                // 生成唯一文件名
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                _logger.LogInformation($"生成的文件名: {fileName}");

                // 确保上传目录存在
                if (!Directory.Exists(_uploadPath))
                {
                    Directory.CreateDirectory(_uploadPath);
                    _logger.LogInformation($"创建上传目录: {_uploadPath}");
                }

                // 保存文件
                var filePath = Path.Combine(_uploadPath, fileName);
                _logger.LogInformation($"保存文件到: {filePath}");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadDto.VideoFile.CopyToAsync(stream);
                }

                // 创建视频记录
                var video = new Video
                {
                    Title = uploadDto.Title,
                    UserId = userId,
                    UploadTime = DateTime.UtcNow,
                    FilePath = fileName,  // 只保存文件名，不包含完整路径
                    FileSize = uploadDto.VideoFile.Length,
                    Format = fileExtension.TrimStart('.'),
                    Status = "Pending"
                };

                _context.Videos.Add(video);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"视频上传成功: ID={video.Id}, 文件名={fileName}");

                return new VideoDTO
                {
                    Id = video.Id,
                    Title = video.Title,
                    UploadTime = video.UploadTime,
                    Status = video.Status,
                    FileSize = video.FileSize,
                    Format = video.Format
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "视频上传失败");
                throw;
            }
        }

        public async Task<VideoDTO> GetVideoAsync(int videoId)
        {
            var video = await _context.Videos
                .Select(v => new VideoDTO
                {
                    Id = v.Id,
                    Title = v.Title,
                    Transcription = v.Transcription,
                    Status = v.Status,
                    UploadTime = v.UploadTime,
                    Duration = v.Duration,
                    FileSize = v.FileSize,
                    Format = v.Format,
                    ProcessingProgress = v.ProcessingProgress,
                    ErrorMessage = v.ErrorMessage,
                    LastProcessedTime = v.LastProcessedTime
                })
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                throw new Exception("Video not found");

            return video;
        }

        public async Task<string> ProcessVideoTranscriptionAsync(int videoId)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video == null)
            {
                throw new Exception("Video not found");
            }

            try
            {
                // 更新视频状态为处理中
                video.Status = "Processing";
                await _context.SaveChangesAsync();

                // 获取视频文件路径
                var videoPath = Path.Combine(_uploadPath, video.FilePath);
                _logger.LogInformation($"视频文件路径: {videoPath}");

                if (!File.Exists(videoPath))
                {
                    _logger.LogError($"视频文件不存在: {videoPath}");
                    throw new FileNotFoundException($"视频文件不存在: {videoPath}");
                }

                // 生成临时音频文件路径
                var audioPath = Path.Combine(_tempPath, $"temp_{videoId}.wav");
                _logger.LogInformation($"临时音频文件路径: {audioPath}");

                // 转换视频为音频
                await ConvertVideoToAudioAsync(videoPath, audioPath);

                // 执行语音识别
                var transcription = await ExecuteSpeechRecognitionAsync(audioPath);

                // 更新视频状态和转写结果
                video.Status = "Completed";
                // 只保存最终的转写文本，不包含任何日志信息
                video.Transcription = transcription.Split('\n').Last().Trim();
                video.LastProcessedTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // 清理临时文件
                if (File.Exists(audioPath))
                {
                    File.Delete(audioPath);
                }

                return transcription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "视频处理失败");
                video.Status = "Failed";
                video.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                throw;
            }
        }

        private async Task<string> ExecuteSpeechRecognitionAsync(string audioPath)
        {
            try
            {
                _logger.LogInformation("Starting speech recognition for audio file: {AudioPath}", audioPath);
                
                // 调用Python ASR脚本
                var transcription = await RunAsrScriptAsync(audioPath);
                
                _logger.LogInformation("Speech recognition completed successfully");
                return transcription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Speech recognition failed");
                throw new Exception($"Speech recognition failed: {ex.Message}");
            }
        }

        private async Task<string> RunAsrScriptAsync(string audioPath)
        {
            try
            {
                _logger.LogInformation($"开始执行ASR脚本，音频文件路径: {audioPath}");
                
                // 检查文件是否存在
                if (!File.Exists(audioPath))
                {
                    _logger.LogError($"音频文件不存在: {audioPath}");
                    throw new FileNotFoundException($"音频文件不存在: {audioPath}");
                }

                // 检查文件大小
                var fileInfo = new FileInfo(audioPath);
                _logger.LogInformation($"音频文件大小: {fileInfo.Length / 1024.0:F2}KB");

                // 检查Python路径
                if (!File.Exists(_pythonPath))
                {
                    _logger.LogError($"Python解释器不存在: {_pythonPath}");
                    throw new FileNotFoundException($"Python解释器不存在: {_pythonPath}");
                }

                // 检查ASR脚本路径
                if (!File.Exists(_asrScriptPath))
                {
                    _logger.LogError($"ASR脚本不存在: {_asrScriptPath}");
                    throw new FileNotFoundException($"ASR脚本不存在: {_asrScriptPath}");
                }

                // 检查ffmpeg路径
                if (!File.Exists(_ffmpegPath))
                {
                    _logger.LogError($"FFmpeg不存在: {_ffmpegPath}");
                    throw new FileNotFoundException($"FFmpeg不存在: {_ffmpegPath}");
                }

                // 获取音频文件的完整路径
                var fullAudioPath = Path.GetFullPath(audioPath);
                _logger.LogInformation($"音频文件完整路径: {fullAudioPath}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _pythonPath,
                        Arguments = $"\"{_asrScriptPath}\" \"{fullAudioPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_asrScriptPath),
                        EnvironmentVariables =
                        {
                            ["PATH"] = $"{Path.GetDirectoryName(_ffmpegPath)}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}",
                            ["PYTHONIOENCODING"] = "utf-8",
                            ["PYTHONUNBUFFERED"] = "1"
                        }
                    }
                };

                _logger.LogInformation($"执行命令: {_pythonPath} \"{_asrScriptPath}\" \"{fullAudioPath}\"");
                _logger.LogInformation($"工作目录: {process.StartInfo.WorkingDirectory}");
                _logger.LogInformation($"环境变量PATH: {process.StartInfo.EnvironmentVariables["PATH"]}");

                process.Start();
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                _logger.LogInformation($"ASR脚本输出: {output}");
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError($"ASR脚本错误: {error}");
                }

                if (process.ExitCode != 0)
                {
                    var errorMessage = $"ASR脚本执行失败，退出码: {process.ExitCode}";
                    if (!string.IsNullOrEmpty(error))
                    {
                        errorMessage += $"\n错误信息: {error}";
                    }
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                if (string.IsNullOrWhiteSpace(output))
                {
                    var errorMessage = "ASR脚本执行成功但没有输出结果";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                _logger.LogInformation($"ASR脚本执行成功，结果: {output.Trim()}");
                return output.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"执行ASR脚本失败: {audioPath}");
                throw new Exception($"Failed to run ASR script: {ex.Message}", ex);
            }
        }

        private async Task ConvertVideoToAudioAsync(string videoPath, string audioPath)
        {
            try
            {
                if (!File.Exists(_ffmpegPath))
                {
                    throw new Exception("FFmpeg not found. Please ensure ffmpeg.exe is present in the Tools directory.");
                }

                _logger.LogInformation("Converting video to audio: {VideoPath} -> {AudioPath}", videoPath, audioPath);

                // 构建FFmpeg命令，使用百度语音识别推荐的参数
                var ffmpegArgs = new[]
                {
                    "-i", videoPath,
                    "-vn",                     // 不处理视频
                    "-acodec", "pcm_s16le",    // 使用PCM编码
                    "-ar", "16000",            // 采样率16kHz（百度语音识别要求）
                    "-ac", "1",                // 单声道（百度语音识别要求）
                    "-af", string.Join(",", new[]
                    {
                        "aresample=16000",     // 重采样到16kHz
                        "asetrate=16000",      // 设置采样率
                        "volume=2.0"           // 增加音量
                    }),
                    "-y",                      // 覆盖输出文件
                    audioPath
                };

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = string.Join(" ", ffmpegArgs),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("FFmpeg conversion failed: {Error}", error);
                    throw new Exception($"FFmpeg conversion failed: {error}");
                }

                // 验证生成的音频文件
                if (!File.Exists(audioPath))
                {
                    throw new Exception("Audio file was not created");
                }

                var fileInfo = new FileInfo(audioPath);
                if (fileInfo.Length == 0)
                {
                    throw new Exception("Generated audio file is empty");
                }

                _logger.LogInformation("Audio extraction completed successfully. File size: {Size}MB", 
                    fileInfo.Length / (1024.0 * 1024.0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert video to audio");
                throw new Exception($"Failed to convert video to audio: {ex.Message}");
            }
        }

        public async Task<List<VideoDTO>> GetUserVideosAsync(int userId)
        {
            try
            {
                var videos = await _context.Videos
                    .Where(v => v.UserId == userId)
                    .OrderByDescending(v => v.UploadTime)
                    .Select(v => new VideoDTO
                    {
                        Id = v.Id,
                        Title = v.Title,
                        Description = v.Description,
                        UploadTime = v.UploadTime,
                        Status = v.Status,
                        Duration = v.Duration,
                        FileSize = v.FileSize,
                        Format = v.Format,
                        ProcessingProgress = v.ProcessingProgress,
                        ErrorMessage = v.ErrorMessage,
                        LastProcessedTime = v.LastProcessedTime,
                        Transcription = v.Transcription
                    })
                    .ToListAsync();

                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户视频列表失败");
                throw;
            }
        }

        public async Task<VideoProcessingStatusDTO> GetVideoStatusAsync(int videoId)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video == null)
                throw new Exception("Video not found");

            return new VideoProcessingStatusDTO
            {
                Id = video.Id,
                Status = video.Status,
                ProcessingProgress = video.ProcessingProgress,
                ErrorMessage = video.ErrorMessage,
                LastProcessedTime = video.LastProcessedTime
            };
        }

        public async Task<bool> DeleteVideoAsync(int videoId, int userId)
        {
            try
            {
                var video = await _context.Videos
                    .FirstOrDefaultAsync(v => v.Id == videoId && v.UserId == userId);

                if (video == null)
                {
                    throw new Exception("视频不存在或无权限删除");
                }

                // 删除物理文件
                var filePath = Path.Combine(_uploadPath, video.FilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"已删除视频文件: {filePath}");
                }

                // 从数据库中删除记录
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"已删除视频记录: ID={videoId}, 用户ID={userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除视频失败");
                throw;
            }
        }
    }
} 