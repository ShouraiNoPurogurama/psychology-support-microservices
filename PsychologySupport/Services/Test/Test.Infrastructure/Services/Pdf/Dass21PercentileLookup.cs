using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace Test.Infrastructure.Services.Pdf
{
    public class Dass21PercentileLookup
    {
        private readonly Dictionary<(string scale, int rawScore), int> _percentileTable = new();

        public Dass21PercentileLookup(string csvPath)
        {
            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Dass21PercentileRecord>();
            foreach (var rec in records)
            {
                if (rec.Depression.HasValue)
                    _percentileTable.Add(("depression", rec.RawScore), rec.Depression.Value);
                if (rec.Anxiety.HasValue)
                    _percentileTable.Add(("anxiety", rec.RawScore), rec.Anxiety.Value);
                if (rec.Stress.HasValue)
                    _percentileTable.Add(("stress", rec.RawScore), rec.Stress.Value);
                if (rec.Total.HasValue)
                    _percentileTable.Add(("total", rec.RawScore), rec.Total.Value);
            }
        }

        public int? GetPercentile(string scale, int rawScore)
        {
            return _percentileTable.TryGetValue((scale.ToLower(), rawScore), out var pct) ? pct : null;
        }

        private class Dass21PercentileRecord
        {
            [Name("raw_score")]
            public int RawScore { get; set; }
            [Name("depression")]
            public int? Depression { get; set; }
            [Name("anxiety")]
            public int? Anxiety { get; set; }
            [Name("stress")]
            public int? Stress { get; set; }
            [Name("total")]
            public int? Total { get; set; }
        }
    }
}