namespace Media.API.Media.Dtos
{
    public class MediaUploadInitRequestDto
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long SizeBytes { get; set; }
        public string AliasVersionId { get; set; }
        public MediaOwnerDto Owner { get; set; }
        public MediaUploadContextDto Context { get; set; }
        public string IdempotencyKey { get; set; }
    }
}
