import os
import ffmpeg
import threading
import websocket
import time
import uuid
import json
import logging
import sys
from utils.get_url import get_real_url  # 引入 B 站直播流获取
from recognizer import const

ffmpeg_process = None  # 新增全局变量
offset = 0  # 在文件顶部定义为全局变量



# 设置 ffmpeg 路径
os.environ["PATH"] += os.pathsep + r"F:\env\ffmpeg-7.1.1-essentials_build\bin"

logger = logging.getLogger()
logging.basicConfig(format='[%(asctime)-15s] [%(funcName)s()][%(levelname)s] %(message)s')
logger.setLevel(logging.INFO)

pcm_file = "audio.pcm"
result_file = "result.txt"

# 🔄 添加退出标志与锁
stop_flag = False
pcm_lock = threading.Lock()


def download_audio_stream(live_url):
    global ffmpeg_process
    logger.info("开始下载直播音频流...")

    try:
        ffmpeg_process = (
            ffmpeg
            .input(live_url)
            .output(pcm_file, format='s16le', acodec='pcm_s16le', ac=1, ar='16k')
            .run_async()  # 不要加 pipe_stdout、pipe_stdin
        )
        logger.info("FFmpeg 流式音频转码已启动")
    except ffmpeg.Error as e:
        logger.error(f"FFmpeg 错误: {e.stderr.decode()}")
        sys.exit(1)


def send_start_params(ws):
    """
    发送百度语音 API 开始参数
    """
    req = {
        "type": "START",
        "data": {
            "appid": const.APPID,
            "appkey": const.APPKEY,
            "dev_pid": const.DEV_PID,
            "cuid": "yourself_defined_user_id",
            "sample": 16000,
            "format": "pcm"
        }
    }
    ws.send(json.dumps(req), websocket.ABNF.OPCODE_TEXT)
    logger.info("发送 START 帧")



def send_audio(ws):
    global stop_flag
    global offset
    chunk_ms = 160
    chunk_size = int(16000 * 2 / 1000 * chunk_ms)
    logger.info("开始发送音频数据...")

    silence_count = 0

    while not stop_flag:
        if not os.path.exists(pcm_file):
            time.sleep(0.1)
            continue

        with pcm_lock:
            try:
                with open(pcm_file, 'rb') as f:
                    f.seek(offset)
                    chunk = f.read(chunk_size)

                    if not chunk:
                        f.seek(0, os.SEEK_END)
                        filesize = f.tell()
                        if offset >= filesize:
                            # 文件还没写新数据
                            time.sleep(chunk_ms / 1000.0)
                            continue
            except Exception as e:
                logger.warning(f"读取 pcm 失败：{e}")
                chunk = None

        if chunk:
            try:
                ws.send(chunk, websocket.ABNF.OPCODE_BINARY)
            except Exception as e:
                logger.warning(f"发送音频失败：{e}")
                break

            offset += len(chunk)
            silence_count = 0
        else:
            silence_count += 1
            time.sleep(chunk_ms / 1000.0)

        if silence_count >= 30:
            logger.info("超过5秒无数据，自动结束发送")
            break

    logger.info("音频数据发送完成")


def send_finish(ws):
    """
    发送结束信号
    """
    req = {"type": "FINISH"}
    ws.send(json.dumps(req), websocket.ABNF.OPCODE_TEXT)
    logger.info("发送 FINISH 帧")


# 每隔五分钟定时发送结束帧
def send_finish_periodically(ws, stop_event):
    """
    每隔一定时间定时发送结束帧（直到用户按下回车）
    """
    while not stop_event.is_set() and not stop_flag:
        time.sleep(300)  # 每300秒发送一次结束帧
        send_finish(ws)
        logger.info("发送 FINISH 帧")
        # 此处调用NLP读某TXT文件的内容，把带起止时间的原文和总结后内容存入数据库。清空TXT的内容。

def on_open(ws):
    """
     连接成功后启动发送音频数据并定时发送结束帧
     """
    stop_event = threading.Event()  # 用于停止定时发送结束帧
    # 启动定时发送结束帧线程
    threading.Thread(target=send_finish_periodically, args=(ws, stop_event), daemon=True).start()

    def run():
        send_start_params(ws)
        send_audio(ws)
        # send_finish(ws)

    threading.Thread(target=run).start()


def on_message(ws, message):
    logger.info(f"识别结果: {message}")
    try:
        msg_json = json.loads(message)
        msg_type = msg_json.get("type", "")
        if msg_type == "MID_TEXT":
            result_text = msg_json.get("result", "")
            if isinstance(result_text, dict):
                text = result_text.get("text", "")
            else:
                text = result_text
            print("📝 中间识别：", text)
            # 此处中间识别结果

        result_text = msg_json.get("result", "")

        if msg_type == "FIN_TEXT" and result_text:
            print("✔ 最终识别结果：", result_text)
            with open(result_file, "a", encoding="utf-8") as f:
                f.write(result_text + "\n")
                # 此处将最终识别结果写入TXT
    except Exception as e:
        logger.warning(f"解析识别结果失败: {e}")


def on_error(ws, error):
    """
    处理错误
    """
    global stop_flag
    logger.error(f"WebSocket 错误: {error}")
    # 错误发生时尝试重连
    if not stop_flag:
        reconnect_websocket()

def on_close(ws, close_status_code, close_msg):
    global stop_flag
    logger.info("WebSocket 连接已关闭")
    # 连接关闭时尝试重连
    if not stop_flag:
        reconnect_websocket()


def reconnect_websocket():
    """
    重新连接 WebSocket
    """
    logger.info("尝试重新连接 WebSocket...")
    time.sleep(1)  # 等待1秒后尝试重新连接
    start_websocket()  # 重新启动 WebSocket 连接


def start_websocket():
    """
    连接百度语音 API 进行实时语音识别
    """
    websocket.enableTrace(False)
    uri = const.URI + "?sn=" + str(uuid.uuid1())
    ws_app = websocket.WebSocketApp(uri,
                                    on_open=on_open,
                                    on_message=on_message,
                                    on_error=on_error,
                                    on_close=on_close)
    ws_app.run_forever()


def user_input_listener():
    """
    监听用户输入，按下回车停止
    """
    global stop_flag
    input("按回车键停止识别...\n")
    stop_flag = True
    logger.info("用户触发退出")


if __name__ == '__main__':
    if os.path.exists(pcm_file):
        os.remove(pcm_file)
        print("旧音频文件已删除")

    if os.path.exists(result_file):
        os.remove(result_file)
        print("旧识别结果文件已删除")

    logger.setLevel(logging.DEBUG)
    logger.info("begin")
    room_id = input('请输入 B 站直播房间号：\n')
    stream_urls = get_real_url(room_id)
    if not stream_urls:
        logger.error("获取直播流失败")
        sys.exit(1)

    live_url = list(stream_urls.values())[0]
    logger.info(f"直播流地址: {live_url}")

    # 启动音频流下载线程
    download_thread = threading.Thread(target=download_audio_stream, args=(live_url,))
    download_thread.daemon = True
    download_thread.start()

    # 启动用户监听线程（保证手动结束）
    input_thread = threading.Thread(target=user_input_listener)
    input_thread.daemon = True
    input_thread.start()

    time.sleep(1)  # 等待1s FFmpeg写入第一段音频

    # 启动实时识别 WebSocket（阻塞）
    start_websocket()

    # 等待用户输入线程完成
    input_thread.join()

    # 🧼 退出前清理 FFmpeg 子进程和音频文件
    stop_flag = True  # 通知所有线程退出

    if ffmpeg_process and ffmpeg_process.poll() is None:
        logger.info("正在终止 ffmpeg 子进程...")
        ffmpeg_process.terminate()
        ffmpeg_process.wait()
        logger.info("ffmpeg 子进程已终止")

    # 删除音频文件
    try:
        os.remove(pcm_file)
        print("音频文件 audio.pcm 已删除")
    except PermissionError:
        print("删除 audio.pcm 失败：文件仍被占用")
