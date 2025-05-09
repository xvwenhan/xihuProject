from flask import Flask, request, jsonify

from main import make_recommendation
from flask_cors import CORS

app = Flask(__name__)
CORS(app)


@app.route('/', methods=['GET'])
def home():
    return jsonify({"message": "Welcome to the Flask API!"}), 200

# 为ID为XX的用户推荐商品
@app.route('/recommend_for_user', methods=['POST'])
def recommend_for_user():
    try:
        data = request.json
        user_id = data.get('userId')

        # 检查 userId 是否存在
        if not user_id:
            return jsonify({"error": "Missing required field: userId"}), 400

        # 尝试将 userId 从字符串转换为整数
        try:
            user_id = int(user_id)
        except ValueError:
            return jsonify({"error": "Invalid userId, must be an integer"}), 400


        print("成功获取",user_id)

        if not user_id:
            return jsonify({"error": "Missing required fields"}), 400

        recommendations = make_recommendation(user_id)
        print(recommendations)
        return jsonify({"recommendations": recommendations}), 200

    except Exception as e:
        print("start"+str(e))
        return jsonify({"error": str(e)}), 500


@app.route('/test', methods=['POST'])
def testAPI():
    try:
        data = request.json
        user_id = data.get('user_id')
        print("成功进入函数！" + str(user_id))
        return jsonify({"message": "User features updated"}), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    # 在本地启动服务，端口号5000
    print("Starting Flask server...")
    # app.run(host='127.0.0.1', port=5005, debug=True)
    app.run(host='0.0.0.0', port=5005, debug=True)


