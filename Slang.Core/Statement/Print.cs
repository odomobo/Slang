using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    // pseudo namespace
    public static partial class Statement
    {
        internal record Print(IExpression Expression) : IStatement {}
    }
}
