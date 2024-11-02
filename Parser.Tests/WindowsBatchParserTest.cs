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
        Assert.True(statements[0] is NodeComment);
        Assert.Equal("Test Comment\nコメントテスト", ((NodeComment)statements[0]).Text);
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
        Assert.True(statements[0] is NodeSetVariable);
        Assert.True(statements[1] is NodeSetVariable);
        NodeSetVariable statement1 = (NodeSetVariable)statements[0];
        Assert.Equal("i", statement1.Name);
        Assert.Equal("1", statement1.Value);
        NodeSetVariable statement2 = (NodeSetVariable)statements[1];
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
        Assert.True(statements[0] is NodeLabel);
        NodeLabel statement1 = (NodeLabel)statements[0];
        Assert.Equal("Label_test123", statement1.Name);
    }
}
