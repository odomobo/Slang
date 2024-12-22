using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Slang.Core
{
    public enum ParserState
    {
        Unmatched,
        Matched,
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

        public ParserContext With(ParserState? state = null, Span<IToken> tokens = default, Error? error = null)
        {
            if (state == null)
                state = State;

            if (tokens == default)
                tokens = Tokens;

            var errors = Errors;

            if (error != null)
                errors = errors.Add(error);

            return new ParserContext(state.Value, tokens, errors);
        }

        public ParserContext Pass()
        {
            return new ParserContext(ParserState.Unmatched, Tokens, Errors);
        }

        public readonly ParserState State { get; init; }
        public readonly Span<IToken> Tokens { get; init; }
        public readonly ImmutableList<Error> Errors { get; init; }

        public static implicit operator bool(ParserContext cx) => cx.State == ParserState.Matched;

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

        public override string ToString()
        {
            if (State == ParserState.Errored && Errors.Any())
            {
                return Errors[0].ToString();
            }
            else
            {
                return $":{State}: - [{this[0]?.GetType()?.Name ?? "NULL"}, {this[1]?.GetType()?.Name ?? "NULL"}, {this[2]?.GetType()?.Name ?? "NULL"}, {this[3]?.GetType()?.Name ?? "NULL"}, {this[4]?.GetType()?.Name ?? "NULL"}, {this[5]?.GetType()?.Name ?? "NULL"}, ...]";
            }
        }
    }
}
