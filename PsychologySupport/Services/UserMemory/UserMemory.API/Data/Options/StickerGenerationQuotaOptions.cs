using BuildingBlocks.Enums;

namespace UserMemory.API.Data.Options;

public static class StickerGenerationQuotaOptions
{
    public const int REWARD_COST = 1000;
    
    public const int MAX_FREE_CLAIMS_PER_DAY = 0;
    public const int MAX_TRIAL_CLAIMS_PER_DAY = 1; 
    public const int MAX_PREMIUM_CLAIMS_PER_DAY = 1;

    /// <summary>
    /// Helper an toàn để lấy giới hạn dựa trên Tier.
    /// Đây là NƠI DUY NHẤT định nghĩa nghiệp vụ "gói nào có giới hạn bao nhiêu".
    /// </summary>
    public static int GetMaxClaimsForTier(SubscriptionTier tier)
    {
        return tier switch
        {
            SubscriptionTier.Free => MAX_FREE_CLAIMS_PER_DAY,
            SubscriptionTier.FreeTrial => MAX_TRIAL_CLAIMS_PER_DAY,
            SubscriptionTier.Premium => MAX_PREMIUM_CLAIMS_PER_DAY,
            _ => MAX_FREE_CLAIMS_PER_DAY
        };
    }
}