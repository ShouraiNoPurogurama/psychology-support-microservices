using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.Exceptions
{
    public class MediaNotFoundException : NotFoundException
    {
        public MediaNotFoundException(string? message) : base(message)
        {
        }

        public MediaNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
