using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Queries.Billing
{
    public record ValidateOrderResponse(bool IsSuccess, List<string> Errors)
    {
        public static ValidateOrderResponse Success() => new(true, new List<string>());
        public static ValidateOrderResponse Failed(string error) => new(false, new List<string> { error });
        public static ValidateOrderResponse Failed(List<string> errors) => new(false, errors);

    }
}
