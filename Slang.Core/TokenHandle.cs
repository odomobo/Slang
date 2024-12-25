using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public record TokenHandle
    {
        public IToken[] Tokens { get; init; }
        public int Position { get; init; }
        public IToken EndOfFileToken { get; init; }

        public TokenHandle(IEnumerable<IToken> tokens)
        {
            Tokens = tokens.ToArray();
            Position = 0;
            var lastToken = tokens.LastOrDefault();
            if (lastToken != null)
            {
                EndOfFileToken = new Token.EndOfFile(new Location(lastToken.Location.Line, lastToken.Location.Line.Text.Length));
            }
            else
            {
                EndOfFileToken = new Token.EndOfFile(new Location(new Line("<???>", string.Empty, 1), 1));
            }
        }

        public IToken this[int offset]
        {
            get
            {
                // we could allow this, but let's not to keep the parser simpler
                if (offset < 0)
                    throw new InvalidOperationException();
                
                var index = offset + Position;

                if (index >= Tokens.Length)
                    return EndOfFileToken;
                else
                    return Tokens[index];
            }
        }

        [Pure]
        public TokenHandle Advance(int offset = 1)
        {
            return this with { Position = Position + offset };
        }

        [Pure]
        public TokenHandle AdvanceUntil(params Type[] types)
        {
            int i;
            for (i = Position; i < Tokens.Length; i++)
            {
                var token = Tokens[i];
                if (types.Contains(token.GetType()))
                {
                    break;
                }
            }
            return this with { Position = i };
        }

        [Pure]
        public TokenHandle AdvanceThrough(params Type[] types)
        {
            var ret = AdvanceUntil(types);
            if (!ret.EndOfStream())
            {
                return ret with { Position = ret.Position + 1 };
            }
            else
            {
                // if we ran off the end of the token list, don't try to advance it further
                return ret;
            }
        }

        [Pure]
        public bool EndOfStream()
        {
            return Position >= Tokens.Length;
        }

        [Pure]
        public Error Error(string message = "Unknown error")
        {
            return new Error(this[0].Location, message);
        }
    }
}
