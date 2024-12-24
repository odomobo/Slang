using Slang.Core;

namespace Slang
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var source = @"
(1 + 2) * 12;
12 * 1 + 2;
3 / 10000;
200 * 400;
5 - 4;
6/(3*2);
1 * 2 + 3 * (4);
";
            var filename = "<src>";

            var tokenizer = new Tokenizer();

            var (tokens, errors) = tokenizer.Tokenize(filename, source);

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                return;
            }

            var parser = new Parser();

            var (statements, errors2) = parser.Parse(tokens.ToArray());

            if (errors2.Any())
            {
                foreach (var error in errors2)
                {
                    Console.WriteLine(error);
                }
                return;
            }

            var evaluator = new Evaluator();
            foreach (var statement in statements)
            {
                evaluator.EvaluateStatement(statement);
            }
        }
    }
}
