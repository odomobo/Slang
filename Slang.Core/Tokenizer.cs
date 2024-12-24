using System.Text.RegularExpressions;

namespace Slang.Core
{
    public class Tokenizer
    {
        private List<Error> _errors;
        private CharHandle _chars;

        public (List<IToken>, List<Error>) Tokenize(string filename, string file)
        {
            var lines = ToLines(filename, file);
            List<IToken> tokens = new List<IToken>();
            _chars = new CharHandle(file, lines);

            _errors = new List<Error>();

            while (true)
            {
                var token = TryGetNextToken();
                if (token == null)
                    break;

                tokens.Add(token);
            }

            return (tokens, _errors);
        }

        private IToken? TryGetNextToken()
        {
            TryEatWhitespace();

            if (_chars.EndOfStream())
                return null;

            IToken token;

            if (TryGetNumberLiteral(out token))
                return token;

            if (TryGetSemicolon(out token))
                return token;

            if (TryGetPlus(out token))
                return token;

            if (TryGetMinus(out token))
                return token;

            if (TryGetAsterisk(out token))
                return token;

            if (TryGetFrontslash(out token))
                return token;

            if (TryGetOpenParen(out token))
                return token;

            if (TryGetCloseParen(out token))
                return token;

            return GetUnknownToken();
        }

        private static readonly HashSet<char> WhitespaceChars = new HashSet<char> { ' ', '\t', '\r', '\n' };
        private bool IsWhitespace(char? c)
        {
            if (c == null)
                return false;

            return WhitespaceChars.Contains(c.Value);
        }

        private void TryEatWhitespace()
        {
            while (IsWhitespace(_chars[0]))
            {
                _chars = _chars.Advance();
            }
        }

        private bool IsDigit(char? c)
        {
            if (c == null)
                return false;

            return c.Value >= '0' && c.Value <= '9';
        }

        private bool TryGetNumberLiteral(out IToken token)
        {
            var initialState = _chars;

            while (IsDigit(_chars[0]))
            {
                _chars = _chars.Advance();
            }

            if (_chars == initialState)
            {
                token = default;
                return false;
            }

            var valueString = initialState.GetString(_chars);
            double value = double.Parse(valueString);
            var location = initialState.Location(_chars);
            token = new Token.NumberLiteral(location, value);
            return true;
        }

        private bool TryGetSemicolon(out IToken token)
        {
            if (_chars[0] == ';')
            {
                token = new Token.Semicolon(_chars.Location());
                _chars = _chars.Advance();
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetPlus(out IToken token)
        {
            if (_chars[0] == '+')
            {
                token = new Token.Plus(_chars.Location());
                _chars = _chars.Advance();
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetMinus(out IToken token)
        {
            if (_chars[0] == '-')
            {
                token = new Token.Minus(_chars.Location());
                _chars = _chars.Advance();
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetAsterisk(out IToken token)
        {
            if (_chars[0] == '*')
            {
                token = new Token.Asterisk(_chars.Location());
                _chars = _chars.Advance();
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetFrontslash(out IToken token)
        {
            if (_chars[0] == '/')
            {
                token = new Token.Frontslash(_chars.Location());
                _chars = _chars.Advance();
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetOpenParen(out IToken token)
        {
            if (_chars[0] == '(')
            {
                token = new Token.OpenParen(_chars.Location());
                _chars = _chars.Advance();
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }
        private bool TryGetCloseParen(out IToken token)
        {
            if (_chars[0] == ')')
            {
                token = new Token.CloseParen(_chars.Location());
                _chars = _chars.Advance();
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private IToken GetUnknownToken()
        {
            var initialState = _chars;
            while (!_chars.EndOfStream() && !IsWhitespace(_chars[0]))
            {
                _chars = _chars.Advance();
            }

            var error = initialState.Error("Unknown token", _chars);
            _errors.Add(error);

            return new Token.Unknown(initialState.Location(_chars));
        }

        private static readonly Regex ToLinesRegex = new Regex(@"(?<=\n)");
        private List<Line> ToLines(string filename, string file)
        {
            var ret = new List<Line>();
            var splitFile = ToLinesRegex.Split(file);
            for (int i = 0; i < splitFile.Length; i++)
            {
                int lineNumber = i + 1;
                ret.Add(new Line(filename, splitFile[i], lineNumber));
            }
            return ret;
        }
    }
}
