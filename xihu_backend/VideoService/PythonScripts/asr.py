import sys
import os
import json
import logging
import time
import wave
import numpy as np
import webrtcvad
from scipy.io import wavfile
from pathlib import Path

# 禁用代理
os.environ['no_proxy'] = '*'
os.environ['NO_PROXY'] = '*'

# 设置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stdout),
        logging.FileHandler('asr.log')
    ]
)
logger = logging.getLogger(__name__)

# VAD 配置
VAD_MODE = 2  # 降低灵敏度，从3改为2，减少过度分段
VAD_PADDING_DURATION = 0.5  # 增加填充时间，从0.3改为0.5秒
SILENCE_THRESHOLD = 0.15  # 略微提高静音阈值
MIN_SEGMENT_DURATION = 1.0  # 最小语音片段长度（秒）
MAX_SEGMENT_DURATION = 15.0  # 最大语音片段长度（秒）

# 设置 ffmpeg 路径
try:
    current_dir = Path(__file__).parent
    root_dir = current_dir.parent
    ffmpeg_path = root_dir / "Tools" / "ffmpeg.exe"
    
    logger.debug(f"Current directory: {current_dir}")
    logger.debug(f"Root directory: {root_dir}")
    logger.debug(f"FFmpeg path: {ffmpeg_path}")
    
    if not ffmpeg_path.exists():
        logger.error(f"FFmpeg not found at: {ffmpeg_path}")
        sys.exit(1)
        
    os.environ["PATH"] = str(root_dir / "Tools") + os.pathsep + os.environ["PATH"]
    logger.info(f"Added FFmpeg to PATH: {root_dir / 'Tools'}")
except Exception as e:
    logger.error(f"Error setting up FFmpeg: {str(e)}")
    sys.exit(1)

# 现在导入其他模块
try:
    from pydub import AudioSegment
    from pydub.utils import make_chunks
    from aip import AipSpeech
    import requests
    import urllib3
    import chardet
    import speech_recognition as sr
    logger.info("Successfully imported all required modules")
except Exception as e:
    logger.error(f"Error importing modules: {str(e)}")
    sys.exit(1)

# 禁用 SSL 警告
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# 百度语音识别配置
APP_ID = '118302592'
API_KEY = 'XWeMgOYl072od8G2fQVUzSmA'
SECRET_KEY = 'H78r2YcWPMESpD6dTK3SbpxsTNa6qnHE'

# 百度API限制
MAX_AUDIO_LENGTH = 30  # 最大音频长度（秒）
MAX_AUDIO_SIZE = 10 * 1024 * 1024  # 最大音频大小（10MB）

# 创建百度语音识别客户端
try:
    client = AipSpeech(APP_ID, API_KEY, SECRET_KEY)
    logger.info("Successfully created Baidu Speech client")
except Exception as e:
    logger.error(f"Error creating Baidu Speech client: {str(e)}")
    sys.exit(1)

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
        AudioSegment.converter = str(ffmpeg_path)
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

def detect_speech_segments(audio_path):
    """使用 VAD 检测语音片段"""
    try:
        # 创建 VAD 对象
        vad = webrtcvad.Vad(VAD_MODE)
        
        # 读取音频文件
        with wave.open(audio_path, 'rb') as wf:
            sample_rate = wf.getframerate()
            pcm_data = wf.readframes(wf.getnframes())
            
        # 将音频数据转换为 numpy 数组
        audio_data = np.frombuffer(pcm_data, dtype=np.int16)
        
        # 计算每个帧的大小（30ms）
        frame_size = int(sample_rate * 0.03)
        frame_count = len(audio_data) // frame_size
        
        # 检测语音片段
        speech_segments = []
        is_speech = False
        speech_start = 0
        silence_frames = 0
        max_silence_frames = int(0.5 / 0.03)  # 0.5秒的静音阈值
        
        for i in range(frame_count):
            frame = audio_data[i * frame_size:(i + 1) * frame_size]
            try:
                # 确保帧大小正确
                if len(frame) != frame_size:
                    continue
                    
                # 检测是否为语音
                if vad.is_speech(frame.tobytes(), sample_rate):
                    if not is_speech:
                        is_speech = True
                        speech_start = i
                    silence_frames = 0
                else:
                    if is_speech:
                        silence_frames += 1
                        if silence_frames >= max_silence_frames:
                            is_speech = False
                            # 添加填充时间
                            start_time = max(0, speech_start - int(VAD_PADDING_DURATION / 0.03))
                            end_time = min(frame_count, i - silence_frames + int(VAD_PADDING_DURATION / 0.03))
                            
                            # 检查片段长度
                            duration = (end_time - start_time) * 0.03
                            if duration >= MIN_SEGMENT_DURATION:
                                speech_segments.append((start_time, end_time))
                        
            except Exception as e:
                logger.error(f"VAD frame processing error: {str(e)}")
                continue
                
        # 处理最后一个语音片段
        if is_speech:
            start_time = max(0, speech_start - int(VAD_PADDING_DURATION / 0.03))
            end_time = min(frame_count, frame_count + int(VAD_PADDING_DURATION / 0.03))
            duration = (end_time - start_time) * 0.03
            if duration >= MIN_SEGMENT_DURATION:
                speech_segments.append((start_time, end_time))
        
        # 合并过近的片段
        if len(speech_segments) > 1:
            merged_segments = []
            current_start, current_end = speech_segments[0]
            
            for start, end in speech_segments[1:]:
                gap = (start - current_end) * 0.03  # 计算间隔时间
                if gap < 1.0:  # 如果间隔小于1秒，合并片段
                    current_end = end
                else:
                    merged_segments.append((current_start, current_end))
                    current_start, current_end = start, end
            
            merged_segments.append((current_start, current_end))
            speech_segments = merged_segments
            
        logger.info(f"检测到 {len(speech_segments)} 个语音片段")
        return speech_segments, sample_rate
        
    except Exception as e:
        logger.error(f"VAD 处理失败: {str(e)}")
        raise

def extract_audio_segments(audio_path, speech_segments, sample_rate):
    """提取语音片段"""
    try:
        # 读取音频文件
        with wave.open(audio_path, 'rb') as wf:
            audio_data = np.frombuffer(wf.readframes(wf.getnframes()), dtype=np.int16)
            
        # 计算每个帧的大小
        frame_size = int(sample_rate * 0.03)
        
        # 提取每个语音片段
        audio_segments = []
        for start, end in speech_segments:
            start_sample = start * frame_size
            end_sample = end * frame_size
            
            # 提取音频片段
            segment = audio_data[start_sample:end_sample]
            
            # 应用简单的音频增强
            segment = segment.astype(np.float32)
            segment = segment / np.max(np.abs(segment))
            segment = (segment * 32767).astype(np.int16)
            
            # 创建临时文件
            temp_path = f"{audio_path[:-4]}_segment_{len(audio_segments)}.wav"
            with wave.open(temp_path, 'wb') as wf:
                wf.setnchannels(1)
                wf.setsampwidth(2)
                wf.setframerate(sample_rate)
                wf.writeframes(segment.tobytes())
            
            audio_segments.append(temp_path)
            
        return audio_segments
        
    except Exception as e:
        logger.error(f"提取音频片段失败: {str(e)}")
        raise

def split_audio(audio_path, max_length_ms=30000):
    """将音频文件分割成小段"""
    try:
        # 设置ffmpeg路径
        if not setup_ffmpeg():
            raise Exception("Failed to setup FFmpeg")
            
        logger.info(f"开始加载音频文件: {audio_path}")
        logger.info(f"文件大小: {os.path.getsize(audio_path) / 1024 / 1024:.2f}MB")
        
        # 使用 VAD 检测语音片段
        speech_segments, sample_rate = detect_speech_segments(audio_path)
        
        # 提取语音片段
        audio_segments = extract_audio_segments(audio_path, speech_segments, sample_rate)
        
        # 处理每个语音片段
        processed_segments = []
        for segment_path in audio_segments:
            try:
                # 加载音频片段
                audio = AudioSegment.from_wav(segment_path)
                
                # 转换为单声道
                audio = audio.set_channels(1)
                
                # 设置采样率为16kHz
                audio = audio.set_frame_rate(16000)
                
                # 应用音频增强
                audio = audio.normalize()
                
                # 分割过长的片段
                if len(audio) > max_length_ms:
                    chunks = make_chunks(audio, max_length_ms)
                    for i, chunk in enumerate(chunks):
                        chunk_path = f"{segment_path[:-4]}_chunk_{i}.wav"
                        chunk.export(chunk_path, format="wav")
                        processed_segments.append(chunk_path)
                else:
                    processed_segments.append(segment_path)
                    
            except Exception as e:
                logger.error(f"处理音频片段失败: {str(e)}")
                continue
                
        logger.info(f"音频处理完成，共 {len(processed_segments)} 个片段")
        return processed_segments
        
    except Exception as e:
        logger.error(f"音频分割失败: {str(e)}")
        raise

def transcribe_audio(audio_path):
    """转写音频文件"""
    try:
        # 设置ffmpeg路径
        if not setup_ffmpeg():
            raise Exception("Failed to setup FFmpeg")
            
        # 分割音频
        chunks = split_audio(audio_path)
        
        # 转写每个音频段
        transcriptions = []
        for i, chunk_path in enumerate(chunks):
            try:
                # 读取音频文件
                with open(chunk_path, 'rb') as fp:
                    audio_data = fp.read()
                
                # 使用百度语音识别
                result = client.asr(audio_data, 'wav', 16000)
                
                if result.get('err_no') == 0:
                    text = result.get('result', [''])[0]
                    transcriptions.append(text)
                else:
                    error_msg = result.get('err_msg', '未知错误')
                    # 如果是识别错误，尝试使用不同的参数
                    try:
                        result = client.asr(audio_data, 'wav', 16000, {
                            'dev_pid': 1537,  # 普通话识别
                            'vad_endpoint_timeout': 2000,
                            'vad_silence_time': 200
                        })
                        if result.get('err_no') == 0:
                            text = result.get('result', [''])[0]
                            transcriptions.append(text)
                    except Exception as e:
                        logger.error(f"备选参数识别失败: {str(e)}")
                
            except Exception as e:
                logger.error(f"转写失败: {str(e)}")
            
            # 删除临时音频段文件
            try:
                os.remove(chunk_path)
            except Exception as e:
                logger.error(f"删除临时文件失败: {str(e)}")
        
        # 合并所有转写结果并清理
        final_transcription = " ".join(transcriptions)
        # 清理多余的空白字符和重复的标点符号
        final_transcription = " ".join(final_transcription.split())
        return final_transcription
        
    except Exception as e:
        logger.error(f"转写失败: {str(e)}")
        raise

def process_audio(audio_path):
    """处理音频文件"""
    try:
        logger.info(f"开始处理音频文件: {audio_path}")
        
        # 转写音频
        transcription = transcribe_audio(audio_path)
        
        return transcription
        
    except Exception as e:
        logger.error(f"音频处理失败: {str(e)}")
        raise

def main():
    if len(sys.argv) != 2:
        logger.error("使用方法: python asr.py <音频文件路径>")
        sys.exit(1)
        
    audio_path = sys.argv[1]
    
    if not os.path.exists(audio_path):
        logger.error(f"错误: 找不到音频文件 {audio_path}")
        sys.exit(1)
        
    try:
        # 设置超时时间（5分钟）
        timeout = 300
        start_time = time.time()
        
        # 处理音频
        transcription = process_audio(audio_path)
        
        # 检查是否超时
        if time.time() - start_time > timeout:
            logger.error("处理超时")
            sys.exit(1)
            
        # 输出结果
        print(transcription)
        
    except Exception as e:
        logger.error(f"处理失败: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main() 