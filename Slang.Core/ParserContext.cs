using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public enum ParserState
    {
        Matched,
        Unmatched,
        Errored,
    }

    public readonly ref struct ParserContext
    {
        public ParserContext(ParserState state, Span<IToken> tokens, ImmutableList<Error> errors)
        {
            State = state;
            Tokens = tokens;
            Errors = errors;
        }

        public ParserContext With(ParserState state, Span<IToken> tokens = default, ImmutableList<Error>? errors = null)
        {
            if (tokens == default)
                tokens = Tokens;

            if (errors == null)
                errors = Errors;

            return new ParserContext(state, tokens, errors);
        }

        public readonly ParserState State { get; init; }
        public readonly Span<IToken> Tokens { get; init; }
        public readonly ImmutableList<Error> Errors { get; init; }

        public IToken? this[int index]
        {
            get
            {
                if (index >= Tokens.Length)
                    return null;
                else
                    return Tokens[index];
            }
        }
    }
}
