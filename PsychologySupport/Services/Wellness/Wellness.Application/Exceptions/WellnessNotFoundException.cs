using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.Exceptions
{

    public class WellnessNotFoundException : NotFoundException
    {
        public WellnessNotFoundException(string? message) : base(message)
        {
        }

        public WellnessNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
