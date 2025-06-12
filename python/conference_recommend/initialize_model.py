import torch
import clip


def initialize_clip_model():
    """
    初始化 CLIP 模型和预处理器。
    返回模型、预处理器以及设备信息。
    """
    device = "cuda" if torch.cuda.is_available() else "cpu"  # 设置设备
    model, preprocess = clip.load("ViT-B/32", device=device)  # 加载模型
    # 加载预训练的 CLIP 模型，ViT-B/32 是一种视觉 Transformer 架构（有不同的模型可以选择）。
    # preprocess 是一个函数，用于对输入的图像进行预处理（例如调整大小、标准化等），以适应 CLIP 模型的输入要求。
    return device, model, preprocess


# def get_mysql_connection():
#     """连接 MySQL 数据库"""
    # return mysql.connector.connect(
    #     host='8.133.201.233',
    #     user='xihu',
    #     password='Xihu2009@',
    #     database='XihuUser'
    # )