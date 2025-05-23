# recognizer/manager.py
import threading
from recognizer.recognizer_worker import RecognizerWorker

class SessionManager:
    def __init__(self):
        self.sessions = {}

    def start_session(self, meeting_id, room_id, send_callback):
        if meeting_id in self.sessions:
            return False, "Session already exists"

        worker = RecognizerWorker(meeting_id, room_id, send_callback)
        thread = threading.Thread(target=worker.run)
        thread.daemon = True
        thread.start()

        self.sessions[meeting_id] = worker
        return True, "Session started"

    def stop_session(self, meeting_id):
        if meeting_id in self.sessions:
            self.sessions[meeting_id].stop()
            del self.sessions[meeting_id]
            return True, "Session stopped"
        return False, "Session not found"

    def get_active_sessions(self):
        return list(self.sessions.keys())

session_manager = SessionManager()
