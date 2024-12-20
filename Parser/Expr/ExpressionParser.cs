
using System.Linq.Expressions;
using System.Reflection;
using Sprache;

namespace Parser.Expr;

static class ExpressionParser
{
    // 構文解析処理
    public static Expression<Func<decimal>> ParseExpression(string text)
    {
        return Lambda.Parse(text);
    }

    // 終端記号の定義

    static readonly Parser<string> Identifier = Parse.Letter.AtLeastOnce().Text().Token();

    // 非終端記号の定義

    static Parser<ExpressionType> Operator(string op, ExpressionType opType)
    {
        return Parse.String(op).Token().Return(opType);
    }

    static readonly Parser<ExpressionType> Add = Operator("+", ExpressionType.AddChecked);
    static readonly Parser<ExpressionType> Subtract = Operator("-", ExpressionType.SubtractChecked);
    static readonly Parser<ExpressionType> Multiply = Operator("*", ExpressionType.MultiplyChecked);
    static readonly Parser<ExpressionType> Divide = Operator("/", ExpressionType.Divide);
    static readonly Parser<ExpressionType> Modulo = Operator("%", ExpressionType.Modulo);
    static readonly Parser<ExpressionType> Power = Operator("^", ExpressionType.Power);

    // BNFの定義

    static readonly Parser<Expression> Function =
        from name in Identifier
        from lparen in Parse.Char('(')
        from expr in Parse.Ref(() => Expr).DelimitedBy(Parse.Char(',').Token())
        from rparen in Parse.Char(')')
        select CallFunction(name, expr.ToArray());

    static Expression CallFunction(string name, Expression[] parameters)
    {
        var methodInfo = typeof(Math).GetTypeInfo().GetMethod(name, parameters.Select(e => e.Type).ToArray());
        if (methodInfo == null)
            throw new ParseException(string.Format("Function '{0}({1})' does not exist.", name,
                                     string.Join(",", parameters.Select(e => e.Type.Name))));

        return Expression.Call(methodInfo, parameters);
    }

    static readonly Parser<Expression> Constant =
        Parse.Decimal
        .Select(x => Expression.Constant(decimal.Parse(x)))
        .Named("number");

    static readonly Parser<Expression> Factor =
        (from lparen in Parse.Char('(')
        from expr in Parse.Ref(() => Expr)
        from rparen in Parse.Char(')')
        select expr).Named("expression")
        .XOr(Constant)
        .XOr(Function);

    static readonly Parser<Expression> Operand =
        (from sign in Parse.Char('-')
        from factor in Factor
        select Expression.Negate(factor)
        ).XOr(Factor).Token();

    // 演算子の優先順位

    static readonly Parser<Expression> InnerTerm = Parse.ChainRightOperator(Power, Operand, Expression.MakeBinary);

    static readonly Parser<Expression> Term = Parse.ChainOperator(Multiply.Or(Divide).Or(Modulo), InnerTerm, Expression.MakeBinary);

    static readonly Parser<Expression> Expr = Parse.ChainOperator(Add.Or(Subtract), Term, Expression.MakeBinary);

    // 構文解析処理

    static readonly Parser<Expression<Func<decimal>>> Lambda =
        Expr.End().Select(body => Expression.Lambda<Func<decimal>>(body));

}
