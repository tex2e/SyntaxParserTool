
using System.CodeDom.Compiler;
using System.Text;

namespace SyntaxParserTool.Parser.WindowsBatch;

public class BatchFile(IEnumerable<IStatement>? statements)
{
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
/// <param name="Text">コメント内容</param>
public class Comment(string Text) : IStatement
{
    public override string ToString()
    {
        return $"<rem text={{{Text}}}>";
    }
}

/// <summary>
/// ラベル
/// </summary>
/// <param name="Text">ラベル名</param>
public class Label(string Text) : IStatement
{
    public override string ToString()
    {
        return $"<label text={{{Text}}}>";
    }
}

/// <summary>
/// 変数の代入
/// </summary>
/// <param name="Name">変数名</param>
/// <param name="Value">設定値</param>
public class SetVariable(string Name, string Value) : IStatement
{
    public override string ToString()
    {
        return $"<setvariable name={{{Name}}} value={{{Value}}}";
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
