using Test.Domain.ValueObjects;

namespace Test.Application.Extensions.Utils;

public static class ProfileClassifier
{
    public static string GetNickname(Score depression, Score anxiety, Score stress)
    {
        // DASS21 chuẩn: (đã nhân đôi)
        // Depression: Mild(10-13), Moderate(14-20), Severe(21-27), Extremely(28+)
        // Anxiety: Mild(8-9), Moderate(10-14), Severe(15-19), Extremely(20+)
        // Stress: Mild(15-18), Moderate(19-25), Severe(26-33), Extremely(34+)

        int d = depression.MultipliedValue;
        int a = anxiety.MultipliedValue;
        int s = stress.MultipliedValue;

        //Bình thường hết
        if (d <= 9 && a <= 7 && s <= 14)
            return "The Calm Optimist";

        //Cả ba đều cực cao (Extremely Severe)
        if (d >= 28 && a >= 20 && s >= 34)
            return "The Survivor";

        //Cả ba đều moderate trở lên (>= moderate)
        if (d >= 14 && a >= 10 && s >= 19)
            return "The Brave Soul";

        //Cả ba đều mild trở xuống (tối đa 1 chỉ số lên moderate)
        if (d <= 13 && a <= 9 && s <= 18)
            return "The Sensitive Dreamer";

        //Chỉ số trầm cảm là cao nhất và severe trở lên
        if (d >= 21 && d >= a && d >= s)
        {
            if (d >= 28) return "The Sensitive Soul";
            if (d >= 21) return "The Deep Thinker";
        }

        //Chỉ số lo âu là cao nhất và severe trở lên
        if (a >= 15 && a >= d && a >= s)
        {
            if (a >= 20) return "The Worrier";
            if (a >= 15) return "The Alert Mind";
        }

        //Chỉ số stress là cao nhất và severe trở lên
        if (s >= 26 && s >= d && s >= a)
        {
            if (s >= 34) return "The Fighter";
            if (s >= 26) return "The Go-Getter";
        }

        //Nếu moderate ở 2 nhóm, 1 nhóm bình thường
        int moderateCount = 0;
        if (d >= 14 && d <= 20) moderateCount++;
        if (a >= 10 && a <= 14) moderateCount++;
        if (s >= 19 && s <= 25) moderateCount++;
        if (moderateCount >= 2)
            return "The Explorer";

        //Nếu một chỉ số mild, còn lại đều normal
        int mildCount = 0;
        if (d >= 10 && d <= 13) mildCount++;
        if (a >= 8 && a <= 9) mildCount++;
        if (s >= 15 && s <= 18) mildCount++;
        if (mildCount == 1 && d <= 13 && a <= 9 && s <= 18)
            return "The Growing Mind";

        //Còn lại (có sự pha trộn các mức, nhưng không quá cao)
        return "The Resilient";
    }
}
