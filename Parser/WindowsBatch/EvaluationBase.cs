
using System.CodeDom.Compiler;
using System.Text;

namespace Parser.WindowsBatch;

public interface INode
{
    public abstract string ToString();
    public abstract void Write(IndentedTextWriter indentWriter);
}


public class BatchFile(IEnumerable<IStatement> statements) : INode
{
    public IEnumerable<IStatement> Statements => statements;

    /// <summary>
    /// 現在のオブジェクトを表す文字列を返す
    /// </summary>
    /// <returns>文字列</returns>
    public override string ToString()
    {
        using var textWriter = new StringWriter();
        using var indentWriter = new IndentedTextWriter(textWriter);
        Write(indentWriter);
        return textWriter.ToString() ?? "";
    }

    /// <summary>
    /// インデント調整ありで文字列を出力する
    /// </summary>
    /// <param name="indentWriter"></param>
    public void Write(IndentedTextWriter indentWriter)
    {
        if (statements is not null)
        {
            foreach (var statement in statements)
            {
                statement.Write(indentWriter);
                indentWriter.WriteLine();
            }
        }
    }
}

public interface IStatement : INode
{}

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

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.Write(ToString());
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

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.Write(ToString());
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

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.Write(ToString());
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

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.Write(ToString());
    }
}

/// <summary>
/// CALL文（ファイル呼び出し）
/// </summary>
/// <param name="name">遷移先のラベル名</param>
public class NodeCall(string name, IEnumerable<string> parameters) : IStatement
{
    public string Name => name;
    public IEnumerable<string> Parameters => parameters;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"<callfile name={{{name}}} parameters={{");
        if (parameters is not null)
            sb.Append(string.Join(",", parameters));
        sb.Append($"}}>");
        return sb.ToString();
    }

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.Write(ToString());
    }
}

/// <summary>
/// ECHO命令
/// </summary>
/// <param name="message"></param>
public class NodeEcho(string message, bool escapeMode = false, IEnumerable<Redirection>? redirects = null) : IStatement
{
    public string Message => message;
    public bool EscapeMode => escapeMode;
    public IEnumerable<Redirection>? Redirects => redirects;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("<echo message={");
        sb.Append(message);
        if (redirects is not null)
        {
            sb.Append("} redirect={");
            foreach (var redirect in redirects)
            {
                sb.Append(redirect);
                sb.Append(" ");
            }
        }
        sb.Append("}>");
        return sb.ToString();
    }

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.Write(ToString());
    }
}

public class NodeAny(string command, IEnumerable<Redirection>? redirects = null) : IStatement
{
    public string Command => command;
    public IEnumerable<Redirection>? Redirects => redirects;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("<any command={");
        sb.Append(command);
        if (redirects is not null)
        {
            sb.Append("} redirect={");
            foreach (var redirect in redirects)
            {
                sb.Append(redirect);
                sb.Append(" ");
            }
        }
        sb.Append("}>");
        return sb.ToString();
    }

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.Write(ToString());
    }
}

