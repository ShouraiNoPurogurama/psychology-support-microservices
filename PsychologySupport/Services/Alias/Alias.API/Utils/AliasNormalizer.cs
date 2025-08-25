using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Alias.API.Utils;

/// <summary>
/// Class này giúp chống mạo danh bằng homoglyph/confusable:
/// @Anh vs @Aпh (chữ “n” Cyrillic) → người dùng thấy như nhau, hệ thống coi khác nhau → fake account, lừa đảo.
/// Và nhiều lợi ích khác như:
/// - Người dùng gõ nguyen van anh, nhưng post lưu Nguyễn Văn Ánh dạng tổ hợp khác → full-text/trigram/index không hit.
/// - Fullwidth/ligature: fi (ligature ﬁ), chữ fullwidth ＡＢＣ → tokenizer/LIKE/ILIKE khó match.
/// - Dấu & hoa/thường: không normalize + không casefold → kết quả lộn xộn, khó autocomplete.
/// </summary>
public static class AliasNormalizer
{
    public static string ToKey(string label)
    {
        //Chuẩn hóa NFKC, convert tất cả variants của 1 kí tự về 1 thể đồng nhất (ví dụ cùng là số 1 nhưng khác font width,...)
        var nfkc = label.Normalize(NormalizationForm.FormKC);
        var sb = new StringBuilder(capacity: nfkc.Length);
        
        //FormD giúp tách chuỗi và kí tự có dấu thành các kí tự rời rạc để dễ xử lý
        foreach (var character in nfkc.Normalize(NormalizationForm.FormD))
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(character);
            //Ko thêm các kí tự dấu chữ vào kết quả, ví dụ Nha◌́ t  Anh => Nhat Anh
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(character);
        }

        //Chuẩn hóa lần nữa cho chắc
        var folded = sb.ToString().Normalize(NormalizationForm.FormKC);
        var collapsed = Regex.Replace(folded, "\\s+", " ").Trim();
        return collapsed.ToLowerInvariant();
    }
}