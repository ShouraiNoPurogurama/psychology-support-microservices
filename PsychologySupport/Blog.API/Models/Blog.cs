using BuildingBlocks.DDD;

namespace Blog.API.Models
{
    public class Blog : Entity<Guid>
    {
        public Guid AuthorId { get; set; } // ManagerId

        public string Title { get; set; }

        public string Content { get; set; }

        public string VideoLink { get; set; }
    }
}
