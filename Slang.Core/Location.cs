using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public record Location (Line Line, int Position, int Length = 1)
    {
        public string EnhanceMessage(string message)
        {
            // Shows message like:
            // file1.sl:12:9: Invalid token
            // var x = %1;
            //         ^

            var line = Line.Text.TrimEnd('\r', '\n');
            return $"{Line.Filename}:{Line.LineNumber}:{Position + 1}: {message}\n{line}\n{"".PadRight(Position)}{"".PadRight(Length, '^')}\n";
        }

        public string GetTokenString()
        {
            return Line.Text.Substring(Position, Length);
        }
    }
}
