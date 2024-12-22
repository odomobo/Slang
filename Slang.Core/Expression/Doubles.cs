using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public static partial class Expression
    {
        public record DoubleLiteral(Token.NumberLiteral Token) : IExpression { }
        public record DoubleAdd(IToken Token, IExpression Left, IExpression Right) : IExpression { }
        public record DoubleSubtract(IToken Token, IExpression Left, IExpression Right) : IExpression { }
        public record DoubleMultiply(IToken Token, IExpression Left, IExpression Right) : IExpression { }
        public record DoubleDivide(IToken Token, IExpression Left, IExpression Right) : IExpression { }
    }
}
