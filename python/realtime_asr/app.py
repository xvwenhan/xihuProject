# app.py
from flask import Flask, request, Response
from flask_cors import CORS
from recognizer.manager import session_manager
import queue
import logging


logger = logging.getLogger()
logging.basicConfig(format='[%(asctime)-15s] [%(funcName)s()][%(levelname)s] %(message)s')
logger.setLevel(logging.INFO)

app = Flask(__name__)
CORS(app)

# 管理每个会话的消息队列
client_queues = {}

@app.route('/start', methods=['POST'])
def start():
    data = request.json
    meeting_id = data.get('meeting_id')
    room_id = data.get('room_id')

    if not meeting_id or not room_id:
        return {"code": 400, "msg": "缺少参数"}, 400

    if meeting_id not in client_queues:
        client_queues[meeting_id] = queue.Queue()
    q = client_queues[meeting_id]

    def send_callback(msg_type, content):
        q.put(f"event: {msg_type}\ndata: {content}\n\n")

    success, msg = session_manager.start_session(meeting_id, room_id, send_callback)
    code = 200 if success else 400
    return {"code": code, "msg": msg}, code


@app.route('/stream/<meeting_id>')
def stream(meeting_id):
    def event_stream():
        q = client_queues.get(meeting_id)
        if not q:
            yield f"event: error\ndata: 会话未启动\n\n"
            return
        while True:
            try:
                data = q.get(timeout=600)
                yield data
            except queue.Empty:
                yield f"event: ping\ndata: keepalive\n\n"

    return Response(event_stream(), mimetype='text/event-stream')


@app.route('/stop/<meeting_id>', methods=['POST'])
def stop(meeting_id):
    logging.info("进入中止函数")
    success, msg = session_manager.stop_session(meeting_id)
    if meeting_id in client_queues:
        del client_queues[meeting_id]
    return {"msg": msg}, 200 if success else 404


if __name__ == '__main__':
    app.run(port=5006, threaded=True)
