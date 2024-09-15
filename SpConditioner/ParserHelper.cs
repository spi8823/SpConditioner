using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpConditioner
{
    internal static class ParserHelper
    {
        public static Parser<Expression> chain(Parser<ExpressionType> ope, Parser<Expression> operand) => ChainOperator(ope, operand, MakeBinary).Token();

        public static Parser<T> Chain<T, TOp>(Parser<TOp> op,
                                              Parser<T> operand,
                                              Func<TOp, T, T, T> apply)
        {
            return operand.Then(first => ChainRest(first, op, operand, apply));
        }

        public static Parser<T> ChainRest<T, TOp>(T firstOperand,
                                           Parser<TOp> op,
                                           Parser<T> operand,
                                           Func<TOp, T, T, T> apply)
        {
            try
            {
                return op.Then(opValue => operand.Then(operandValue => ChainRest(apply(opValue, firstOperand, operandValue), op, operand, apply)));
            }
            catch
            {
                return Return(firstOperand);
            }
        }

        public static Parser<Char> CharOrArray(string array)
        {
            Parser<char> result = Char(array[0]);
            for (var i = 1; i < array.Length; i++)
            {
                result = result.Or(Char(array[i]));
            }
            return result;
        }

        public static Parser<Expression> WhereType<T>(this Parser<Expression> parser) => parser.Where(e => e.Type == typeof(T));
    }
}
