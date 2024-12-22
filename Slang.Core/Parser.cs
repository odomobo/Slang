using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
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
    public class Parser
    {
        public (List<IStatement> statements, List<Error> errors) Parse(IToken[] tokens)
        {
            var statements = new List<IStatement>();
            var errors = new List<Error>();

            var context = new ParserContext(ParserState.Unmatched, tokens, ImmutableList<Error>.Empty);
            
            while (context.Tokens.Length > 0 && context[0] is not Token.EndOfFile)
            {
                if (context = TryParseStatement(context.Pass(), out var statement))
                {
                    statements.Add(statement);
                }
                else
                {
                    // TODO: error handling needs to be better
                    errors.AddRange(context.Errors);

                    // only add this error if we don't have a better one
                    if (context.State != ParserState.Errored)
                        errors.Add(new Error(context.Tokens[0].Location, "Could not parse statement"));

                    break;
                }
            }
            
            return (statements, errors);
        }

        private ParserContext TryParseStatement(ParserContext initialContext, out IStatement statement)
        {
            ParserContext context = initialContext;
            if (context = TryParseExpressionAny(context.Pass(), out IExpression expression))
            {
                if (context[0] is Token.Semicolon)
                {
                    statement = new Statement.Print(expression);
                    return context.With(ParserState.Matched, context.Tokens[1..]);
                }
            }

            statement = default;
            return initialContext;
        }

        private ParserContext TryParseExpressionAny(ParserContext initialContext, out IExpression expression)
        {
            return TryParseExpressionAddSubtract(initialContext.Pass(), out expression);
        }

        private ParserContext TryParseExpressionAddSubtract(ParserContext initialContext, out IExpression expression)
        {
            ParserContext context = initialContext;
            if (context = TryParseExpressionMulDiv(context.Pass(), out var lhsExpression))
            {
                // we go in a loop to allow left-associativity
                while (true)
                {
                    var currentToken = context[0];
                    if (currentToken is not Token.Plus && currentToken is not Token.Minus)
                    {
                        expression = lhsExpression;
                        return context.With(ParserState.Matched);
                    }

                    context = context.With(tokens: context.Tokens[1..]);
                    if (! (context = TryParseExpressionMulDiv(context.Pass(), out var rhsExpression)) )
                    {
                        expression = default;

                        // We matched an expression and a plus/minus, but we couldn't find another expression.
                        // Only add an error if there isn't a deeper error.
                        if (context.State != ParserState.Errored)
                        {
                            var error = new Error(context.Tokens[0].Location, "Expected expression");
                            return initialContext.With(ParserState.Errored, error: error);
                        }
                        else
                        {
                            return initialContext;
                        }
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

            expression = default;
            return initialContext; // unmatched
        }

        private ParserContext TryParseExpressionMulDiv(ParserContext initialContext, out IExpression expression)
        {
            ParserContext context = initialContext;

            if (context = TryParseExpressionLiteralOrParen(context.Pass(), out var lhsExpression))
            {
                // we go in a loop to allow left-associativity
                while (true)
                {
                    var currentToken = context[0];
                    if (currentToken is not Token.Asterisk && currentToken is not Token.Frontslash)
                    {
                        expression = lhsExpression;
                        return context;
                    }

                    context = context.With(tokens: context.Tokens[1..]);
                    if (! (context = TryParseExpressionLiteralOrParen(context.Pass(), out var rhsExpression)) )
                    {
                        expression = default;

                        // We matched an expression and a asterisk/frontslash, but we couldn't find another expression.
                        // Only add an error if there isn't a deeper error.
                        if (context.State != ParserState.Errored)
                        {
                            var error = new Error(context.Tokens[0].Location, "Expected expression");
                            return initialContext.With(ParserState.Errored, error: error);
                        }
                        else
                        {
                            return initialContext;
                        }
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

            expression = default;
            return initialContext;
        }

        private ParserContext TryParseExpressionLiteralOrParen(ParserContext initialContext, out IExpression expression)
        {
            ParserContext context;
            if (context = TryParseExpressionLiteral(initialContext.Pass(), out expression))
            {
                return context;
            }

            if (context = TryParseExpressionParen(initialContext.Pass(), out expression))
            {
                return context;
            }

            expression = default;
            return initialContext;
        }

        private ParserContext TryParseExpressionLiteral(ParserContext initialContext, out IExpression expression)
        {
            var currentToken = initialContext[0];
            if (currentToken is Token.NumberLiteral)
            {
                expression = new Expression.DoubleLiteral((Token.NumberLiteral)currentToken);
                return initialContext.With(ParserState.Matched, initialContext.Tokens[1..]);
            }

            expression = default;
            return initialContext;
        }

        private ParserContext TryParseExpressionParen(ParserContext initialContext, out IExpression expression)
        {
            ParserContext context = initialContext;

            var currentToken = context[0];
            if (currentToken is Token.OpenParen)
            {
                // matched an open paren, but couldn't match an expression... I guess this is ok?
                if (! (context = TryParseExpressionAny(context.With(ParserState.Unmatched, context.Tokens[1..]), out var tmpExpression)) )
                {
                    expression = default;
                    return initialContext;
                }

                if (context[0] is Token.CloseParen)
                {
                    expression = tmpExpression;
                    return context.With(ParserState.Matched, context.Tokens[1..]);
                }
            }

            // We found an open paren but couldn't complete it.
            // Only add an error if there isn't a deeper error.
            if (context.State != ParserState.Errored)
            {
                expression = default;
                var error = new Error(context.Tokens[0].Location, "Expected ')'");
                return initialContext.With(ParserState.Errored, error: error);
            }
            else
            {
                expression = default;
                return initialContext;
            }
        }
    }
}
