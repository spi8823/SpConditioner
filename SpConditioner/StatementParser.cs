namespace SpConditioner
{
    public static class StatementParser
    {
        public static Expression ParseToBoolExpression(string statement) => condition.Parse(statement);
        public static Func<IVariableAccessor, bool> ParseToBoolFunc(string statement)
        {
            var expression = ParseToBoolExpression(statement);
            return Lambda<Func<IVariableAccessor, bool>>(expression, variableParameter).Compile();
        }

        public static Expression ParseToDoubleExpression(string statement) => addsub.Parse(statement);
        public static Func<IVariableAccessor, double> ParseToDoubleFunc(string statement)
        {
            var expression = ParseToDoubleExpression(statement);
            return Lambda<Func<IVariableAccessor, double>>(ParseToDoubleExpression(statement), variableParameter).Compile();
        }

        public static Func<IVariableAccessor, bool> CompileBoolExpression(Expression expression) => Lambda<Func<IVariableAccessor, bool>>(expression, variableParameter).Compile();
        public static Func<IVariableAccessor, double> CompileDoubleExpression(Expression expression) => Lambda<Func<IVariableAccessor, double>>(expression, variableParameter).Compile();

        private static Parser<T> Chain<T, TOp>(Parser<TOp> op,
                                              Parser<T> operand,
                                              Func<TOp, T, T, T> apply)
        {
            return operand.Then(first => ChainRest(first, op, operand, apply));
        }

        private static Parser<T> ChainRest<T, TOp>(T firstOperand,
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

        private static Parser<Char> CharOrArray(string array)
        {
            Parser<char> result = Char(array[0]);
            for(var i = 1 ; i < array.Length; i++)
            {
                result = result.Or(Char(array[i]));
            }
            return result;
        }

        private static Parser<Expression> chain(Parser<ExpressionType> ope, Parser<Expression> operand) => ChainOperator(ope, operand, MakeBinary).Token();

        public static ParameterExpression variableParameter = Parameter(typeof(IVariableAccessor), "variable");

        private static Parser<ExpressionType> Operator(ExpressionType operation, string signature) => IgnoreCase(signature).Select(_ => operation).Token();
        
        private static Parser<ExpressionType> add = Ref(() => Operator(ExpressionType.Add, "+"));
        private static Parser<ExpressionType> sub = Ref(() => Operator(ExpressionType.Subtract, "-"));
        private static Parser<ExpressionType> mul = Ref(() => Operator(ExpressionType.Multiply, "*"));
        private static Parser<ExpressionType> div = Ref(() => Operator(ExpressionType.Divide, "/"));
        private static Parser<ExpressionType> mod = Ref(() => Operator(ExpressionType.Modulo, "%"));

        private static Parser<ExpressionType> equal = Ref(() => Operator(ExpressionType.Equal, "=="));
        private static Parser<ExpressionType> notEqual = Ref(() => Operator(ExpressionType.NotEqual, "!="));
        private static Parser<ExpressionType> greater = Ref(() => Operator(ExpressionType.GreaterThan, ">"));
        private static Parser<ExpressionType> greaterEqual = Ref(() => Operator(ExpressionType.GreaterThanOrEqual, ">="));
        private static Parser<ExpressionType> less = Ref(() => Operator(ExpressionType.LessThan, "<"));
        private static Parser<ExpressionType> lessEqual = Ref(() => Operator(ExpressionType.LessThanOrEqual, "<="));

        private static Parser<ExpressionType> and = Ref(() => Operator(ExpressionType.AndAlso, "&&"));
        private static Parser<ExpressionType> or = Ref(() => Operator(ExpressionType.OrElse, "||"));

        private const string signatureSpecialCharacters = "_＿&＆$＄#＃（）「」『』【】";
        private static Parser<Expression> signature = from head in Letter.Or(CharOrArray(signatureSpecialCharacters)).AtLeastOnce().Text()
                                                      from next in LetterOrDigit.Or(CharOrArray(signatureSpecialCharacters)).Many().Text().Token()
                                                      select Constant(head + next);

        private static Parser<Expression> doubleConst = from d in Parse.Decimal.Token()
                                                        select Constant(double.Parse(d));
        private static Parser<Expression> doubleVariable = from variableSignature in signature.Token()
                                                           select Call(variableParameter, typeof(IVariableAccessor).GetMethod(nameof(IVariableAccessor.GetDouble)), variableSignature);
        private static Parser<Expression> doubleOperand = Ref(() => doubleConst.Or(doubleVariable)).Token().Named("double Operand");

        private static Parser<Expression> trueConst = IgnoreCase("true").Select(_ => Constant(true)).Token();
        private static Parser<Expression> falseConst = IgnoreCase("false").Select(_ => Constant(false)).Token();
        private static Parser<Expression> boolConst = Ref(() => trueConst.Or(falseConst));
        private static Parser<Expression> boolVariable = from variableSignature in signature.Token()
                                                         select Call(variableParameter, typeof(IVariableAccessor).GetMethod(nameof(IVariableAccessor.GetBool)), variableSignature);
        private static Parser<Expression> boolOperand = Ref(() => boolConst.Or(boolVariable)).Named("bool Operand");

        private static Parser<Expression> muldiv = Ref(() => chain(mul.Or(div), doubleOperand));
        private static Parser<Expression> addsub = Ref(() => chain(add.Or(sub), muldiv));
        private static Parser<Expression> compare = Ref(() => chain(equal.Or(notEqual).Or(greater).Or(greaterEqual).Or(less).Or(lessEqual), addsub));
        private static Parser<Expression> condition = Ref(() => chain(and.Or(or), compare.Where(e => e.Type == typeof(bool)).Or(boolOperand)));
    }
}
