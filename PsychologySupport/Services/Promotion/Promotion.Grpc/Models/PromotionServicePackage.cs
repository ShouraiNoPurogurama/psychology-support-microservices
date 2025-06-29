namespace Promotion.Grpc.Models
{
    public class PromotionServicePackage
    {
        public string PromotionId { get; set; }
        public string ServicePackageId { get; set; }

        public virtual Promotion Promotion { get; set; } = default!;
    }
}
