using ChatBox.API.Domains.AIChats.Dtos.AI;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

/// <summary>
/// Dịch vụ xử lý việc "claim" (nhận) sticker thưởng.
/// </summary>
public interface IStickerRewardService
{
    /// <summary>
    /// Người dùng nhận sticker đang chờ.
    /// Thao tác này sẽ:
    /// 1. Tìm sticker chưa claim.
    /// 2. Gọi LLM (Emo) để sinh câu chat tiếp lời (dựa trên prompt_filter và lịch sử chat).
    /// 3. Tạo một tin nhắn chat (dạng đặc biệt) trong session chính xác.
    /// 4. Đánh dấu sticker đã được claim.
    /// 5. Trả về tin nhắn chat mới được tạo.
    /// </summary>
    /// <param name="userId">ID của người dùng đang claim</param>
    /// <returns>Tin nhắn chat mới (dạng DTO) chứa sticker và câu text của Emo</returns>
    Task<AIMessageResponseDto> ClaimStickerAsync(Guid userId);
}