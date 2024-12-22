using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public record Error(Location Location, string Message)
    {
        public override string ToString()
        {
            return Location.EnhanceMessage(Message);
        }
    }
}
