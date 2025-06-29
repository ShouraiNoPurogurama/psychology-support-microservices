using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Promotion.Grpc.Services;

public class ValidatorService
{
    public void ValidateCreatePromotionRequest(CreatePromotionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PromotionTypeId) || !Guid.TryParse(request.PromotionTypeId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Promotion Type không hợp lệ"));

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
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"ID của {entityName} không hợp lệ."));
    }

    private void ValidateStartEndDates(Timestamp startDate, Timestamp endDate)
    {
        if (startDate > endDate)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Ngày bắt đầu phải trước ngày kết thúc."));

        if (endDate.ToDateTime() <= DateTime.UtcNow)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Ngày kết thúc phải nằm trong tương lai."));
    }

    private void ValidateCreatePromoCodesDto(CreatePromoCodesDto dto)
    {
        if (dto.Quantity < 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Số lượng mã khuyến mãi phải lớn hơn 0."));

        if (dto.Value is <= 0 or > 100)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Giá trị mã khuyến mãi (%) phải nằm trong khoảng từ 1 đến 100."));

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Mô tả mã khuyến mãi không được để trống."));

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Tên mã khuyến mãi không được để trống."));
    }
    #endregion

}