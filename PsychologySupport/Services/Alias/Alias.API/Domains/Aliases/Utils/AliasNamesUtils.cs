namespace Alias.API.Domains.Aliases.Utils;

public static class AliasNamesUtils
{
    public static readonly string[] Animals = new[]
    {
        "Gấu", "Mèo", "Cáo", "Thỏ", "Cú", "Hải Ly", "Hổ", "Nhím",
        "Khỉ", "Sóc", "Chó Sói", "Bướm", "Chim Sẻ", "Cừu", "Hươu Cao Cổ",
        "Cá Heo", "Rùa Biển", "Ếch Xanh", "Cá Mập", "Ngựa Vằn", "Lạc Đà"
    };
    
    public static readonly string[] Adjectives = new[]
    {
        "Vui Vẻ", "Buồn Ngủ", "Lấp Lánh", "Đáng Yêu", "Huyền Bí",
        "Tinh Nghịch", "Lãng Tử", "Ngốc Nghếch", "Bí Ẩn", "Mơ Mộng",
        "Nhanh Nhẹn", "Chậm Chạp", "Đêm Khuya", "Ban Mai", "Lười Biếng",
        "Khó Đoán", "Hồn Nhiên", "Thần Tốc", "Xinh Đẹp", "Dễ Thương"
    };
    
    public static List<(string Key, string Label)> GenerateCandidates(int poolSize)
    {
        var candidates = new List<(string Key, string Label)>(poolSize);
        var seenKeys = new HashSet<string>();

        while (candidates.Count < poolSize)
        {
            var adjective = Adjectives[Random.Shared.Next(Adjectives.Length)];
            var animal = Animals[Random.Shared.Next(Animals.Length)];
            var number = Random.Shared.Next(100, 1000); //inclusive [100..999]

            var label = $"{adjective} {animal} #{number}";
            var key = AliasNormalizerUtils.ToUniqueKey(label);

            if (seenKeys.Add(key))
            {
                candidates.Add((key, label));
            }
        }

        return candidates;
    }
}