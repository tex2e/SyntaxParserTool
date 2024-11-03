
using System.CodeDom.Compiler;

namespace SyntaxParserTool.Parser.WindowsBatch;

public interface ICondition {}

/// <summary>
/// IF文の否定条件
/// </summary>
/// <param name="condition">条件文</param>
public class NodeNegatedCondition(ICondition condition) : ICondition
{
    public ICondition Condition => condition;

    public override string ToString()
    {
        return $"<not {{{condition}}}>";
    }
}

/// <summary>
/// IF文のEXIST条件
/// </summary>
/// <param name="path"></param>
public class NodeExists(string path) : ICondition
{
    public string Path => path;

    public override string ToString()
    {
        return $"<exists path={{{path}}}>";
    }
}

/// <summary>
/// IF文の比較条件
/// </summary>
/// <param name="leftLiteral">左辺</param>
/// <param name="ope">比較演算子</param>
/// <param name="rightLiteral">右辺</param>
public class NodeComparison(
    string leftLiteral, 
    string ope, 
    string rightLiteral) : ICondition
{
    public string LeftLiteral => leftLiteral;
    public string Ope => ope;
    public string RightLiteral => rightLiteral;

    public override string ToString()
    {
        return $"<comparison left={{{leftLiteral}}} ope={{{ope}}} right={{{rightLiteral}}}>";
    }
}

/// <summary>
/// IF文
/// </summary>
/// <param name="Cond">条件文</param>
/// <param name="whenTrueStatements">Trueのときのブロック</param>
/// <param name="whenFalseStatements">Falseのときのブロック</param>
public class NodeIfStatement(
    ICondition condition, 
    IEnumerable<IStatement> whenTrueStatements, 
    IEnumerable<IStatement> whenFalseStatements) : IStatement
{
    public ICondition Condition => condition;
    public IEnumerable<IStatement> WhenTrueStatements => whenTrueStatements;
    public IEnumerable<IStatement> WhenFalseStatements => whenFalseStatements;

    public override string ToString()
    {
        using var output = new StringWriter();
        using var writer = new IndentedTextWriter(output);
        writer.WriteLine($"<if condition={{{condition}}} whenTrueStatements={{");
        writer.Indent++;
        foreach (var statement in whenTrueStatements) {
            writer.WriteLine(statement.ToString());
        }
        writer.Indent--;
        writer.Write("}}");
        if (whenFalseStatements is not null)
        {
            writer.WriteLine("whenFalseStatements={{");
            writer.Indent++;
            foreach (var statement in whenFalseStatements) {
                writer.WriteLine(statement.ToString());
            }
            writer.Indent--;
            writer.Write("}}");
        }
        writer.WriteLine($">");
        return output.ToString();
    }
}
