using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    // pseudo namespace
    public static partial class Token
    {
        public record Semicolon(Location Location) : IToken { }
        public record Plus(Location Location) : IToken { }
        public record Minus(Location Location) : IToken { }
        public record Asterisk(Location Location) : IToken { }
        public record Frontslash(Location Location) : IToken { }
        public record OpenParen(Location Location) : IToken { }
        public record CloseParen(Location Location) : IToken { }
    }
}
