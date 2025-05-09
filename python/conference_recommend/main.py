import pandas as pd
import numpy as np
import torch
import faiss
from collections import defaultdict
import mysql.connector

from extract_feature import extract_description_feature
from initialize_model import initialize_clip_model

import torch
import torch.nn.functional as F

# 使用你封装好的 CLIP 相关函数
device, model, preprocess = initialize_clip_model()


# **1. 读取 MySQL 数据**
def get_mysql_connection():
    """连接 MySQL 数据库"""
    return mysql.connector.connect(
        host='8.133.201.233',
        user='xihu',
        password='Xihu2009@',
        database='XihuUser'
    )


def load_data():
    """从 MySQL 读取用户订阅数据和会议议程数据"""
    conn = get_mysql_connection()
    query_subscribe = "SELECT user_id, conference_id FROM subscribe"
    query_agenda = "SELECT conference_id, agenda_name, name FROM agenda"
    query_conference = "SELECT conference_id, conference_name FROM conference"

    df_subscribe = pd.read_sql(query_subscribe, conn)
    df_agenda = pd.read_sql(query_agenda, conn)
    df_conference = pd.read_sql(query_conference, conn)

    conn.close()
    return df_subscribe, df_agenda, df_conference


# **2. 协同过滤推荐**
def collaborative_filtering_recommend(user_id, df_subscribe, top_k=5):
    """基于协同过滤的推荐（使用 PyTorch 计算余弦相似度）"""
    user_conference_matrix = df_subscribe.pivot_table(index="user_id", columns="conference_id", aggfunc="size",
                                                      fill_value=0)
    if user_id not in user_conference_matrix.index:
        return []

    # 转换为 PyTorch 张量
    user_vector_torch = torch.tensor(user_conference_matrix.loc[user_id].values, dtype=torch.float32).unsqueeze(
        0)  # (1, N)
    user_conference_matrix_torch = torch.tensor(user_conference_matrix.values, dtype=torch.float32)  # (M, N)

    # 计算余弦相似度
    similarity_scores = F.cosine_similarity(user_vector_torch, user_conference_matrix_torch, dim=1)

    # 获取最相似的用户（排除自己）
    similar_users = torch.argsort(similarity_scores, descending=True)[1:top_k + 1]

    recommended_conferences = set()
    for idx in similar_users:
        similar_user_id = user_conference_matrix.index[idx.item()]
        subscribed_conferences = set(df_subscribe[df_subscribe["user_id"] == similar_user_id]["conference_id"])
        recommended_conferences.update(subscribed_conferences)

    return list(recommended_conferences)


# **3. 计算会议描述的 CLIP 语义向量**
def build_faiss_index(df_conference):
    """构建 FAISS 向量索引"""
    descriptions = df_conference["conference_name"].tolist()
    vectors = extract_description_feature(descriptions, model, device).cpu().numpy()

    d = vectors.shape[1]  # 向量维度
    index = faiss.IndexFlatL2(d)
    index.add(vectors)

    return index, df_conference["conference_id"].tolist()


def content_based_recommend(user_id, df_subscribe, df_conference, faiss_index, conference_ids, top_k=5):
    """基于内容的 CLIP 语义推荐"""
    user_conferences = df_subscribe[df_subscribe["user_id"] == user_id]["conference_id"].tolist()
    if not user_conferences:
        return []

    # 获取用户已订阅会议的描述特征
    user_conference_names = df_conference[df_conference["conference_id"].isin(user_conferences)][
        "conference_name"].tolist()
    user_vector = extract_description_feature(user_conference_names, model, device).mean(dim=0,
                                                                                         keepdim=True).cpu().numpy()

    # FAISS 搜索
    _, indices = faiss_index.search(user_vector, top_k)
    recommended_conferences = [conference_ids[i] for i in indices[0]]

    return recommended_conferences


# **4. 混合推荐** （此版本没有去除用户已订阅的会议）
def hybrid_recommend(user_id, df_subscribe, df_conference, faiss_index, conference_ids, top_k=5):
    """混合协同过滤和内容推荐"""
    cf_recommendations = collaborative_filtering_recommend(user_id, df_subscribe, top_k)
    content_recommendations = content_based_recommend(user_id, df_subscribe, df_conference, faiss_index, conference_ids,
                                                      top_k)

    # 结合两种推荐方法，去重
    final_recommendations = list(set(cf_recommendations + content_recommendations))[:top_k]
    return final_recommendations



# **5. 主函数**
def make_recommendation(user_id):
    df_subscribe, df_agenda, df_conference = load_data()
    faiss_index, conference_ids = build_faiss_index(df_conference)

    recommended_conferences = hybrid_recommend(user_id, df_subscribe, df_conference, faiss_index, conference_ids,
                                               top_k=5)

    print(f"为用户 {user_id} 推荐的会议 ID: {recommended_conferences}")
    return recommended_conferences


# 运行推荐
# if __name__ == "__main__":
#     test_user_id = 7  # 测试用户 ID
#     make_recommendation(test_user_id)
