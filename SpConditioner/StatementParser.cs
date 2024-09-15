using static SpConditioner.ParserHelper;

namespace SpConditioner
{
    public static class StatementParser
    {
        public static ParameterExpression variableParameter = Parameter(typeof(IVariableAccessor), "variable");
        public static Expression ParseToBoolExpression(string statement) => condition.Parse(statement);
        public static Expression ParseToDoubleExpression(string statement) => addsub.Parse(statement);
        
        public static Func<IVariableAccessor, bool> ParseToBoolFunc(string statement)
        {
            var expression = ParseToBoolExpression(statement);
            return Lambda<Func<IVariableAccessor, bool>>(expression, variableParameter).Compile();
        }
        
        public static Func<IVariableAccessor, double> ParseToDoubleFunc(string statement)
        {
            var expression = ParseToDoubleExpression(statement);
            return Lambda<Func<IVariableAccessor, double>>(ParseToDoubleExpression(statement), variableParameter).Compile();
        }

        public static bool ParseToBool(string statement, IVariableAccessor variable = null)
        {
            var func = ParseToBoolFunc(statement);
            return func.Invoke(variable);
        }

        public static double ParseToDouble(string statement, IVariableAccessor variable = null)
        {
            var func = ParseToDoubleFunc(statement);
            return func.Invoke(variable);
        }

        public static Func<IVariableAccessor, bool> CompileBoolExpression(Expression expression) => Lambda<Func<IVariableAccessor, bool>>(expression, variableParameter).Compile();
        public static Func<IVariableAccessor, double> CompileDoubleExpression(Expression expression) => Lambda<Func<IVariableAccessor, double>>(expression, variableParameter).Compile();

        #region Parser
        #region Operator
        private static Parser<ExpressionType> Operator(ExpressionType operation, string signature) => IgnoreCase(signature).Select(_ => operation).Token();

        #region Arithmetic
        private static Parser<ExpressionType> add = Ref(() => Operator(ExpressionType.Add, "+"));
        private static Parser<ExpressionType> sub = Ref(() => Operator(ExpressionType.Subtract, "-"));
        private static Parser<ExpressionType> mul = Ref(() => Operator(ExpressionType.Multiply, "*"));
        private static Parser<ExpressionType> div = Ref(() => Operator(ExpressionType.Divide, "/"));
        private static Parser<ExpressionType> mod = Ref(() => Operator(ExpressionType.Modulo, "%"));
        #endregion

        #region Comparison
        private static Parser<ExpressionType> equal = Ref(() => Operator(ExpressionType.Equal, "=="));
        private static Parser<ExpressionType> notEqual = Ref(() => Operator(ExpressionType.NotEqual, "!="));
        private static Parser<ExpressionType> greater = Ref(() => Operator(ExpressionType.GreaterThan, ">"));
        private static Parser<ExpressionType> greaterEqual = Ref(() => Operator(ExpressionType.GreaterThanOrEqual, ">="));
        private static Parser<ExpressionType> less = Ref(() => Operator(ExpressionType.LessThan, "<"));
        private static Parser<ExpressionType> lessEqual = Ref(() => Operator(ExpressionType.LessThanOrEqual, "<="));
        #endregion

        #region Condition
        private static Parser<ExpressionType> and = Ref(() => Operator(ExpressionType.AndAlso, "&&"));
        private static Parser<ExpressionType> or = Ref(() => Operator(ExpressionType.OrElse, "||"));
        #endregion
        #endregion

        #region Signature
        private const string signatureSpecialCharacters = "_＿$＄#＃（）「」『』【】";
        private static Parser<Expression> signature = from head in Letter.Or(CharOrArray(signatureSpecialCharacters)).AtLeastOnce().Text()
                                                      from next in LetterOrDigit.Or(CharOrArray(signatureSpecialCharacters)).Many().Text().Token()
                                                      select Constant(head + next);
        #endregion

        #region Bool
        private static Parser<Expression> trueConst = IgnoreCase("true").Select(_ => Constant(true)).Token();
        private static Parser<Expression> falseConst = IgnoreCase("false").Select(_ => Constant(false)).Token();
        private static Parser<Expression> boolConst = Ref(() => trueConst.Or(falseConst));
        private static Parser<Expression> boolVariable = from variableSignature in signature.Token()
                                                         select Call(variableParameter, typeof(IVariableAccessor).GetMethod(nameof(IVariableAccessor.GetBool)), variableSignature);
        private static Parser<Expression> boolOperand = Ref(() => boolConst.Or(boolVariable)).Named("bool オペランド");
        private static Parser<Expression> boolOrNot(Parser<Expression> exp) => (from not in Char('!').Token()
                                                                             from b in exp.Token()
                                                                             select Not(b)).Or(exp);
        #endregion

        #region Double
        private static Parser<Expression> doubleConst = from d in Parse.Decimal.Token()
                                                        select Constant(double.Parse(d));
        private static Parser<Expression> doubleVariable = from variableSignature in signature.Token()
                                                           select Call(variableParameter, typeof(IVariableAccessor).GetMethod(nameof(IVariableAccessor.GetDouble)), variableSignature);
        private static Parser<Expression> doubleOperand = Ref(() => doubleConst.Or(doubleVariable)).Token().Named("double Operand");
        #endregion

        private static Parser<Expression> bracket(Parser<Expression> exp) => (from start in Char('(').Token()
                                                                              from content in Ref(() => exp).Token()
                                                                              from end in Char(')').Token()
                                                                              select content).Named($"bracket ({exp})");

        private static Parser<Expression> muldiv = Ref(()
            => chain(
                mul.Or(div),
                doubleOperand.Or(bracket(addsub))
                )).Named("MulDiv");

        private static Parser<Expression> addsub = Ref(()
            => chain(
                add.Or(sub),
                muldiv.Or(bracket(muldiv))
                )).Named("AddSub");

        private static Parser<Expression> compare = Ref(()
            => chain(
                equal.Or(notEqual).Or(greater).Or(greaterEqual).Or(less).Or(lessEqual),
                addsub.Or(bracket(addsub))
                )).Named("Compare");

        private static Parser<Expression> condition = Ref(()
            => chain(
                and.Or(or),
                boolOrNot(compare.WhereType<bool>().Or(boolOperand)).Or(boolOrNot(bracket(condition)))
                )).Named("Condition");
        #endregion
    }
}
