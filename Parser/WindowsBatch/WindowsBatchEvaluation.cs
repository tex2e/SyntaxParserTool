
using System.CodeDom.Compiler;
using System.Text;

namespace SyntaxParserTool.Parser.WindowsBatch;

public class BatchFile(IEnumerable<IStatement> statements)
{
    public IEnumerable<IStatement> Statements => statements;

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (statements is not null)
        {
            foreach (var statement in statements)
            {
                sb.AppendLine(statement.ToString());
            }
        }
        return sb.ToString();
    }
}

public interface IStatement
{
    public abstract string ToString();
}

/// <summary>
/// コメント
/// </summary>
/// <param name="text">コメント内容</param>
public class Comment(string text) : IStatement
{
    public string Text => text;

    public override string ToString()
    {
        return $"<rem text={{{text}}}>";
    }
}

/// <summary>
/// ラベル
/// </summary>
/// <param name="Text">ラベル名</param>
public class Label(string name) : IStatement
{
    public string Name => name;

    public override string ToString()
    {
        return $"<label name={{{name}}}>";
    }
}

/// <summary>
/// 変数の代入
/// </summary>
/// <param name="name">変数名</param>
/// <param name="value">設定値</param>
public class SetVariable(string name, string value) : IStatement
{
    public string Name => name;
    public string Value => value;

    public override string ToString()
    {
        return $"<setvariable name={{{name}}} value={{{value}}}";
    }
}

public interface ICondition {}

public class NegatedCondition(ICondition condition)
{
    public override string ToString()
    {
        return $"<not {{{condition}}}>";
    }
}

public class Exists(string Path) : ICondition
{
    public override string ToString()
    {
        return $"<exists path={{{Path}}}>";
    }
}

public class Comparison(string LeftLiteral, string Operator, string RightLiteral) : ICondition
{
    public override string ToString()
    {
        return $"<comparison left={{{LeftLiteral}}} operator={{{Operator}}} right={{{RightLiteral}}}>";
    }
}


public class IfStatement(ICondition Cond, IEnumerable<IStatement> whenTrueStatements, IEnumerable<IStatement> whenFalseStatements) : IStatement
{
    public override string ToString()
    {
        using var output = new StringWriter();
        using var writer = new IndentedTextWriter(output);
        writer.WriteLine($"<if condition={{{Cond}}}");
        writer.WriteLine($"whenTrueStatements={{");
        writer.Indent++;
        foreach (var statement in whenTrueStatements) {
            writer.WriteLine(statement.ToString());
        }
        writer.Indent--;
        writer.WriteLine("}}");
        if (whenFalseStatements is not null)
        {
            writer.WriteLine("whenFalseStatements={{");
            writer.Indent++;
            foreach (var statement in whenFalseStatements) {
                writer.WriteLine(statement.ToString());
            }
            writer.Indent--;
            writer.WriteLine("}}");
        }
        writer.WriteLine($">");
        return output.ToString();
    }

}
