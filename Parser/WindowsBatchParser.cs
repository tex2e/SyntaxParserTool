
using System.Text;
using Sprache;

namespace SyntaxParserTool.WindowsBatch;

public static class WindowsBatchParser
{
    /// <summary>
    /// 識別子の構文
    /// </summary>
    public static Parser<string> identifierRule =
        (from first in Parse.Letter.Once()
        from rest in Parse.LetterOrDigit.XOr(Parse.Char('_')).Many()
        select new string(first.Concat(rest).ToArray())).Named("identifier");

    /// <summary>
    /// 単行コメントの構文
    /// </summary>
    public static Parser<string> commentRule =
        (from directive in Parse.IgnoreCase("REM").Text()
        from spaces in Parse.WhiteSpace
        from comment in Parse.CharExcept("\r\n").Many().Text()
        from newline in Parse.LineTerminator
        select comment).Named("comment");

    /// <summary>
    /// 複数行コメントの構文
    /// </summary>
    public static Parser<Comment> commentsRule =
        (from comments in commentRule.Many()
        select new Comment(string.Join("\n", comments))).Named("comments");

    /// <summary>
    /// GOTO用のラベルの構文
    /// </summary>
    public static Parser<Label> labelRule =
        (from leading in Parse.WhiteSpace.Many()
        from mark in Parse.Char(':')
        from label in identifierRule
        select new Label(label)).Named("label");

    /// <summary>
    /// 変数への値代入の構文
    /// </summary>
    public static Parser<SetVariable> setVariableRule =
        (from set in Parse.IgnoreCase("SET")
        from _ in Parse.WhiteSpace.AtLeastOnce()
        from name in identifierRule
        from eq in Parse.Char('=')
        from value in Parse.CharExcept("\r\n").Many().Text()
        select new SetVariable(name, value)).Named("setvariable");

    /// <summary>
    /// ステートメントの構文
    /// </summary>
    public static Parser<Statement> statementRule =
        commentsRule.Token<Statement>()
        .Or(labelRule.Token())
        .Or(setVariableRule.Token());

    /// <summary>
    /// バッチファイルの構文
    /// </summary>
    public static readonly Parser<BatchFile> BatchFile =
        from statements in statementRule.Many().End()
        select new BatchFile(statements);
}