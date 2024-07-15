using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpConditioner
{
    public class ConditionState
    {
        private Stack<List<Expression>> stack = new();

        public int depth => stack.Count;
        public void OnIf(string statement)
        {
            stack.Push(new());
            stack.Peek().Add(StatementParser.ParseToBoolExpression(statement));
        }

        public void OnElseIf(string statement)
        {
            stack.Peek().Add(StatementParser.ParseToBoolExpression(statement));
        }

        public void OnElse()
        {
            stack.Peek().Add(Constant(true));
        }

        public void OnEndIf()
        {
            stack.Pop();
        }

        public bool Check(IVariableAccessor variable)
        {
            if (stack.Count == 0)
                return true;

            Expression expression = Constant(true);
            foreach(var expressions in stack)
            {
                for(var i = 0;i < expressions.Count;i++)
                {
                    if (i == expressions.Count - 1)
                        expression = AndAlso(expression, expressions[i]);
                    else
                        expression = AndAlso(expression, Not(expressions[i]));
                }
            }

            var func = StatementParser.CompileBoolExpression(expression);
            return func.Invoke(variable);
        }
    }
}
