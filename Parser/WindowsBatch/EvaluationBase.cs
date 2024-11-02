
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
public class NodeComment(string text) : IStatement
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
/// <param name="name">ラベル名</param>
public class NodeLabel(string name) : IStatement
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
public class NodeSetVariable(string name, string value) : IStatement
{
    public string Name => name;
    public string Value => value;

    public override string ToString()
    {
        return $"<setvariable name={{{name}}} value={{{value}}}";
    }
}

/// <summary>
/// GOTO文
/// </summary>
/// <param name="name">遷移先のラベル名</param>
public class NodeGoto(string name) : IStatement
{
    public string Name => name;

    public override string ToString()
    {
        return $"<goto name={{{name}}}>";
    }
}

/// <summary>
/// CALL文（ファイル呼び出し）
/// </summary>
/// <param name="name">遷移先のラベル名</param>
public class NodeCallFile(string path, IEnumerable<string> parameters) : IStatement
{
    public string Path => path;
    public IEnumerable<string> Parameters => parameters;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"<callfile path={{{path}}} parameters={{");
        sb.Append(string.Join(" ", parameters));
        sb.Append($"}}>");
        return sb.ToString();
    }
}

