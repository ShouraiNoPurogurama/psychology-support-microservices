using BuildingBlocks.Data.Common;
using BuildingBlocks.DDD;
using Profile.API.ValueObjects.Pii;

namespace Profile.API.Models.Pii;

public partial class PersonProfile : IHasCreationAudit, IHasModificationAudit
{
    public Guid SubjectRef { get; private set; }

    public Guid UserId { get; private set; }

    public PersonName FullName { get; private set; }

    public UserGender Gender { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public ContactInfo ContactInfo { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public AliasOwnerMap? AliasOwnerMap { get; set; }

    public static PersonProfile Create(
        Guid subjectRef,
        Guid userId,
        string? fullName,
        UserGender? gender,
        DateOnly? birthDate,
        ContactInfo contactInfo)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User Id không được để trống.", nameof(userId));
        if (contactInfo is null) throw new ArgumentNullException(nameof(contactInfo));

        var entity = new PersonProfile
        {
            SubjectRef = subjectRef,
            UserId = userId,
            FullName = PersonName.Of(fullName),
            Gender = gender ?? UserGender.Else,
            BirthDate = birthDate,
            ContactInfo = contactInfo
        };

        //TODO Public 1 event để validate tên mới xem có vi phạm gì ko

        return entity;
    }

    public void Update(
        string? fullName,
        UserGender? gender,
        DateOnly? birthDate,
        ContactInfo contactInfo)
    {
        if (contactInfo is null) throw new ArgumentNullException(nameof(contactInfo));

        FullName = PersonName.Of(fullName);
        Gender = gender ?? Gender;
        BirthDate = birthDate;
        ContactInfo = contactInfo;

        //TODO Public 1 event để validate tên mới xem có vi phạm gì ko
    }

    public void Rename(string? fullName)
    {
        FullName = PersonName.Of(fullName);
    }

    public void ChangeGender(UserGender gender)
    {
        Gender = gender;
    }

    public void ChangeBirthDate(DateOnly birthDate)
    {
        BirthDate = ValidateBirthDate(birthDate);
    }

    public void UpdateContact(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
    }


    private static DateOnly? ValidateBirthDate(DateOnly? birthDate)
    {
        if (birthDate is null) return null;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (birthDate > today) throw new ArgumentOutOfRangeException(nameof(birthDate), "Ngày sinh nhật phải ở quá khứ.");
        if (today.Year - birthDate.Value.Year > 100)
            throw new ArgumentOutOfRangeException(nameof(birthDate), "Ngày sinh nhật quá xa so với hiện tại.");
        return birthDate;
    }
}