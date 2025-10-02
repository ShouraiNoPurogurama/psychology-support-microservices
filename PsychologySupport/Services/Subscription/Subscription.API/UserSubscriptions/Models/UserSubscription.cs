using BuildingBlocks.DDD;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Models;

namespace Subscription.API.UserSubscriptions.Models;

public class UserSubscription : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }

    public Guid ServicePackageId { get; private set; }

    public DateTimeOffset StartDate { get; private set; }

    public DateTimeOffset EndDate { get; private set; }

    public Guid? PromoCodeId { get; private set; }

    public Guid? GiftId { get; private set; }

    public SubscriptionStatus Status { get; set; }

    public ServicePackage ServicePackage { get; private set; }

    public decimal FinalPrice { get; private set; }


    public UserSubscription()
    {
        ServicePackage = new ServicePackage();
    }

    private UserSubscription(Guid patientId, Guid servicePackageId, DateTimeOffset startDate, DateTimeOffset endDate, Guid? promoCodeId,
        Guid? giftId, SubscriptionStatus status, ServicePackage servicePackage, decimal finalPrice)
    {
        PatientId = patientId;
        ServicePackageId = servicePackageId;
        StartDate = startDate;
        EndDate = endDate;
        PromoCodeId = promoCodeId;
        GiftId = giftId;
        Status = status;
        ServicePackage = servicePackage;
        FinalPrice = finalPrice;
    }

    public static UserSubscription Create(Guid patientId, Guid servicePackageId, DateTimeOffset startDate,
        Guid? promoCodeId, Guid? giftId, ServicePackage servicePackage, decimal finalPrice)
    {
        #region Validations

        ArgumentException.ThrowIfNullOrEmpty(nameof(patientId), patientId.ToString());
        ArgumentException.ThrowIfNullOrEmpty(nameof(servicePackageId), servicePackageId.ToString());
        ArgumentNullException.ThrowIfNull(servicePackage, nameof(servicePackage));
        ArgumentOutOfRangeException.ThrowIfLessThan(finalPrice, 0);

        if (!servicePackage.IsActive)
            throw new InvalidOperationException("ServicePackage must be active.");

        #endregion

        if (promoCodeId == Guid.Empty)
        {
            promoCodeId = null;
        }

        if (giftId == Guid.Empty)
        {
            giftId = null;
        }

        var userSubscription = new UserSubscription(patientId, servicePackageId, startDate.ToUniversalTime(),
            startDate.AddDays(servicePackage.DurationDays).ToUniversalTime(), promoCodeId, giftId,
            SubscriptionStatus.AwaitPayment, servicePackage, finalPrice);

        return userSubscription;
    }

    public static UserSubscription Activate(Guid patientId, Guid servicePackageId, DateTimeOffset startDate,
       Guid? promoCodeId, Guid? giftId, ServicePackage servicePackage, decimal finalPrice)
    {
        #region Validations

        ArgumentException.ThrowIfNullOrEmpty(nameof(patientId), patientId.ToString());
        ArgumentException.ThrowIfNullOrEmpty(nameof(servicePackageId), servicePackageId.ToString());
        ArgumentNullException.ThrowIfNull(servicePackage, nameof(servicePackage));
        ArgumentOutOfRangeException.ThrowIfLessThan(finalPrice, 0);

        if (!servicePackage.IsActive)
            throw new InvalidOperationException("ServicePackage must be active.");

        #endregion

        if (promoCodeId == Guid.Empty)
        {
            promoCodeId = null;
        }

        if (giftId == Guid.Empty)
        {
            giftId = null;
        }

        var userSubscription = new UserSubscription(patientId, servicePackageId, startDate.ToUniversalTime(),
            startDate.AddDays(servicePackage.DurationDays).ToUniversalTime(), promoCodeId, giftId,
            SubscriptionStatus.Active, servicePackage, finalPrice);

        return userSubscription;
    }
}