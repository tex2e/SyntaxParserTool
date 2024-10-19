
using System.Text;
using Sprache;

namespace SyntaxParserTool.WindowsBatch;

public class JCL(IEnumerable<Statement>? statements)
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

public interface Statement
{
    public abstract string ToString();

}

public class Comment(string Text) : Statement
{
    public override string ToString()
    {
        return $"<rem text={{{Text}}}>";
    }
}

public class Label(string Text) : Statement
{
    public override string ToString()
    {
        return $"<label text={{{Text}}}>";
    }
}

public class SetVariable(string Name, string Value) : Statement
{
    public override string ToString()
    {
        return $"<setvariable name={{{Name}}} value={{{Value}}}";
    }
}

public static class WindowsBatchParser
{
    // 識別子の定義
    public static Parser<string> identifierRule =
        (from first in Parse.Letter.Once()
        from rest in Parse.LetterOrDigit.XOr(Parse.Char('_')).Many()
        select new string(first.Concat(rest).ToArray())).Named("identifier");

    // -------------------------------------------------------------------------
    // コメントの定義

    public static Parser<string> commentRule =
        (from directive in Parse.IgnoreCase("REM").Text()
        from spaces in Parse.WhiteSpace
        from comment in Parse.CharExcept("\r\n").Many().Text()
        from newline in Parse.LineTerminator
        select comment).Named("comment");

    public static Parser<Comment> commentsRule =
        (from comments in commentRule.Many()
        select new Comment(string.Join("\n", comments))).Named("comments");

    // -------------------------------------------------------------------------
    // ラベルの定義

    public static Parser<Label> labelRule =
        (from leading in Parse.WhiteSpace.Many()
        from mark in Parse.Char(':')
        from label in identifierRule
        select new Label(label)).Named("label");

    // -------------------------------------------------------------------------
    // 変数代入の定義
    public static Parser<SetVariable> setVariableRule =
        (from set in Parse.IgnoreCase("SET")
        from _ in Parse.WhiteSpace.AtLeastOnce()
        from name in identifierRule
        from eq in Parse.Char('=')
        from value in Parse.CharExcept("\r\n").Many().Text()
        select new SetVariable(name, value)).Named("setvariable");

    // -------------------------------------------------------------------------
    // 行の定義

    public static Parser<Statement> statementRule =
        commentsRule.Token<Statement>()
        .Or(labelRule.Token())
        .Or(setVariableRule.Token());

    public static readonly Parser<JCL> JCL =
        from statements in statementRule.Many().End()
        select new JCL(statements);
}