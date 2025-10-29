using BuildingBlocks.Data.Common;
using BuildingBlocks.DDD;

namespace Profile.API.Models.Public
{
    public class AffiliateProfile : AggregateRoot<Guid>
    {
        public AffiliateProfile()
        {
        }

        public AffiliateProfile(
            Guid userId,
            string fullName,
            ContactInfo contactInfo,
            string referralCode,
            string referralLink)
        {
            UserId = userId;
            FullName = fullName;
            ContactInfo = contactInfo;
            ReferralCode = referralCode;
            ReferralLink = referralLink;
        }

        public Guid UserId { get; set; }
        public string FullName { get; set; } = default!;
        public ContactInfo ContactInfo { get; set; } = default!;
        public string ReferralCode { get; set; } = default!;
        public string ReferralLink { get; set; } = default!;

        public static AffiliateProfile Create(
            Guid userId,
            string fullName,
            ContactInfo contactInfo,
            string referralCode,
            string referralLink)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("ID người dùng không được để trống.", nameof(userId));

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Tên đầy đủ không được để trống.", nameof(fullName));

            if (contactInfo == null)
                throw new ArgumentNullException(nameof(contactInfo), "Thông tin liên hệ không được để trống.");

            if (string.IsNullOrWhiteSpace(referralCode))
                throw new ArgumentException("Mã giới thiệu không được để trống.", nameof(referralCode));

            if (string.IsNullOrWhiteSpace(referralLink))
                throw new ArgumentException("Liên kết giới thiệu không được để trống.", nameof(referralLink));

            return new AffiliateProfile(userId, fullName, contactInfo, referralCode, referralLink);
        }

        public void Update(
            string fullName,
            ContactInfo contactInfo,
            string referralCode,
            string referralLink)
        {
            FullName = fullName;
            ContactInfo = contactInfo;
            ReferralCode = referralCode;
            ReferralLink = referralLink;
        }
    }
}
