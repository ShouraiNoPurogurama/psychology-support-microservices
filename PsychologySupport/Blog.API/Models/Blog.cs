namespace Blog.API.Models
{
    public class Blog
    {
        public Guid Id { get; set; }

        public Guid AuthorId { get; set; } // UserId

        public string Title { get; set; }

        public string Content { get; set; }

        public string VideoLink { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }
    }
}
