using MongoDB.Bson;
using MongoDB.Driver;
using ChatService.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace ChatService.Data
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(IMongoClient mongoClient)
        {
            // 获取数据库实例
            _database = mongoClient.GetDatabase("chat_db");
        }

        // 插入文档到指定集合
        public async Task InsertDocumentAsync(string collectionName, BsonDocument document)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertOneAsync(document);
        }

        //根据usrid返回聊天记录
        public async Task<PagedResult> GetChatHistoryPagedAsync(string userId, int page, int pageSize)
        {
            var collection = _database.GetCollection<BsonDocument>("chat_logs");

            // 查询条件：匹配用户 ID
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);

            // 获取总记录数
            var totalCount = await collection.CountDocumentsAsync(filter);

            // 分页查询并手动映射字段
            var skip = (page - 1) * pageSize;
            var rawChatLogs = await collection.Find(filter)
                                               .Skip(skip)
                                               .Limit(pageSize)
                                               .ToListAsync();

            // 将原始数据映射到 ChatLog 对象
            var chatLogs = rawChatLogs.Select(doc =>
            {
                // 提取嵌套的 ChatResult 数据
                var chatResult = doc.GetValue("ChatResult").AsBsonDocument;

                // 解析时间字符串
                var timeString = chatResult.GetValue("time").AsString;
                var timestamp = DateTime.ParseExact(timeString, "yyyy年M月d日 HH:mm", CultureInfo.InvariantCulture).ToUniversalTime();

                return new ChatLog
                {
                    timestamp = timestamp,
                    question = chatResult.GetValue("question").AsString,
                    answer = chatResult.GetValue("answer").AsString
                };
            }).ToList();

            // 返回分页结果
            return new PagedResult
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                userId = userId,
                Data = chatLogs
            };
        }
    }
}
