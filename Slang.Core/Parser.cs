using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public class Parser
    {
        public (List<IStatement> statements, List<Error> errors) Parse(IToken[] tokens)
        {
            Span<IToken> tok = tokens;
            var statements = new List<IStatement>();
            var errors = new List<Error>();
            
            while (tok.Length > 0)
            {
                if (TryParseStatement(tok, out var newTok, out var statement))
                {
                    statements.Add(statement);
                    tok = newTok;
                }
                else
                {
                    // TODO: error handling needs to be better
                    errors.Add(new Error(tok[0].Location, "Could not parse statement"));
                    break;
                }
            }
            
            return (statements, errors);
        }

        private bool TryParseStatement(Span<IToken> tok, out Span<IToken> newTok, out IStatement statement)
        {
            if (TryParseExpressionAny(tok, out var tmpTok, out IExpression expression))
            {
                if (tmpTok.GetOrDefault(0) is Token.Semicolon)
                {
                    newTok = tmpTok[1..];
                    statement = new Statement.Print(expression);
                    return true;
                }
            }

            statement = default;
            newTok = tok;
            return false;
        }

        private bool TryParseExpressionAny(Span<IToken> tok, out Span<IToken> newTok, out IExpression expression)
        {
            return TryParseExpressionAddSubtract(tok, out newTok, out expression);
        }

        private bool TryParseExpressionAddSubtract(Span<IToken> tok, out Span<IToken> newTok, out IExpression expression)
        {
            if (TryParseExpressionMulDiv(tok, out var tmpTok, out var lhsExpression))
            {
                // we go in a loop to allow left-associativity
                while (true)
                {
                    var currentToken = tmpTok.GetOrDefault(0);
                    if (currentToken is not Token.Plus && currentToken is not Token.Minus)
                    {
                        newTok = tmpTok;
                        expression = lhsExpression;
                        return true;
                    }

                    tmpTok = tmpTok[1..];
                    if (!TryParseExpressionMulDiv(tmpTok, out tmpTok, out var rhsExpression))
                    {
                        newTok = tok;
                        expression = default;
                        return false;
                    }

                    // if it worked
                    if (currentToken is Token.Plus)
                    {
                        lhsExpression = new Expression.DoubleAdd(currentToken, lhsExpression, rhsExpression);
                    }
                    else if (currentToken is Token.Minus)
                    {
                        lhsExpression = new Expression.DoubleSubtract(currentToken, lhsExpression, rhsExpression);
                    }
                    else
                    {
                        throw new Exception("Shouldn't be able to reach here");
                    }
                }
            }

            newTok = tok;
            expression = default;
            return false;
        }

        private bool TryParseExpressionMulDiv(Span<IToken> tok, out Span<IToken> newTok, out IExpression expression)
        {
            if (TryParseExpressionLiteralOrParen(tok, out var tmpTok, out var lhsExpression))
            {
                // we go in a loop to allow left-associativity
                while (true)
                {
                    var currentToken = tmpTok.GetOrDefault(0);
                    if (currentToken is not Token.Asterisk && currentToken is not Token.Frontslash)
                    {
                        newTok = tmpTok;
                        expression = lhsExpression;
                        return true;
                    }

                    tmpTok = tmpTok[1..];
                    if (!TryParseExpressionLiteralOrParen(tmpTok, out tmpTok, out var rhsExpression))
                    {
                        newTok = tok;
                        expression = default;
                        return false;
                    }

                    // if it worked
                    if (currentToken is Token.Asterisk)
                    {
                        lhsExpression = new Expression.DoubleMultiply(currentToken, lhsExpression, rhsExpression);
                    }
                    else if (currentToken is Token.Frontslash)
                    {
                        lhsExpression = new Expression.DoubleDivide(currentToken, lhsExpression, rhsExpression);
                    }
                    else
                    {
                        throw new Exception("Shouldn't be able to reach here");
                    }
                }
            }

            newTok = tok;
            expression = default;
            return false;
        }

        private bool TryParseExpressionLiteralOrParen(Span<IToken> tok, out Span<IToken> newTok, out IExpression expression)
        {
            if (TryParseExpressionLiteral(tok, out newTok, out expression))
            {
                return true;
            }

            if (TryParseExpressionParen(tok, out newTok, out expression))
            {
                return true;
            }

            newTok = tok;
            expression = default;
            return false;
        }

        private bool TryParseExpressionLiteral(Span<IToken> tok, out Span<IToken> newTok, out IExpression expression)
        {
            var currentToken = tok.GetOrDefault(0);
            if (currentToken is Token.NumberLiteral)
            {
                newTok = tok[1..];
                expression = new Expression.DoubleLiteral((Token.NumberLiteral)currentToken);
                return true;
            }

            newTok = tok;
            expression = default;
            return false;
        }

        private bool TryParseExpressionParen(Span<IToken> tok, out Span<IToken> newTok, out IExpression expression)
        {
            var currentToken = tok.GetOrDefault(0);
            if (currentToken is Token.OpenParen)
            {
                // this should really produce an error, because we found an open paren but couldn't complete it
                if (!TryParseExpressionAny(tok[1..], out var tmpTok, out var tmpExpression))
                {
                    newTok = tok;
                    expression = default;
                    return false;
                }

                if (tmpTok.GetOrDefault(0) is Token.CloseParen)
                {
                    newTok = tmpTok[1..];
                    expression = tmpExpression;
                    return true;
                }
            }

            newTok = tok;
            expression = default;
            return false;
        }
    }
}
