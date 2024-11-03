using SyntaxParserTool.Parser.WindowsBatch;
using Sprache;

namespace Parser.Tests;

public class UnitTestWindowsBatchParser
{
    [Fact]
    public void Truth()
    {
        Assert.Equal("1", "1");
    }

    [Fact]
    public void ParseComment()
    {
        string input = """
        rem Test Comment
        REM コメントテスト
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        Assert.IsType<NodeComment>(statements[0]);
        Assert.Equal("Test Comment\nコメントテスト", ((NodeComment)statements[0]).Text);
    }

    [Fact]
    public void ParseSetVariable()
    {
        string input = """
        set i=1
        SET sample_variable123=456
        SET datetime=%YYYY%/%MM%/%DD% %HH%:%MM%:%SS%
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(3, statements.Length);
        Assert.IsType<NodeSetVariable>(statements[0]);
        Assert.IsType<NodeSetVariable>(statements[1]);
        Assert.IsType<NodeSetVariable>(statements[2]);
        NodeSetVariable statement1 = (NodeSetVariable)statements[0];
        Assert.Equal("i", statement1.Name);
        Assert.Equal("1", statement1.Value);
        NodeSetVariable statement2 = (NodeSetVariable)statements[1];
        Assert.Equal("sample_variable123", statement2.Name);
        Assert.Equal("456", statement2.Value);
        NodeSetVariable statement3 = (NodeSetVariable)statements[2];
        Assert.Equal("datetime", statement3.Name);
        Assert.Equal("%YYYY%/%MM%/%DD% %HH%:%MM%:%SS%", statement3.Value);
    }

    [Fact]
    public void ParseLabel()
    {
        string input = """
        :Label_test123
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        Assert.IsType<NodeLabel>(statements[0]);
        NodeLabel statement1 = (NodeLabel)statements[0];
        Assert.Equal("Label_test123", statement1.Name);
    }

    [Fact]
    public void ParseAtMark()
    {
        string input = """
        @rem test
        @set a=1
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(2, statements.Length);
        Assert.IsType<NodeComment>(statements[0]);
        Assert.IsType<NodeSetVariable>(statements[1]);
        NodeComment statement1 = (NodeComment)statements[0];
        Assert.Equal("test", statement1.Text);
        NodeSetVariable statement2 = (NodeSetVariable)statements[1];
        Assert.Equal("a", statement2.Name);
        Assert.Equal("1", statement2.Value);
    }

    [Fact]
    public void ParseGoto()
    {
        string input = """
        goto LABEL1_EXIT
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        Assert.IsType<NodeGoto>(statements[0]);
        NodeGoto statement1 = (NodeGoto)statements[0];
        Assert.Equal("LABEL1_EXIT", statement1.Name);
    }

    [Fact]
    public void ParseCallFile()
    {
        string input = """
        call %BIN_PATH%\MyProcess.cmd
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        Assert.IsType<NodeCallFile>(statements[0]);
        NodeCallFile statement1 = (NodeCallFile)statements[0];
        Assert.Equal(@"%BIN_PATH%\MyProcess.cmd", statement1.Path);
        Assert.Empty(statement1.Parameters.ToArray());
    }

    [Fact]
    public void ParseCallFileWithParams()
    {
        string input = """
        call %BIN_PATH%\MyProcess1.cmd %_MyVariable% 1234
        call %BIN_PATH%\MyProcess2.cmd "%YYYY%/%MM%/%DD% %hh%:%mm%:%ss%" 1234 "test text"
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(2, statements.Length);
        Assert.True(statements[0] is NodeCallFile);
        Assert.True(statements[1] is NodeCallFile);

        NodeCallFile statement1 = (NodeCallFile)statements[0];
        Assert.Equal(@"%BIN_PATH%\MyProcess1.cmd", statement1.Path);
        var statement1Parameters = statement1.Parameters.ToArray();
        Assert.Equal(2, statement1Parameters.Length);
        Assert.Equal("%_MyVariable%", statement1Parameters[0]);
        Assert.Equal("1234", statement1Parameters[1]);

        NodeCallFile statement2 = (NodeCallFile)statements[1];
        Assert.Equal(@"%BIN_PATH%\MyProcess2.cmd", statement2.Path);
        var statement2Parameters = statement2.Parameters.ToArray();
        Assert.Equal(3, statement2Parameters.Length);
        Assert.Equal("%YYYY%/%MM%/%DD% %hh%:%mm%:%ss%", statement2Parameters[0]);
        Assert.Equal("1234", statement2Parameters[1]);
        Assert.Equal("test text", statement2Parameters[2]);
    }
}
