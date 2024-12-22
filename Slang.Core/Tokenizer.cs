namespace Slang.Core
{
    public class Tokenizer
    {
        public (List<IToken>, List<Error>) Tokenize(string filename, string file)
        {
            var lines = ToLines(filename, file);
            var tokens = new List<IToken>();
            var errors = new List<Error>();
            foreach (var line in lines)
            {
                if (!TokenizeLine(line, ref tokens, ref errors))
                {
                    return (tokens, errors);
                }
            }

            var endOfFileToken = new Token.EndOfFile(new Location(lines.Last(), lines.Last().Text.Length));
            tokens.Add(endOfFileToken);

            return (tokens, errors);
        }

        private bool TokenizeLine(Line line, ref List<IToken> tokens, ref List<Error> errors)
        {
            int position = 0;
            while (position < line.Text.Length)
            {
                if (TryEatWhitespace(line, ref position))
                {
                    // do nothing
                }
                else if (TryGetNumberLiteral(line, ref position, out var numberLiteral))
                {
                    tokens.Add(numberLiteral);
                }
                else if (TryGetSemicolon(line, ref position, out var semicolon))
                {
                    tokens.Add(semicolon);
                }
                else if (TryGetPlus(line, ref position, out var plus))
                {
                    tokens.Add(plus);
                }
                else if (TryGetMinus(line, ref position, out var minus))
                {
                    tokens.Add(minus);
                }
                else if (TryGetAsterisk(line, ref position, out var asterisk))
                {
                    tokens.Add(asterisk);
                }
                else if (TryGetFrontslash(line, ref position, out var frontslash))
                {
                    tokens.Add(frontslash);
                }
                else if (TryGetOpenParen(line, ref position, out var leftParen))
                {
                    tokens.Add(leftParen);
                }
                else if (TryGetCloseParen(line, ref position, out var rightParen))
                {
                    tokens.Add(rightParen);
                }
                else
                {
                    errors.Add(new Error(new Location(line, position), "Unexpected token"));
                    return false;
                }
            }

            return true;
        }

        private bool TryEatWhitespace(Line line, ref int position)
        {
            int myPosition = position;
            while (myPosition < line.Text.Length)
            {
                var c = line[myPosition];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    myPosition++;
                    continue;
                }

                // if invalid character, or we ran off the end, then break;
                break;
            }

            // if we ate any whitespace characters, then return true
            if (myPosition > position)
            {
                position = myPosition;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryGetNumberLiteral(Line line, ref int position, out IToken token)
        {
            int myPosition = position;
            while (myPosition < line.Text.Length)
            {
                var c = line[myPosition];
                if (c != null && c >= '0' && c <= '9')
                {
                    myPosition++;
                    continue;
                }

                break;
            }

            if (myPosition > position)
            {
                int length = myPosition - position;
                var location = new Location(line, position, length);
                var valueString = location.GetTokenString();
                // TODO: validations that it's a valid double???
                double value = double.Parse(valueString);
                token = new Token.NumberLiteral(new Location(line, position, length), value);
                position = myPosition;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetSemicolon(Line line, ref int position, out IToken token)
        {
            var c = line[position];
            if (c == ';')
            {
                token = new Token.Semicolon(new Location(line, position));
                position++;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetPlus(Line line, ref int position, out IToken token)
        {
            var c = line[position];
            if (c == '+')
            {
                token = new Token.Plus(new Location(line, position));
                position++;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetMinus(Line line, ref int position, out IToken token)
        {
            var c = line[position];
            if (c == '-')
            {
                token = new Token.Minus(new Location(line, position));
                position++;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetAsterisk(Line line, ref int position, out IToken token)
        {
            var c = line[position];
            if (c == '*')
            {
                token = new Token.Asterisk(new Location(line, position));
                position++;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetFrontslash(Line line, ref int position, out IToken token)
        {
            var c = line[position];
            if (c == '/')
            {
                token = new Token.Frontslash(new Location(line, position));
                position++;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private bool TryGetOpenParen(Line line, ref int position, out IToken token)
        {
            var c = line[position];
            if (c == '(')
            {
                token = new Token.OpenParen(new Location(line, position));
                position++;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }
        private bool TryGetCloseParen(Line line, ref int position, out IToken token)
        {
            var c = line[position];
            if (c == ')')
            {
                token = new Token.CloseParen(new Location(line, position));
                position++;
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        private List<Line> ToLines(string filename, string file)
        {
            var ret = new List<Line>();
            var splitFile = file.Split('\n');
            for (int i = 0; i < splitFile.Length; i++)
            {
                int lineNumber = i + 1;
                ret.Add(new Line(filename, splitFile[i], lineNumber));
            }
            return ret;
        }
    }
}
