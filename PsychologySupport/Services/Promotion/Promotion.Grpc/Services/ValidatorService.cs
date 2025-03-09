using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Promotion.Grpc.Services;

public class ValidatorService
{
    public void ValidateCreatePromotionRequest(CreatePromotionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PromotionTypeId) || !Guid.TryParse(request.PromotionTypeId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Promotion Type Id"));

        ValidateStartEndDates(request.StartDate, request.EndDate);

        ValidateCreatePromoCodesDto(request.CreatePromoCodesDto);
    }

    public void ValidateUpdatePromotionRequest(UpdatePromotionRequest request)
    {
        ValidateStartEndDates(request.StartDate, request.EndDate);

        ValidateGuid(request.PromotionId, "Promotion");
    }

    #region Common

    public void ValidateGuid(string id, string entityName)
    {
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid {entityName} Id"));
    }

    private void ValidateStartEndDates(Timestamp startDate, Timestamp endDate)
    {
        if (startDate > endDate)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Start date must be before end date"));

        if (endDate.ToDateTime() <= DateTime.UtcNow)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "End date must be in the future"));
    }

    private void ValidateCreatePromoCodesDto(CreatePromoCodesDto dto)
    {
        if (dto.Quantity < 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Promo code quantity must be greater than 0"));

        if (dto.Value is <= 0 or > 100)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Promo code value must be between 0 and 100"));

        //Extra Validations for name, and other string fields can be added here
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Promo code description cannot be empty"));

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Promo code name cannot be empty"));
    }
    #endregion
}