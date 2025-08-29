using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace Profile.API.Domains.Pii.Models;

public partial class PersonProfile
{
    public Guid UserId { get; set; }

    public string? FullName { get; set; }

    public UserGender Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public ContactInfo ContactInfo { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
    
    public AliasOwnerMap? AliasOwnerMap { get; set; }

    public static PersonProfile Create(
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
            UserId = userId,
            FullName = fullName,
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

        FullName = fullName;
        Gender = gender ?? Gender;
        BirthDate = birthDate;
        ContactInfo = contactInfo;
        
        //TODO Public 1 event để validate tên mới xem có vi phạm gì ko
    }

    public void Rename(string? fullName, string actor)
    {
        FullName = fullName;
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