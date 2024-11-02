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
        Assert.True(statements.Length >= 1);
        Assert.True(statements[0] is Comment);
        Assert.Equal("Test Comment\nコメントテスト", ((Comment)statements[0]).Text);
    }

    [Fact]
    public void ParseSetVariable()
    {
        string input = """
        set i=1
        SET sample_variable123=456
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.True(statements.Length >= 2);
        Assert.True(statements[0] is SetVariable);
        Assert.True(statements[1] is SetVariable);
        SetVariable statement1 = (SetVariable)statements[0];
        Assert.Equal("i", statement1.Name);
        Assert.Equal("1", statement1.Value);
        SetVariable statement2 = (SetVariable)statements[1];
        Assert.Equal("sample_variable123", statement2.Name);
        Assert.Equal("456", statement2.Value);
    }

    [Fact]
    public void ParseLabel()
    {
        string input = """
        :Label_test123
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.True(statements.Length >= 1);
        Assert.True(statements[0] is Label);
        Label statement1 = (Label)statements[0];
        Assert.Equal("Label_test123", statement1.Name);
    }
}
