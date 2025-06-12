import redis
import json
from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity
from flask_cors import CORS

app = Flask(__name__)
CORS(app)
model = SentenceTransformer('/opt/models/paraphrase-multilingual-MiniLM-L12-v2')
r = redis.StrictRedis(host='8.133.201.233', port=6379, password='Xihu2009@', decode_responses=True)


def get_best_match(user_question):
    questions = r.keys("*")  # 获取所有问题的键
    print(f"Redis 中所有的问题键: {questions}")
    if not questions:
        print("Redis 中没有找到任何问题键")
        return False, None, None
        
    user_emb = model.encode(user_question)
    best_score, best_question = 0, None

    for q in questions:
        try:
            q_emb = model.encode(q)
            score = cosine_similarity([user_emb], [q_emb])[0][0]
            if score > best_score:
                best_score = score
                best_question = q
        except Exception as e:
            print(f"处理问题 {q} 时出错: {str(e)}")
            continue

    print(f"最佳匹配问题: {best_question}, 得分: {best_score}")
    
    if best_score > 0.80 and best_question:
        try:
            # 首先检查键的类型
            key_type = r.type(best_question)
            print(f"键 {best_question} 的类型是: {key_type}")
            
            # 根据不同的类型尝试获取数据
            if key_type == 'string':
                answer_json = r.get(best_question)
            elif key_type == 'hash':
                answer_json = r.hgetall(best_question)
            else:
                print(f"不支持的键类型: {key_type}")
                return False, None, "不支持的键类型"

            if answer_json:
                try:
                    # 如果是哈希类型，需要从 data 字段中获取 JSON
                    if isinstance(answer_json, dict):
                        print(f"哈希数据的所有键: {answer_json.keys()}")
                        if 'data' in answer_json:
                            # 解析 data 字段中的 JSON
                            data_json = json.loads(answer_json['data'])
                            print(f"解析后的 data JSON: {data_json}")
                            answer = data_json.get("Answer", "")
                            return True, best_question, answer
                        else:
                            return False, None, "数据格式不正确：缺少 data 字段"
                    # 如果是字符串类型，尝试解析 JSON
                    else:
                        answer_data = json.loads(answer_json)
                        print(f"解析后的 JSON 数据: {answer_data}")
                        print(f"JSON 数据的所有键: {answer_data.keys()}")
                        if isinstance(answer_data, dict):
                            answer = answer_data.get("Answer", "")
                            return True, best_question, answer
                        else:
                            return False, None, "数据格式不正确"
                except json.JSONDecodeError as e:
                    print(f"JSON 解析错误: {str(e)}")
                    return False, None, "JSON 解析错误"
            else:
                print("Redis 返回空值")
                return False, None, None
        except redis.exceptions.ResponseError as e:
            print(f"Redis 操作错误: {str(e)}")
            return False, None, "Redis 操作错误"
    return False, None, None


@app.route('/semantic_match', methods=['GET'])
def semantic_match_interface():
    user_question = request.args.get("question")
    if not user_question:
        return jsonify({
            "success": False,
            "data": None
        }), 400

    success, best_question, answer = get_best_match(user_question)

    return jsonify({
        "success": success,
        "data": {
            "question": best_question,
            "answer": answer
        }
    })


if __name__ == '__main__':
    app.run(
        host='0.0.0.0',       
        port=5004,            
        debug=False
    )
