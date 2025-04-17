import clip
import torch
from torch.nn.functional import normalize
import re

# 提取文本特征(文本可多个)
def extract_description_feature(description, model, device):
    # 如果输入是单个文本，转化为列表
    if isinstance(description, str):
        description = [description]
    # 处理文本，确保总共不超过 20 个汉字
    def truncate_text(text, max_hanzi=20):
        count = 0
        result = []
        for char in text:
            if re.match(r'[\u4e00-\u9fff]', char):  # 识别汉字
                count += 1
            result.append(char)
            if count >= max_hanzi:  # 超过 20 个汉字就截断
                break
        return ''.join(result)

    description = [truncate_text(text) for text in description]
    description_tokenized = clip.tokenize(description).to(device)
    with torch.no_grad():
        description_features = model.encode_text(description_tokenized)
        description_features = normalize(description_features, p=2, dim=-1)  # L2 归一化
    return description_features