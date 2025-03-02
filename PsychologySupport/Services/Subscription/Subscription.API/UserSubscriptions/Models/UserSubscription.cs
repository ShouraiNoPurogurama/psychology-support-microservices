using BuildingBlocks.DDD;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Models;

namespace Subscription.API.UserSubscriptions.Models;

public class UserSubscription : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }

    public Guid ServicePackageId { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public Guid? PromoCodeId { get; private set; }

    public Guid? GiftId { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public ServicePackage ServicePackage { get; private set; }
    
    
    public decimal FinalPrice { get; private set; }
    
    
    public UserSubscription()
    {
        ServicePackage = new ServicePackage();
    }

    private UserSubscription(Guid patientId, Guid servicePackageId, DateTime startDate, DateTime endDate, Guid? promoCodeId,
        Guid? giftId, SubscriptionStatus status, ServicePackage servicePackage)
    {
        PatientId = patientId;
        ServicePackageId = servicePackageId;
        StartDate = startDate;
        EndDate = endDate;
        PromoCodeId = promoCodeId;
        GiftId = giftId;
        Status = status;
        ServicePackage = servicePackage;
    }

    public static UserSubscription Create(Guid patientId, Guid servicePackageId, DateTime startDate, DateTime endDate,
        Guid? promoCodeId, Guid? giftId, ServicePackage servicePackage)
    {
        #region Validations

        ArgumentException.ThrowIfNullOrEmpty(nameof(patientId), patientId.ToString());
        ArgumentException.ThrowIfNullOrEmpty(nameof(servicePackageId), servicePackageId.ToString());
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startDate, endDate);
        ArgumentNullException.ThrowIfNull(servicePackage, nameof(servicePackage));

        if (!servicePackage.IsActive)
            throw new InvalidOperationException("ServicePackage must be active.");

        #endregion

        var userSubscription = new UserSubscription(patientId, servicePackageId, startDate, endDate, promoCodeId, giftId,
            SubscriptionStatus.Active, servicePackage);

        return userSubscription;
    }
}