using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slang.Core
{
    public class Evaluator
    {
        public void EvaluateStatement(IStatement statement)
        {
            switch (statement)
            {
                case Statement.Print printStatement:
                    Console.WriteLine(EvaluateExpression(printStatement.Expression));
                    break;
                default:
                    throw new Exception("Shouldn't be able to reach here");
            }
        }

        private double EvaluateExpression(IExpression expression)
        {
            switch (expression)
            {
                case Expression.DoubleLiteral literal:
                    return literal.Token.Value;
                case Expression.DoubleAdd add:
                    return EvaluateExpression(add.Left) + EvaluateExpression(add.Right);
                case Expression.DoubleSubtract subtract:
                    return EvaluateExpression(subtract.Left) - EvaluateExpression(subtract.Right);
                case Expression.DoubleMultiply multiply:
                    return EvaluateExpression(multiply.Left) * EvaluateExpression(multiply.Right);
                case Expression.DoubleDivide divide:
                    return EvaluateExpression(divide.Left) / EvaluateExpression(divide.Right);
                default:
                    throw new Exception("Shouldn't be able to reach here");
            }
        }
    }
}
