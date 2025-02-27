using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Domain.ValueObjects
{
    public record Recommendation
    {
        public string Value { get; }

        public Recommendation(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Recommendation cannot be empty.");

            if (value.Length > 1000)
                throw new ArgumentException("Recommendation cannot exceed 1000 characters.");

            Value = value;
        }

        public override string ToString() => Value;
    }
}
