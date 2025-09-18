using BuildingBlocks.Data.Common;
using BuildingBlocks.DDD;
using BuildingBlocks.Utils;
using Profile.API.Domains.Pii.Events;
using Profile.API.Enums.Pii;
using Profile.API.ValueObjects.Pii;

namespace Profile.API.Models.Pii;

public partial class PersonProfile : AggregateRoot<Guid>
{
    public Guid SubjectRef { get; private set; }

    public Guid UserId { get; private set; }

    public PersonName FullName { get; private set; } = PersonName.Empty;
    public UserGender Gender { get; private set; } = UserGender.NotSet;

    public DateOnly? BirthDate { get; private set; }
    public ContactInfo ContactInfo { get; private set; } = ContactInfo.Empty();

    public PersonProfileStatus Status { get; private set; } = PersonProfileStatus.Pending;

    public static PersonProfile CreateActive(
        Guid subjectRef,
        Guid userId,
        string fullName,
        UserGender gender,
        DateOnly? birthDate,
        ContactInfo contactInfo)
    {
        ValidationBuilder.Create()
            .When(() => userId == Guid.Empty)
            .WithErrorCode("INVALID_USER_ID")
            .WithMessage("ID người dùng không được để trống.")
            .When(() => !contactInfo.HasEnoughInfo())
            .WithErrorCode("INVALID_CONTACT")
            .WithMessage("Thông tin liên hệ chưa được điền đủ.")
            .ThrowIfInvalid();

        var entity = new PersonProfile
        {
            Id = subjectRef,
            UserId = userId,
            FullName = PersonName.Of(fullName),
            Gender = gender,
            BirthDate = ValidateBirthDate(birthDate),
            ContactInfo = contactInfo!,
            Status = PersonProfileStatus.Active
        };

        entity.EnsureActiveInvariants(); // dùng ValidationBuilder<T>
        // DomainEvent: PersonProfileCreated
        return entity;
    }

    public static PersonProfile SeedPending(
        Guid subjectRef,
        Guid userId,
        string? fullName,
        ContactInfo contactInfo)
    {
        ValidationBuilder.Create()
            .When(() => userId == Guid.Empty)
            .WithErrorCode("INVALID_USER_ID")
            .WithMessage("User Id không được để trống.")
            .ThrowIfInvalid();

        var entity = new PersonProfile
        {
            Id = subjectRef,
            UserId = userId,
            FullName = PersonName.OfNullable(fullName),
            Gender = UserGender.NotSet,
            BirthDate = null,
            ContactInfo = contactInfo,
            Status = PersonProfileStatus.Pending
        };

        // DomainEvent: PersonProfileSeeded
        return entity;
    }

    public void CompleteOnboarding(
        UserGender gender,
        DateOnly birthDate,
        ContactInfo contactInfo)
    {
        if (Status == PersonProfileStatus.Active)
            throw new BadRequestException("Hồ sơ cá nhân đã được kích hoạt và không thể onboarding lại.",
                "PERSON_PROFILE_ALREADY_ACTIVE");

        ValidationBuilder.Create()
            .When(() => !contactInfo.HasEnoughInfo())
            .WithErrorCode("INVALID_CONTACT")
            .WithMessage("Thông tin liên hệ chưa được điền đủ.")
            .ThrowIfInvalid();

        ChangeGender(gender);
        ChangeBirthDate(birthDate);
        UpdateContact(contactInfo);

        EnsureActiveInvariants();
        Status = PersonProfileStatus.Active;

        AddDomainEvent(new PersonProfileOnboardedEvent(SubjectRef));
    }

    public void Update(string? fullName, UserGender? gender, DateOnly? birthDate, ContactInfo contactInfo)
    {
        ValidationBuilder.Create()
            .When(() => !contactInfo.HasEnoughInfo())
            .WithErrorCode("INVALID_CONTACT")
            .WithMessage("Thông tin liên hệ không được để trống.")
            .ThrowIfInvalid();

        FullName = PersonName.Of(fullName);
        Gender = gender ?? Gender;
        BirthDate = ValidateBirthDate(birthDate);
        ContactInfo = contactInfo;

        if (Status != PersonProfileStatus.Active && IsActiveReady())
        {
            EnsureActiveInvariants();
            Status = PersonProfileStatus.Active;
        }
        // DomainEvent: PersonProfileUpdated
    }

    public void Rename(string? fullName) => FullName = PersonName.Of(fullName);
    public void ChangeGender(UserGender gender) => Gender = gender;
    public void ChangeBirthDate(DateOnly birthDate) => BirthDate = ValidateBirthDate(birthDate);

    public void UpdateContact(ContactInfo contactInfo)
    {
        ValidationBuilder.Create()
            .When(() => !contactInfo.HasEnoughInfo())
            .WithErrorCode("INVALID_CONTACT")
            .WithMessage("Thông tin liên hệ không được để trống.")
            .ThrowIfInvalid();

        ContactInfo = contactInfo!;
    }

    private static DateOnly? ValidateBirthDate(DateOnly? birthDate)
    {
        if (birthDate is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        ValidationBuilder.Create()
            .When(() => birthDate > today.AddYears(-13))
            .WithErrorCode("INVALID_BIRTHDATE")
            .WithMessage("Người dùng phải ít nhất 13 tuổi.")
            .When(() => today.Year - birthDate.Value.Year > 100)
            .WithErrorCode("INVALID_BIRTHDATE")
            .WithMessage("Ngày sinh nhật quá xa so với hiện tại.")
            .ThrowIfInvalid();

        return birthDate;
    }

    private bool IsActiveReady()
    {
        var hasName = !FullName.IsEmpty;
        var hasAnyContact = ContactInfo.HasEnoughInfo();
        return hasName && hasAnyContact;
    }

    private void EnsureActiveInvariants()
    {
        ValidationBuilder<PersonProfile>.Create(this)
            .When(_ => FullName.IsEmpty)
            .WithErrorCode("PROFILE_INCOMPLETE")
            .WithMessage("Tên không được để trống khi kích hoạt hồ sơ.")
            .When(_ => !ContactInfo.HasEnoughInfo())
            .WithErrorCode("PROFILE_INCOMPLETE")
            .WithMessage("Cần ít nhất một phương thức liên hệ hợp lệ để kích hoạt hồ sơ.")
            .ThrowIfInvalid();
    }
}