namespace Media.Application.Dtos
{
    public class PresignedDto
    {
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public int ExpiresIn { get; set; }
    }
}
