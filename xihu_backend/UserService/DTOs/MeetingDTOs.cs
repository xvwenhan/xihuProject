namespace UserService.DTOs
{
    public class MeetingDTOs
    {
        public class MeetingResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Time { get; set; }
            public string Type { get; set; }
            public bool IsOnlyOffline { get; set; }
            public string Location { get; set; }
            public int OfflineNum { get; set; }//线下参会人数
            public string Url {  get; set; }
            public bool IsSubscribed {  get; set; }
        }

        public class SubscribeResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string Type { get; set; }
            public bool IsOnlyOffline { get; set; }
            public string Location { get; set; }
            public int SubscribeNum { get; set; }
            public string Url { get; set; }
        }
        public class SubscribeRequest
        {
            public int ConferenceId { get; set; }
        }
        public class SortRequest
        {
            public bool SortByTime { get; set; }
            public bool IsAsc { get; set; }
        }
    }
}
