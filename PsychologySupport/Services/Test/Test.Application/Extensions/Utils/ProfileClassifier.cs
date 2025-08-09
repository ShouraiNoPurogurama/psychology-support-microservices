using Test.Domain.ValueObjects;

namespace Test.Application.Extensions.Utils;

public static class ProfileClassifier
{
    public static string GetNickname(Score depression, Score anxiety, Score stress)
    {
        int d = depression.MultipliedValue;
        int a = anxiety.MultipliedValue;
        int s = stress.MultipliedValue;

        //Bình thường hết
        if (d <= 9 && a <= 7 && s <= 14)
            return "The Calm Optimist";

        //Cả ba đều cực cao (Extremely Severe)
        if (d >= 28 && a >= 20 && s >= 34)
            return "The Survivor";

        //Một chỉ số vượt trội, severe trở lên và cao nhất
        if (d >= 21 && d >= a && d >= s)
            return d >= 28 ? "The Sensitive Soul" : "The Deep Thinker";
        if (a >= 15 && a >= d && a >= s)
            return a >= 20 ? "The Worrier" : "The Alert Mind";
        if (s >= 26 && s >= d && s >= a)
            return s >= 34 ? "The Fighter" : "The Go-Getter";

        //Cả ba đều moderate, nhưng không cái nào lên severe trở lên
        bool allModerate =
            (d >= 14 && d <= 20) &&
            (a >= 10 && a <= 14) &&
            (s >= 19 && s <= 25);
        if (allModerate)
            return "The Brave Soul";

        //Nếu 2 moderate trở lên (không có severe nào)
        int moderateCount = 0;
        if (d >= 14 && d <= 20) moderateCount++;
        if (a >= 10 && a <= 14) moderateCount++;
        if (s >= 19 && s <= 25) moderateCount++;
        if (moderateCount >= 2)
            return "The Explorer";

        //Nếu 1 mild, còn lại bình thường
        int mildCount = 0;
        if (d >= 10 && d <= 13) mildCount++;
        if (a >= 8 && a <= 9) mildCount++;
        if (s >= 15 && s <= 18) mildCount++;
        if (mildCount == 1 && d <= 13 && a <= 9 && s <= 18)
            return "The Growing Mind";

        //Cả ba đều mild trở xuống (không moderate, không severe)
        if (d <= 13 && a <= 9 && s <= 18)
            return "The Sensitive Dreamer";

        //Pha trộn nhiều mức độ, nhưng không có mức nào nổi bật rõ ràng
        return "The Resilient";
    }

}
