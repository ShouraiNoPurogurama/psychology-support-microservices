using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Alias.API.Aliases.Utils;

public static class AliasNormalizerUtils
{
    private static readonly Regex SpaceCollapse = new("\\s+", RegexOptions.Compiled);
    private static readonly UnicodeRange[] AllowedRanges =
    {
        UnicodeRanges.BasicLatin,
        UnicodeRanges.Latin1Supplement,
        UnicodeRanges.LatinExtendedA,
        UnicodeRanges.LatinExtendedB,
        UnicodeRanges.LatinExtendedAdditional,
        UnicodeRanges.LatinExtendedC,
        UnicodeRanges.LatinExtendedD,
        UnicodeRanges.LatinExtendedE,
        UnicodeRanges.CombiningDiacriticalMarks   //cho ký tự dấu VN
    };

    /// <summary>
    /// Key dùng để UNIQUE: phân biệt đầy đủ dấu/âm, nhưng normalize width/ligature/case + chuẩn hóa khoảng trắng.
    /// Ví dụ: "Nhật  Anh" -> "nhật anh"; "Aпh" (Cyrillic n) => bị chặn ở ValidateAllowedScript.
    /// </summary>
    public static string ToUniqueKey(string label)
    {
        ValidateAllowedScript(label);

        //NFKC để gom fullwidth/ligature; KHÔNG bỏ dấu
        var nfkc = label.Normalize(NormalizationForm.FormKC);
        var lower = nfkc.ToLowerInvariant();

        var collapsed = SpaceCollapse.Replace(lower, " ").Trim();
        return collapsed;
    }

    /// <summary>
    /// Key dùng cho tìm kiếm (accent-insensitive).
    /// Ví dụ: "Nhật Anh" -> "nhat anh"; "Nguyễn  Văn  Ánh" -> "nguyen van anh".
    /// </summary>
    public static string ToSearchKey(string label)
    {
        ValidateAllowedScript(label);

        var nfkc = label.Normalize(NormalizationForm.FormKC);
        var sb = new StringBuilder(capacity: nfkc.Length);

        foreach (var ch in nfkc.Normalize(NormalizationForm.FormD))
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var noMarks = sb.ToString().Normalize(NormalizationForm.FormKC);
        var lower = noMarks.ToLowerInvariant();
        var collapsed = SpaceCollapse.Replace(lower, " ").Trim();
        return collapsed;
    }

    /// <summary>
    /// Chỉ cho phép Latin (+ Vietnamese diacritics), digits, space và 1 số ký tự an toàn.
    /// Dừng sớm nếu gặp ký tự ngoài allowlist để tránh homoglyph spoofing (Cyrillic/Greek).
    /// </summary>
    public static void ValidateAllowedScript(string label)
    {
        foreach (var rune in label.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune) || char.IsDigit((char)rune.Value))
                continue;

            if (IsInAllowedRanges(rune))
                continue;

            if (rune.Value is '_' or '-' or '.')
                continue;

            throw new ArgumentException($"Kí tự không hợp lệ trong tên bí danh: '{rune}'");
        }
    }

    private static bool IsInAllowedRanges(Rune r)
        => AllowedRanges.Any(range => r.Value >= range.FirstCodePoint && r.Value <= range.FirstCodePoint + range.Length);

    /// <summary>
    /// Bộ đôi key phục vụ lưu DB & search.
    /// </summary>
    public static (string unique_key, string search_key) BuildKeys(string label)
        => (ToUniqueKey(label), ToSearchKey(label));
}
