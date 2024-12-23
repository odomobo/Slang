using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public class ParserPanic : Exception
    {
    }

    public class Unreachable : Exception
    {
        public Unreachable() : base("Reached unreachable line") { }
    }

    /*
     * Grammar:
     * <Statement> := <ExpressionAny> {Semicolon}
     * TODO: <PrintStatement> := {Identifier:"print"} <ExpressionAny> {Semicolon}
     * <ExpressionAny> := <ExpressionAddSubtract>
     * <ExpressionAddSubtract> := <ExpressionMulDiv> ( ({Plus} or {Minus}) <ExpressionMulDiv> )*
     * <ExpressionMulDiv> := <ExpressionLiteralOrParen> ( ({Asterisk} or {Frontslash}) <ExpressionLiteralOrParen> )*
     * <ExpressionLiteralOrParen> := <ExpressionLiteral> | <ExpressionParen>
     * <ExpressionLiteral> := {NumberLiteral}
     * <ExpressionParen> := {OpenParen} <ExpressionAny> {CloseParen}
     */

    // TODO: proper error handling
    public class Parser
    {
        private List<Error> _errors;
        private TokenHandle _tokens;

        public (List<IStatement> statements, List<Error> errors) Parse(IToken[] tokens)
        {
            // TODO: move this to a constructor maybe????
            _errors = new List<Error>();
            _tokens = new TokenHandle(tokens);

            var statements = new List<IStatement>();
            
            while (!_tokens.EndOfStream())
            {
                var statement = ParseStatement();
                statements.Add(statement);
            }
            
            return (statements, _errors);
        }

        private IStatement ParseStatement()
        {
            try
            {
                var initialState = _tokens;
                IExpression? expression;
                if (null == (expression = TryParseExpressionAny()))
                {
                    _errors.Add(_tokens.Error("Expected statement"));
                    throw new ParserPanic();
                }

                if (_tokens[0] is not Token.Semicolon)
                {
                    // We parsed an expression, but there's a missing semicolon.
                    _errors.Add(_tokens.Error("Expected semicolon"));
                    throw new ParserPanic();
                }
                
                _tokens = _tokens.Advance();
                return new Statement.Print(expression);
            } 
            catch (ParserPanic)
            {
                // advance to next semicolon, and past it
                _tokens = _tokens.AdvanceThrough(typeof(Token.Semicolon));
                return new Statement.Unknown();
            }
        }

        private IExpression? TryParseExpressionAny()
        {
            return TryParseExpressionAddSubtract();
        }

        private IExpression? TryParseExpressionAddSubtract()
        {
            var initialState = _tokens;
            IExpression? lhsExpression;
            if (null == (lhsExpression = TryParseExpressionMulDiv()) )
            {
                // this should be fine...
                _tokens = initialState;
                return null;
            }

            // we go in a loop to allow left-associativity
            while (true)
            {
                var currentToken = _tokens[0];
                if (currentToken is not Token.Plus && currentToken is not Token.Minus)
                {
                    return lhsExpression;
                }

                // advance past + or -
                _tokens = _tokens.Advance();

                IExpression? rhsExpression;
                if (null == (rhsExpression = TryParseExpressionMulDiv()) )
                {
                    // We matched an expression and a plus/minus, but we couldn't find another expression.
                    _errors.Add(_tokens.Error("Expected expression"));
                    throw new ParserPanic();
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
                    throw new Unreachable();
                }
            }
        }

        private IExpression? TryParseExpressionMulDiv()
        {
            var initialState = _tokens;
            IExpression? lhsExpression;
            if (null == (lhsExpression = TryParseExpressionLiteralOrParen()))
            {
                // this should be fine...
                _tokens = initialState;
                return null;
            }

            // we go in a loop to allow left-associativity
            while (true)
            {
                var currentToken = _tokens[0];
                if (currentToken is not Token.Asterisk && currentToken is not Token.Frontslash)
                {
                    return lhsExpression;
                }

                // advance past * or /
                _tokens = _tokens.Advance();

                IExpression? rhsExpression;
                if (null == (rhsExpression = TryParseExpressionLiteralOrParen()))
                {
                    // We matched an expression and a asterisk/frontslash, but we couldn't find another expression.
                    _errors.Add(_tokens.Error("Expected expression"));
                    throw new ParserPanic();
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

        private IExpression? TryParseExpressionLiteralOrParen()
        {
            var initialState = _tokens;

            IExpression? expression;
            if (null != (expression = TryParseExpressionLiteral()) )
            {
                return expression;
            }

            if (null != (expression = TryParseExpressionParen()) )
            {
                return expression;
            }

            _tokens = initialState;
            return null;
        }

        private IExpression? TryParseExpressionLiteral()
        {
            var currentToken = _tokens[0];
            if (currentToken is Token.NumberLiteral)
            {
                _tokens = _tokens.Advance();
                return new Expression.DoubleLiteral((Token.NumberLiteral)currentToken);
            }

            return null;
        }

        private IExpression? TryParseExpressionParen()
        {
            var initialState = _tokens;

            var currentToken = _tokens[0];
            if (currentToken is not Token.OpenParen)
            {
                _tokens = initialState;
                return null;
            }
            
            _tokens = _tokens.Advance();

            IExpression? expression;
            if (null == (expression = TryParseExpressionAny()))
            {
                // matched an open paren, but couldn't match an expression... I guess this is ok?
                _tokens = initialState;
                return null;
            }

            if (_tokens[0] is Token.CloseParen)
            {
                _tokens = _tokens.Advance();
                return expression;
            }

            // We found an open paren but couldn't complete it.
            _errors.Add(_tokens.Error("Expected ')'"));
            throw new ParserPanic();
        }
    }
}
