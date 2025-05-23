# recognizer/recognizer_worker.py
import os, time, uuid, json, threading
import sys

import ffmpeg
import websocket
import pymysql
import requests
from utils.get_url import get_real_url
from recognizer import const
import logging

logger = logging.getLogger()
logging.basicConfig(format='[%(asctime)-15s] [%(funcName)s()][%(levelname)s] %(message)s')
logger.setLevel(logging.INFO)



class RecognizerWorker:
    def __init__(self, meeting_id, room_id, send_callback):
        self.meeting_id = meeting_id
        self.room_id = room_id
        self.send_callback = send_callback
        self.stop_flag = False
        self.offset = 0
        self.pcm_file = f"tmp_{meeting_id}.pcm"
        self.ffmpeg_process = None
        self.ws=None
        self.db_connection = None
        self.init_db_connection()  # 添加这一行
        self.result_file = f"{meeting_id}.txt"  # 假设这个是存储识别结果的文件
        self.send_audio_thread = None
        self.audio_stop_flag = False
        self.ws_app = None
        self.ws_thread = None
        self.txt_lock = threading.Lock()
        self.last_word = ""
        self.message_queue = []
        self.ws_lock = threading.Lock()  # 初始化锁

    def run(self):
        urls = get_real_url(self.room_id)
        if not urls:
            self.send_callback("error", "直播地址获取失败")
            return

        live_url = list(urls.values())[0]
        threading.Thread(target=self.download_audio_stream, args=(live_url,), daemon=True).start()
        time.sleep(1)  # 等待一秒
        threading.Thread(target=self.websocket_reconnect_loop, daemon=True).start()  # 五分钟检查
        self.start_websocket()

    def send_finish(self):
        """
        发送结束信号
        """
        print("send_finish")
        req = {"type": "FINISH"}
        while self.ws_app is None and self.stop_flag != True:
            time.sleep(0.1)
            logger.info(f"正在等待1…………………………")
        while  self.ws_app.sock is None or  self.ws_app.sock.connected is None:
            if self.stop_flag is True:
                return
            time.sleep(0.1)
            logger.info(f"正在等待2…………………………")
        if self.stop_flag is True:
            logger.info(f"准备退出…………………………")
            return
        #self.process_and_store_summary()
        self.ws_app.send(json.dumps(req), websocket.ABNF.OPCODE_TEXT)
        logger.info("发送 FINISH 帧")
        self.process_and_store_summary()


    def websocket_reconnect_loop(self):
        while not self.stop_flag:
            # 每次最多连接保持 5 分钟（300秒），然后强制关闭（触发 on_close 自动重连）
            for _ in range(180):
                print("滴答滴答！")
                if self.stop_flag:
                    return
                #if self.ws_app is None:
                    #return
                time.sleep(1)
            print("五分钟已到开始切断！")
            #self.send_finish()
            self.process_and_store_summary()


    def stop(self):
        self.stop_flag = True

        if self.ffmpeg_process:
            self.ffmpeg_process.terminate()

        if os.path.exists(self.pcm_file):
            time.sleep(1)
            os.remove(self.pcm_file)

        if self.ws_app:
            try:
                self.ws_app.keep_running = False
                self.ws_app.close()
            except:
                pass
        if self.ws_thread and self.ws_thread.is_alive():
            self.ws_thread.join()

    def download_audio_stream(self, live_url):
        self.ffmpeg_process = (
            ffmpeg
            .input(live_url)
            .output(self.pcm_file, format='s16le', acodec='pcm_s16le', ac=1, ar='16k')
            .run_async()
        )

    def send_start_params(self, ws):
        req = {
            "type": "START",
            "data": {
                "appid": const.APPID,
                "appkey": const.APPKEY,
                "dev_pid": const.DEV_PID,
                "cuid": self.meeting_id,
                "sample": 16000,
                "format": "pcm"
            }
        }
        ws.send(json.dumps(req))

    def send_audio(self, ws):
        chunk_ms = 160
        chunk_size = int(16000 * 2 / 1000 * chunk_ms)

        while not self.stop_flag:
            if self.audio_stop_flag:
                break
            if not os.path.exists(self.pcm_file):
                time.sleep(0.1)
                continue
            if self.ws_app is None or self.ws_app.sock is None or  self.ws_app.sock.connected is None:  # 检查连接状态
                logger.info(f"222222222222222222")
                time.sleep(0.1)
                continue
            with open(self.pcm_file, 'rb') as f:
                f.seek(self.offset)
                chunk = f.read(chunk_size)

            if chunk:
                if self.audio_stop_flag:
                    break
                logger.info(f"准备发送音频audio！！！！")
                ws.send(chunk, websocket.ABNF.OPCODE_BINARY)
                self.offset += len(chunk)
            else:
                time.sleep(chunk_ms / 1000.0)

        # ws.send(json.dumps({"type": "FINISH"}), websocket.ABNF.OPCODE_TEXT)

    def on_message(self, ws, message):
        msg_json = json.loads(message)
        msg_type = msg_json.get("type", "")
        result = msg_json.get("result", "")
        if msg_type == "MID_TEXT" and result:
            self.last_word = result
            self.send_callback(msg_type.lower(), result)
            print("中间识别：", result)
        if msg_type == "FIN_TEXT":
            final_text = result if result else self.last_word
            self.last_word=''

            with open(f"{self.meeting_id}.txt", "a", encoding="utf-8") as f:
                f.write(final_text + "\n")
                print("结束识别：", final_text)
                print("结束帧打印###：",msg_json )
                if not self.stop_flag:
                    print("收完结束帧，开始重连！")
                    threading.Thread(target=self.restart_websocket, daemon=True).start()

    def on_close(self,ws, close_status_code, close_msg):
        # self.send_callback("closed", "连接关闭")
        # if not self.stop_flag:
        #with self.ws_lock:  # 加锁
            #if self.ws_app == ws:  # 确保只处理当前连接的关闭
                #self.ws_app = None
        logger.info(f"WebSocket 连接关闭: {close_status_code}, {close_msg}")
        print("进入onclose！")

    def on_error(self, ws, error):
        print("WebSocket 出错：", error)
        self.send_callback("error", str(error))
        if not self.stop_flag:
            threading.Thread(target=self.restart_websocket, daemon=True).start()

    def on_open(self,ws):
        """
         连接成功后启动发送音频数据并定时发送结束帧
         """
        logger.info(f"进去重连……………………")
        self.send_start_params(ws)
        if self.send_audio_thread and self.send_audio_thread.is_alive():
            self.audio_stop_flag = True
            self.send_audio_thread.join()  # 等待旧线程结束
            logger.info(f"正在重连……………………")
            self.audio_stop_flag = False  # 重连时允许继续跑
        self.send_audio_thread = threading.Thread(target=self.send_audio, args=(ws,), daemon=True)
        self.send_audio_thread.start()
        # threading.Thread(target=self.send_audio, args=(ws,), daemon=True).start()


    def start_websocket(self):
        uri = const.URI + "?sn=" + str(uuid.uuid4())
        with self.ws_lock:  # 加锁
            self.ws_app = websocket.WebSocketApp(
                uri,
                on_open=self.on_open,
                on_message=self.on_message,
                on_close=self.on_close,
                on_error=lambda ws, err: self.on_error(ws, err)
            )

        self.ws_thread = threading.Thread(target=self.ws_app.run_forever, daemon=True)
        self.ws_thread.start()

    def restart_websocket(self):
        with self.ws_lock:  # 加锁
            if self.ws_app:
                try:
                    self.ws_app.keep_running = False
                    self.ws_app.close()
                except Exception as e:
                    print("关闭旧 websocket 出错：", e)
            self.ws_app = None

        if self.ws_thread and self.ws_thread.is_alive():
            self.ws_thread.join()
        #time.sleep(1)  # 小停顿，避免频繁重连
        print("已关闭旧 websocket，准备重连")
        self.start_websocket()

    def get_access_token(self):
        """
        使用 AK，SK 生成鉴权签名（Access Token）
        """
        url = "https://aip.baidubce.com/oauth/2.0/token"
        params = {"grant_type": "client_credentials", "client_id": const.API_KEY, "client_secret": const.SECRET_KEY}
        return str(requests.post(url, params=params).json().get("access_token"))

    def get_summary(self, text):
        """
        调用百度 NLP API 生成文章总结
        :param text: 需要总结的文章文本
        :return: 返回总结内容
        """
        url = f"https://aip.baidubce.com/rpc/2.0/nlp/v1/news_summary?charset=UTF-8&access_token={self.get_access_token()}"
        payload = json.dumps({"content": text}, ensure_ascii=False)
        headers = {'Content-Type': 'application/json', 'Accept': 'application/json'}

        try:
            response = requests.post(url, headers=headers, data=payload.encode("utf-8"))
            result = response.json()
            if "summary" in result:
                return result["summary"]
            else:
                logger.warning(f"API 调用失败: {result}")
                return None
        except Exception as e:
            logger.error(f"总结 API 调用失败: {e}")
            return None

    def safely_read_and_clear_txt_file(self):
        """
        安全读取 TXT 文件内容并清空
        :return: 文件内容
        """
        try:
            with self.txt_lock:  # 使用锁来确保线程安全
                with open(self.result_file, "r", encoding="utf-8") as f:
                    original_text = f.read().strip()  # 读取文件内容并去除多余空白
                # 清空文件内容
                with open(self.result_file, "w", encoding="utf-8") as f:
                    f.truncate(0)
                logger.info(f"成功读取并清空 TXT 文件内容")
                return original_text
        except Exception as e:
            logger.error(f"读取和清空 TXT 文件失败: {e}")
            return None

    def init_db_connection(self):
        """
        初始化数据库连接并返回连接对象
        """
        try:
            self.db_connection = pymysql.connect(
                host="8.133.201.233",  # 你的数据库服务器地址
                user="xihu",  # 你的数据库用户名
                password="Xihu2009@",  # 你的数据库密码
                database="VideoServiceDB",  # 你的数据库名称
                charset="utf8mb4"  # 设置字符集
            )
            logger.info("数据库连接成功")
        except pymysql.MySQLError as err:
            logger.error(f"数据库连接错误: {err}")
            sys.exit(1)  # 如果数据库连接失败，则退出程序

    def cleanup_db_connection(self):
        """
        程序退出时关闭数据库连接
        """
        if self.db_connection:
            self.db_connection.close()
            logger.info("数据库连接已关闭")

    def store_in_database(self, original_text, summary, start_time, end_time):
        """
        存储识别文本和总结到数据库
        """
        if self.db_connection is None:
            logger.error("数据库连接为空，无法执行数据库操作")
            return

        try:
            cursor = self.db_connection.cursor()
            # 编写插入语句
            sql = """INSERT INTO VideoSummary (MeetingId, StartTime, EndTime, OriginalText, Summary)
                            VALUES (%s, %s, %s, %s, %s)"""
            try:
                cursor.execute(sql, (self.meeting_id, start_time, end_time, original_text, summary))
                self.db_connection.commit()  # 提交事务
                logger.info("数据已成功插入数据库")
            except Exception as e:
                logger.error(f"SQL 执行错误: {e}")
                self.db_connection.rollback()  # 出错时回滚事务
            cursor.close()
        except Exception as e:
            logger.error(f"插入数据库失败: {e}")

    def process_and_store_summary(self):
        """处理文本总结并存储到数据库"""
        original_text = self.safely_read_and_clear_txt_file()  # 安全读取 TXT 内容并清空
        if original_text:
            summary = self.get_summary(original_text)  # 调用 NLP API 获取总结
            if summary:
                logger.info(f"总结: {summary}")
                end_time = time.strftime('%Y-%m-%d %H:%M:%S', time.localtime())
                start_time = time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(time.time() - 180))  # 300秒前为开始时间
                logger.info(f"插入的数据: {original_text}, {summary}, {start_time}, {end_time}")
                self.store_in_database(original_text, summary, start_time, end_time)
            else:
                logger.error("总结失败")
        else:
            logger.error("未能成功读取 TXT 文件内容")
