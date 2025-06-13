namespace UserService.DTOs
{
    public class RecommendDTOs
    {
        public string UserId { get; set; }
    }

    public class RecommendResponse
    {
        public List<int> Recommendations { get; set; }
    }



}
