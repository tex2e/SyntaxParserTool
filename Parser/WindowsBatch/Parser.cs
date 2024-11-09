
// App-BatParser/lib/App/BatParser.pm at master · pablrod/App-BatParser
// https://github.com/pablrod/App-BatParser/blob/master/lib/App/BatParser.pm

using System.Text.RegularExpressions;
using Sprache;

namespace Parser.WindowsBatch;

public static class WindowsBatchParser
{
    /// <summary>
    /// 改行以外の空白
    /// </summary>
    public static readonly Parser<char> spaceRule =
        Parse.WhiteSpace.Except(Parse.Char('\r')).Except(Parse.Char('\n'));

    /// <summary>
    /// 識別子の構文
    /// </summary>
    public static readonly Parser<string> identifierRule =
        (from first in Parse.Letter.Once()
        from rest in Parse.LetterOrDigit.XOr(Parse.Char('_')).Many()
        select new string(first.Concat(rest).ToArray())).Named("identifier");

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
    /// 単行コメントの構文
    /// </summary>
    /// <example>
    ///   REM コメント
    /// </example>
    public static readonly Parser<string> commentRule =
        from directive in Parse.IgnoreCase("REM").Text()
        from spaces in spaceRule
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
        from comments in commentRule.AtLeastOnce()
        select new NodeComment(string.Join("\n", comments));

    /// <summary>
    /// リダイレクト
    /// </summary>
    public static readonly Parser<Redirection> redirectionRule =
        from redirectionHandles in (
            Parse.Chars('0', '1', '2')
            .Once().Text()
        ).Optional()
        from redirectMode in
            Parse.String(">>")
            .Or(Parse.String(">&"))
            .Or(Parse.String(">"))
            .Or(Parse.String("<&"))
            .Or(Parse.String("<"))
            .Text()
        from _ in spaceRule.XMany()
        from filename in (
            Parse.Number
            .Or(quotedValue.XOr(literalValue))
        )
        select new Redirection(
            (redirectionHandles.GetOrDefault() ?? "") + redirectMode,
            filename
        );

    /// <summary>
    /// ECHOコマンド
    /// </summary>
    public static readonly Parser<NodeEcho> echoRule =
        from keywordEcho in Parse.IgnoreCase("ECHO")
        from escapeMode in
            Parse.Char(':').Return(true)
            .XOr(Parse.Char('.').Return(false))
            .XOr(spaceRule.AtLeastOnce().Return(false))
        // メッセージ
        //   注意：リダイレクトの 1> が存在するときに数字の1の部分を食べないように正規表現を作る
        from message in Parse.Regex(@"(?:[012]+(?!>)|[^\r\n><|&012]*)*")
        // リダイレクト
        from redirect in redirectionRule.Optional()
        select new NodeEcho(message, escapeMode, redirect.GetOrDefault());

    public static readonly Parser<string> pipelineKeywordRule =
        from _whitespace1 in spaceRule.XMany()
        from pipelineKeyword in
            Parse.String("&&")
            .Or(Parse.String("||"))
            .Or(Parse.String("&"))
            .Or(Parse.String("|"))
            .Text()
        from _whitespace2 in spaceRule.XMany()
        select pipelineKeyword;

    public static readonly Parser<IStatement> pipelineRule =
        Parse.ChainOperator(
            pipelineKeywordRule,
            Parse.Ref(() => execStatementRule.Token()),
            // Parse.Ref(() => controlStatementRule.Token()).Or(Parse.Ref(() => execStatementRule.Token())),
            (op, left, right) => new NodePipeline(left, op, right))
        .Token();


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
    /// GOTO命令の構文
    /// </summary>
    public static readonly Parser<NodeGoto> gotoRule =
        from keywordGoto in Parse.IgnoreCase("GOTO")
        from _ in spaceRule.AtLeastOnce()
        from label in identifierRule
        select new NodeGoto(label);

    /// <summary>
    /// CALL命令の構文（ファイル / ラベル）
    /// </summary>
    /// <example>
    ///   CALL Sample.cmd arg1 "custom message" 123
    ///   call :subroutine "%%G"
    /// </example>
    public static readonly Parser<NodeCall> callRule =
        from keywordCall in Parse.IgnoreCase("CALL")
        from _ in spaceRule.AtLeastOnce()
        // CMDファイル名 or ジャンプ先ラベル名
        from name in quotedValue.XOr(literalValue)
        // 引数
        from parameters in (
            from _ in spaceRule.AtLeastOnce().Optional()
            from param in quotedValue.XOr(literalValue)
            select param
        ).XMany().Optional()
        select new NodeCall(name, parameters.GetOrDefault());

    /// <summary>
    /// 変数への値代入の構文
    /// </summary>
    /// <example>
    ///   SET COUNT=123
    /// </example>
    public static readonly Parser<NodeSetVariable> setVariableRule =
        from set in Parse.IgnoreCase("SET")
        from _ in spaceRule.AtLeastOnce()
        from name in identifierRule
        from eq in Parse.Char('=')
        from value in Parse.CharExcept("\r\n").Many().Text()
        select new NodeSetVariable(name, value);

    /// <summary>
    /// IFの数値比較の構文
    /// </summary>
    /// <example>
    ///   IF %VAR1% EQU 1
    /// </example>
    public static readonly Parser<ICondition> comparisonNumberRule =
        from left in Parse.CharExcept(char.IsWhiteSpace, "leftLiteral").XMany().Text().Token()
        from ope in
            Parse.IgnoreCase("EQU")
            .Or(Parse.IgnoreCase("NEQ"))
            .Or(Parse.IgnoreCase("GTR"))
            .Or(Parse.IgnoreCase("LSS"))
            .Or(Parse.IgnoreCase("LEQ"))
            .Or(Parse.IgnoreCase("GEQ"))
            .Text()
        from right in Parse.CharExcept(char.IsWhiteSpace, "rightLiteral").XMany().Text().Token()
        select new NodeComparison(left, ope, right);

    /// <summary>
    /// IFの文字列比較の構文
    /// </summary>
    /// <example>
    ///   IF %VAR1% == 1
    /// </example>
    public static readonly Parser<ICondition> comparisonStringRule =
        from left in Parse.Regex(@"([^=]+|=(?=[^=]))+").Text().Named("leftLiteral")
        from ope in Parse.String("==").Text()
        from right in Parse.CharExcept(char.IsWhiteSpace, "rightLiteral").XMany().Text().Token()
        select new NodeComparison(left.Trim(), ope, right.Trim());

    /// <summary>
    /// IF EXISTの構文
    /// </summary>
    /// <example>
    ///   IF EXIST C:\path\to\file.txt
    /// </example>
    public static readonly Parser<ICondition> existsRule =
        from keywordExists in Parse.IgnoreCase("EXIST").Token()
        from path in quotedValue.XOr(literalValue)
        select new NodeExists(path);

    /// <summary>
    /// IFの条件文の構文
    /// </summary>
    public static readonly Parser<ICondition> conditionRule =
        existsRule
        .XOr(comparisonNumberRule)
        .Or(comparisonStringRule);

    /// <summary>
    /// IFの条件文の否定時の構文
    /// </summary>
    /// <example>
    ///   IF NOT EXIST 〜
    /// </example>
    public static readonly Parser<ICondition> notConditionRule =
        from not in Parse.IgnoreCase("NOT")
        from _ in spaceRule.AtLeastOnce()
        from cond in conditionRule
        select new NodeNegatedCondition(cond);

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
                from statements in Parse.Ref(() => statementRule.Many())
                from rparen in Parse.Char(')')
                select statements)
        )
        // 条件がFalseのときのブロック（任意）
        from whenFalseStatements in (
            from keywordElse in Parse.IgnoreCase("ELSE").Token()
            from whenFalseStatements in (
                Parse.Ref(() => statementRule).Once()
                .Or(from lparen in Parse.Char('(')
                    from statements in Parse.Ref(() => statementRule.Many())
                    from rparen in Parse.Char(')')
                    select statements)
            )
            select whenFalseStatements
        ).Optional()
        select new NodeIfStatement(cond, whenTrueStatements, whenFalseStatements.GetOrDefault());

    /// <summary>
    /// FORの構文
    /// </summary>
    /// <example>
    ///   FOR /F "delims= " %%i IN ('DATE /T') DO SET YMD=%%i
    /// </example>
    public static readonly Parser<NodeForFile> forFileRule = 
        from keywordFor in Parse.IgnoreCase("FOR")
        from _whitespace1 in spaceRule.AtLeastOnce()
        from mode in Parse.IgnoreCase("/F")
        from _whitespace2 in spaceRule.AtLeastOnce()
        from option in (
            from option in quotedValue
            from _ in spaceRule.AtLeastOnce()
            select option
        ).Optional()
        from parameter in Parse.Char('%').Repeat(2).Then(_ => Parse.Letter.Once()).Text()
        from _whitespace4 in spaceRule.AtLeastOnce()
        from keywordIn in Parse.IgnoreCase("IN")
        from _whitespace5 in spaceRule.Many()
        from lparen in Parse.Char('(')
        from set in Parse.CharExcept(')').XMany().Text()
        from rparen in Parse.Char(')')
        from _whitespace6 in spaceRule.Many()
        from keywordDo in Parse.IgnoreCase("DO")
        from _whitespace7 in spaceRule.AtLeastOnce()
        from statements in (
            Parse.Ref(() => statementRule).Once()
            .Or(from lparen in Parse.Char('(')
                from statements in Parse.Ref(() => statementRule.Many())
                from rparen in Parse.Char(')')
                select statements)
        )
        select new NodeForFile(option.GetOrDefault(), parameter, set, statements);

    /// <summary>
    /// ステートメント（制御構文）の構文
    /// </summary>
    public static readonly Parser<IStatement> controlStatementRule =
        from atmark in Parse.Char('@').Optional()  // コマンド名の出力を非表示にするための「@」
        from statement in
            labelRule
            .Or<IStatement>(ifStatementRule)
            .Or(forFileRule)
        select statement;

    /// <summary>
    /// ステートメント（コマンドのみ）の構文
    /// </summary>
    public static readonly Parser<IStatement> execStatementRule =
        from atmark in Parse.Char('@').Optional()  // コマンド名の出力を非表示にするための「@」
        from statement in
            commentsRule
            .Or<IStatement>(setVariableRule)
            .Or(echoRule)
            .Or(gotoRule)
            .Or(callRule)
        select statement;

    /// <summary>
    /// ステートメントの構文
    /// </summary>
    public static readonly Parser<IStatement> statementRule =
        from statement in
            controlStatementRule.Token()
            // .Or(execStatementRule.Token())
            .Or(pipelineRule.Token())
            // pipelineRule
        select statement;

    /// <summary>
    /// バッチファイルの構文
    /// </summary>
    public static readonly Parser<BatchFile> BatchFile =
        from statements in statementRule.Many().End()
        select new BatchFile(statements);
}