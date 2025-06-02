using Image.API.Data.Common;

namespace Image.API.Models
{
    public class Image
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public OwnerType OwnerType { get; set; }
        public Guid OwnerId { get; set; }
        public string Extension { get; set; } = string.Empty;
    }
}
