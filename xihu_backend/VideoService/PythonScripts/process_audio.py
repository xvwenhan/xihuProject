import sys
import os
import logging
from pathlib import Path

# 设置 ffmpeg 路径
current_dir = Path(__file__).parent
root_dir = current_dir.parent
ffmpeg_path = root_dir / "Tools" / "ffmpeg.exe"
os.environ["PATH"] = str(root_dir / "Tools") + os.pathsep + os.environ["PATH"]

# 现在导入其他模块
import pydub
import numpy as np

# 配置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

def setup_ffmpeg():
    """设置ffmpeg路径"""
    try:
        logger.debug(f"Current directory: {current_dir}")
        logger.debug(f"Root directory: {root_dir}")
        logger.debug(f"Looking for FFmpeg at: {ffmpeg_path}")
        
        if not ffmpeg_path.exists():
            logger.error(f"FFmpeg not found at: {ffmpeg_path}")
            return False
            
        # 设置ffmpeg路径
        pydub.AudioSegment.converter = str(ffmpeg_path)
        logger.info(f"FFmpeg path set to: {ffmpeg_path}")
        
        # 验证ffmpeg是否可用
        try:
            import subprocess
            result = subprocess.run([str(ffmpeg_path), '-version'], 
                                 capture_output=True, 
                                 text=True)
            logger.info(f"FFmpeg version check: {result.stdout.split('\n')[0]}")
            return True
        except Exception as e:
            logger.error(f"FFmpeg version check failed: {str(e)}")
            return False
            
    except Exception as e:
        logger.error(f"Error setting up FFmpeg: {str(e)}")
        return False

def normalize_audio(audio):
    """标准化音频音量"""
    try:
        # 计算当前音量
        current_rms = audio.rms
        target_rms = -20  # 目标音量（dB）
        
        # 计算需要的增益
        gain = target_rms - current_rms
        logger.info(f"Applying gain: {gain}dB")
        
        # 应用增益
        return audio + gain
    except Exception as e:
        logger.error(f"Error in normalize_audio: {str(e)}")
        return audio

def remove_noise(audio):
    """去除噪音"""
    try:
        # 计算噪音阈值
        samples = np.array(audio.get_array_of_samples())
        noise_threshold = np.std(samples) * 0.1
        
        # 创建静音段
        silence = pydub.silence.detect_silence(
            audio,
            min_silence_len=100,  # 最小静音长度（ms）
            silence_thresh=noise_threshold
        )
        
        # 移除静音段
        if silence:
            logger.info(f"Removing {len(silence)} silence segments")
            return pydub.silence.remove_silence(audio, silence)
        return audio
    except Exception as e:
        logger.error(f"Error in remove_noise: {str(e)}")
        return audio

def enhance_speech(audio):
    """增强语音"""
    try:
        # 转换为单声道
        audio = audio.set_channels(1)
        
        # 设置采样率为16kHz
        audio = audio.set_frame_rate(16000)
        
        # 设置采样宽度为16位
        audio = audio.set_sample_width(2)
        
        # 应用高通滤波器去除低频噪音
        audio = audio.high_pass_filter(100)
        
        # 应用低通滤波器保留语音频率
        audio = audio.low_pass_filter(8000)
        
        logger.info("Speech enhancement completed")
        return audio
    except Exception as e:
        logger.error(f"Error in enhance_speech: {str(e)}")
        return audio

def process_audio(input_path, output_path):
    """处理音频文件"""
    try:
        logger.info(f"Processing audio file: {input_path}")
        
        # 加载音频文件
        audio = pydub.AudioSegment.from_wav(input_path)
        logger.info(f"Audio loaded: {len(audio)}ms, {audio.channels} channels, {audio.frame_rate}Hz")
        
        # 只进行基本的音频处理
        # 1. 确保是单声道
        if audio.channels > 1:
            audio = audio.set_channels(1)
            logger.info("Converted to mono")
            
        # 2. 确保采样率为16kHz
        if audio.frame_rate != 16000:
            audio = audio.set_frame_rate(16000)
            logger.info("Set sample rate to 16kHz")
            
        # 3. 确保采样宽度为16位
        if audio.sample_width != 2:
            audio = audio.set_sample_width(2)
            logger.info("Set sample width to 16-bit")
            
        # 4. 轻微增加音量
        audio = audio + 6  # 增加6dB音量
        logger.info("Increased volume by 6dB")
        
        # 导出处理后的音频
        audio.export(output_path, format="wav")
        logger.info(f"Processed audio saved to: {output_path}")
        
        return True
    except Exception as e:
        logger.error(f"Error processing audio: {str(e)}")
        return False

def main():
    try:
        # 设置ffmpeg路径
        if not setup_ffmpeg():
            logger.error("Failed to setup FFmpeg")
            return 1
            
        # 检查命令行参数
        if len(sys.argv) != 3:
            logger.error("Usage: python process_audio.py <input_path> <output_path>")
            return 1
            
        input_path = sys.argv[1]
        output_path = sys.argv[2]
        
        # 检查输入文件是否存在
        if not os.path.exists(input_path):
            logger.error(f"Input file not found: {input_path}")
            return 1
            
        # 处理音频
        if process_audio(input_path, output_path):
            logger.info("Audio processing completed successfully")
            return 0
        else:
            logger.error("Audio processing failed")
            return 1
            
    except Exception as e:
        logger.error(f"Error in main: {str(e)}")
        return 1

if __name__ == "__main__":
    sys.exit(main()) 