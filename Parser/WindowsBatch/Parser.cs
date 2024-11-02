
// App-BatParser/lib/App/BatParser.pm at master · pablrod/App-BatParser
// https://github.com/pablrod/App-BatParser/blob/master/lib/App/BatParser.pm

using System.Text;
using Sprache;

namespace SyntaxParserTool.Parser.WindowsBatch;

public static class WindowsBatchParser
{
    /// <summary>
    /// 識別子の構文
    /// </summary>
    public static readonly Parser<string> identifierRule =
        (from first in Parse.Letter.Once()
        from rest in Parse.LetterOrDigit.XOr(Parse.Char('_')).Many()
        select new string(first.Concat(rest).ToArray())).Named("identifier");

    /// <summary>
    /// 単行コメントの構文
    /// </summary>
    /// <example>
    ///   REM コメント
    /// </example>
    public static readonly Parser<string> commentRule =
        from directive in Parse.IgnoreCase("REM").Text()
        from spaces in Parse.WhiteSpace
        from comment in Parse.CharExcept("\r\n").Many().Text()
        from newline in Parse.LineTerminator
        select comment;

    /// <summary>
    /// 複数行コメントの構文
    /// </summary>
    /// <example>
    ///   REM コメント1行目
    ///   REM コメント2行目
    ///   REM ...
    /// </example>
    public static readonly Parser<NodeComment> commentsRule =
        from comments in commentRule.Many()
        select new NodeComment(string.Join("\n", comments));

    /// <summary>
    /// GOTO用のラベルの構文
    /// </summary>
    /// <example>
    ///   :LABEL_EXIT
    /// </example>
    public static readonly Parser<NodeLabel> labelRule =
        (from mark in Parse.Char(':')
        from label in identifierRule
        select new NodeLabel(label)).Named("label");

    /// <summary>
    /// 変数への値代入の構文
    /// </summary>
    /// <example>
    ///   SET COUNT=123
    /// </example>
    public static readonly Parser<NodeSetVariable> setVariableRule =
        from set in Parse.IgnoreCase("SET")
        from _ in Parse.WhiteSpace.AtLeastOnce()
        from name in identifierRule
        from eq in Parse.Char('=')
        from value in Parse.CharExcept("\r\n").Many().Text()
        select new NodeSetVariable(name, value);

    /// <summary>
    /// ダブルクオーテーションで囲まれた文字列の構文
    /// </summary>
    /// <example>
    ///   "C:\path\to\my custom file.txt"
    /// </example>
    public static readonly Parser<string> quotedValue =
        from open in Parse.Char('"')
        from content in Parse.CharExcept('"').Many().Text()
        from end in Parse.Char('"')
        select content;
    
    /// <summary>
    /// ダブルクオーテーションで囲まれていない文字列の構文
    /// </summary>
    /// <example>
    ///   C:\path\to\file.txt
    /// </example>
    public static readonly Parser<string> literalValue =
        Parse.CharExcept(char.IsWhiteSpace, "literalValue").Many().Text();

    /// <summary>
    /// IFの比較の構文
    /// </summary>
    /// <example>
    ///   IF %VAR1% == 1
    /// </example>
    public static readonly Parser<ICondition> comparisonRule =
        from left in Parse.CharExcept(char.IsWhiteSpace, "leftLiteral").XMany().Text().Token()
        from ope in Parse.String("==")
                    .Or(Parse.IgnoreCase("EQU"))
                    .Or(Parse.IgnoreCase("NEQ"))
                    .Or(Parse.IgnoreCase("GTR"))
                    .Or(Parse.IgnoreCase("LSS"))
                    .Or(Parse.IgnoreCase("LEQ"))
                    .Or(Parse.IgnoreCase("GEQ"))
                    .Text()
        from right in Parse.CharExcept(char.IsWhiteSpace, "rightLiteral").XMany().Text().Token()
        select new NodeComparison(left, ope, right);

    /// <summary>
    /// IF EXISTの構文
    /// </summary>
    /// <example>
    ///   IF EXIST C:\path\to\file.txt
    /// </example>
    public static readonly Parser<ICondition> existsRule =
        from keywordExists in Parse.String("EXIST").Token()
        from path in quotedValue.XOr(literalValue)
        select new NodeExists(path);

    /// <summary>
    /// IFの条件文の構文
    /// </summary>
    public static readonly Parser<ICondition> conditionRule =
        existsRule.XOr(comparisonRule);

    /// <summary>
    /// IFの条件文の否定時の構文
    /// </summary>
    /// <example>
    ///   IF NOT EXIST 〜
    /// </example>
    public static readonly Parser<ICondition> notConditionRule =
        from not in Parse.IgnoreCase("NOT")
        from _ in Parse.WhiteSpace.AtLeastOnce()
        from cond in conditionRule
        select cond;

    /// <summary>
    /// IFの構文
    /// </summary>
    /// <example>
    ///   IF condition (
    ///     statements
    ///   ) ELSE (
    ///     statements
    ///   )
    /// </example>
    public static readonly Parser<NodeIfStatement> ifStatementRule =
        from set in Parse.IgnoreCase("IF")
        // 条件文
        from cond in notConditionRule.Token().Or(conditionRule.Token())
        // 条件がTrueのときのブロック
        from whenTrueStatements in (
            Parse.Ref(() => statementRule).Once()
            .Or(from lparen in Parse.Char('(')
                from statements in Parse.Ref(() => statementRule.XMany())
                from rparen in Parse.Char(')')
                select statements)
        )
        // 条件がFalseのときのブロック（任意）
        from whenFalseStatements in (
            from keywordElse in Parse.IgnoreCase("ELSE").Token()
            from whenFalseStatements in (
                Parse.Ref(() => statementRule).Once()
                .Or(from lparen in Parse.Char('(')
                    from statements in Parse.Ref(() => statementRule.XMany())
                    from rparen in Parse.Char(')')
                    select statements)
            )
            select whenFalseStatements
        ).Optional()
        select new NodeIfStatement(cond, whenTrueStatements, whenFalseStatements.GetOrDefault());

    /// <summary>
    /// ステートメントの構文
    /// </summary>
    public static readonly Parser<IStatement> statementRule =
        from atmark in Parse.Char('@').Optional()  // コマンド名の出力を非表示にするための「@」
        from _ in Parse.WhiteSpace.Many()
        from statement in
            execStatementRule
            .Or(commentsRule.Token())
            .Or(labelRule.Token())
            .Or(ifStatementRule.Token())
        select statement;

    /// <summary>
    /// ステートメント（コマンド実行系のみ）の構文
    /// </summary>
    public static readonly Parser<IStatement> execStatementRule =
        from statement in setVariableRule.Token()
        select statement;

    /// <summary>
    /// バッチファイルの構文
    /// </summary>
    public static readonly Parser<BatchFile> BatchFile =
        from statements in statementRule.XMany().End()
        select new BatchFile(statements);
}