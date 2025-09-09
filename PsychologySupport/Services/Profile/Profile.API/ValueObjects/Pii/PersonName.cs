using BuildingBlocks.Utils;

namespace Profile.API.ValueObjects.Pii;

public record PersonName
{
    public string Value { get; }
    private PersonName(string value) => Value = value;

    public static PersonName Of(string? name)
    {
        ValidationBuilder.Create()
            .When(() => string.IsNullOrWhiteSpace(name))
            .WithErrorCode("INVALID_PERSON_NAME")
            .WithMessage("Tên không được để trống.")
            
            .When(() => name is not null && name.Length > 100)
            .WithErrorCode("INVALID_PERSON_NAME")
            .WithMessage("Tên không được vượt quá 100 ký tự.")
            .ThrowIfInvalid();
        

        return new PersonName(name!);
    }
}