
using Parser.WindowsBatch;
using Sprache;

namespace Parser.Tests;

public class UnitTestWindowsBatchParserForStatement
{
    [Fact]
    public void Truth()
    {
        Assert.Equal("1", "1");
    }

    [Fact]
    public void ParseForCommandSet()
    {
        string input = """
        FOR /F "delims= " %%i IN ('DATE /T') DO SET YMD=%%i
        FOR /F "tokens=1*" %%i IN ('MyCommand.exe') DO CALL :SUBROUTINE %%i %%j %%k %%l %%m %%n
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(2, statements.Length);

        // 1個目のFOR
        var target1 = statements[0];
        Assert.IsType<NodeForFile>(target1);
        NodeForFile nodeForFile1 = (NodeForFile)target1;
        Assert.Equal("delims= ", nodeForFile1.Option);
        Assert.Equal("i", nodeForFile1.Parameter);
        Assert.Equal("'DATE /T'", nodeForFile1.Set);
        Assert.Single(nodeForFile1.Statements);
        IStatement[] nodeForFile1Statements = nodeForFile1.Statements.ToArray();
        Assert.IsType<NodeSetVariable>(nodeForFile1Statements[0]);
        NodeSetVariable statements1_1 = (NodeSetVariable)nodeForFile1Statements[0];
        Assert.Equal("YMD", statements1_1.Name);
        Assert.Equal("%%i", statements1_1.Value);

        // 2個目のFOR
        var target2 = statements[1];
        Assert.IsType<NodeForFile>(target2);
        NodeForFile nodeForFile2 = (NodeForFile)target2;
        Assert.Equal("tokens=1*", nodeForFile2.Option);
        Assert.Equal("i", nodeForFile2.Parameter);
        Assert.Equal("'MyCommand.exe'", nodeForFile2.Set);
        Assert.Single(nodeForFile2.Statements);
        IStatement[] nodeForFile2Statements = nodeForFile2.Statements.ToArray();
        Assert.IsType<NodeCall>(nodeForFile2Statements[0]);
        NodeCall statements2_1 = (NodeCall)nodeForFile2Statements[0];
        Assert.Equal(":SUBROUTINE", statements2_1.Name);
        string[] statements2_1_params = statements2_1.Parameters.ToArray();
        Assert.Equal(6, statements2_1_params.Length);
        Assert.Equal("%%i", statements2_1_params[0]);
        Assert.Equal("%%j", statements2_1_params[1]);
        Assert.Equal("%%k", statements2_1_params[2]);
        Assert.Equal("%%l", statements2_1_params[3]);
        Assert.Equal("%%m", statements2_1_params[4]);
        Assert.Equal("%%n", statements2_1_params[5]);
    }

    [Fact]
    public void ParseForWithoutOption()
    {
        string input = """
        for /f %%p in (%FILEPATH%) do CALL :SUBROUTINE %%p
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements1 = result.Statements.ToArray();
        Assert.Single(statements1);
        var target1 = statements1[0];
        Assert.IsType<NodeForFile>(target1);
        NodeForFile nodeForFile = (NodeForFile)target1;
        Assert.Null(nodeForFile.Option);
        Assert.Equal("p", nodeForFile.Parameter);
        Assert.Equal("%FILEPATH%", nodeForFile.Set);
        Assert.Single(nodeForFile.Statements);
        IStatement[] statementsFor1 = nodeForFile.Statements.ToArray();
        Assert.IsType<NodeCall>(statementsFor1[0]);
        NodeCall statements1_1 = (NodeCall)statementsFor1[0];
        Assert.Equal(":SUBROUTINE", statements1_1.Name);
        string[] statements1_1_params = statements1_1.Parameters.ToArray();
        Assert.Single(statements1_1_params);
        Assert.Equal("%%p", statements1_1_params[0]);
    }

    [Fact]
    public void ParseForWithMultipleStatements()
    {
        string input = """
        for /f %%p in (%FILEPATH%) do (
            set flag=true
            CALL :SUBROUTINE %%p
        )
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements1 = result.Statements.ToArray();
        Assert.Single(statements1);
        var target1 = statements1[0];
        Assert.IsType<NodeForFile>(target1);
        NodeForFile nodeForFile = (NodeForFile)target1;
        Assert.Null(nodeForFile.Option);
        Assert.Equal("p", nodeForFile.Parameter);
        Assert.Equal("%FILEPATH%", nodeForFile.Set);
        IStatement[] statementsFor1 = nodeForFile.Statements.ToArray();
        Assert.Equal(2, statementsFor1.Length);

        var target1_1 = statementsFor1[0];
        Assert.IsType<NodeSetVariable>(target1_1);
        NodeSetVariable statements1_1 = (NodeSetVariable)target1_1;
        Assert.Equal("flag", statements1_1.Name);
        Assert.Equal("true", statements1_1.Value);

        var target1_2 = statementsFor1[1];
        Assert.IsType<NodeCall>(target1_2);
        NodeCall statements1_2 = (NodeCall)target1_2;
        Assert.Equal(":SUBROUTINE", statements1_2.Name);
        string[] statements1_2_params = statements1_2.Parameters.ToArray();
        Assert.Single(statements1_2_params);
        Assert.Equal("%%p", statements1_2_params[0]);
    }

    [Fact]
    public void ParseNestedFor()
    {
        string input = """
        FOR /F "delims= " %%i IN ("1 2 3") DO FOR /F "delims= " %%i IN ("4 5 6") DO echo %%i %%j
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements1 = result.Statements.ToArray();
        Assert.Single(statements1);

        var target1 = statements1[0];
        Assert.IsType<NodeForFile>(target1);
        NodeForFile nodeForFile1 = (NodeForFile)target1;
        Assert.Equal("\"1 2 3\"", nodeForFile1.Set);

        var statements1_1 = nodeForFile1.Statements.ToArray();
        Assert.Single(statements1_1);
        var target1_1 = statements1_1[0];
        Assert.IsType<NodeForFile>(target1_1);
        NodeForFile nodeForFile1_1 = (NodeForFile)target1_1;
        Assert.Equal("\"4 5 6\"", nodeForFile1_1.Set);

        var statements1_1_1 = nodeForFile1_1.Statements.ToArray();
        Assert.Single(statements1_1_1);
        var target1_1_1 = statements1_1_1[0];
        Assert.IsType<NodeEcho>(target1_1_1);
        NodeEcho nodeEcho = (NodeEcho)target1_1_1;
        Assert.Equal("%%i %%j", nodeEcho.Message);
    }

    [Fact]
    public void ParseNestedForWithMultipleStatements()
    {
        string input = """
        FOR /F "delims= " %%i IN ("1 2 3") DO (
            FOR /F "delims= " %%i IN ("4 5 6") DO (
                echo %%i %%j
            )
        )
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements1 = result.Statements.ToArray();
        Assert.Single(statements1);

        var target1 = statements1[0];
        Assert.IsType<NodeForFile>(target1);
        NodeForFile nodeForFile1 = (NodeForFile)target1;
        Assert.Equal("\"1 2 3\"", nodeForFile1.Set);

        var statements1_1 = nodeForFile1.Statements.ToArray();
        Assert.Single(statements1_1);
        var target1_1 = statements1_1[0];
        Assert.IsType<NodeForFile>(target1_1);
        NodeForFile nodeForFile1_1 = (NodeForFile)target1_1;
        Assert.Equal("\"4 5 6\"", nodeForFile1_1.Set);

        var statements1_1_1 = nodeForFile1_1.Statements.ToArray();
        Assert.Single(statements1_1_1);
        var target1_1_1 = statements1_1_1[0];
        Assert.IsType<NodeEcho>(target1_1_1);
        NodeEcho nodeEcho = (NodeEcho)target1_1_1;
        Assert.Equal("%%i %%j", nodeEcho.Message);
    }

}
