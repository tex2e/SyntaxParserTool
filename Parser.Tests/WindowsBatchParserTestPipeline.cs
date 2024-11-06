
using Parser.WindowsBatch;
using Sprache;

namespace Parser.Tests;

public class UnitTestWindowsBatchParserPipeline
{
    [Fact]
    public void Truth()
    {
        Assert.Equal(2, 1+1);
    }

    [Fact]
    public void ParsePipelineIfSucceedsThen()
    {
        string input = """
        echo 123 && echo 456
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        var target1 = statements[0];
        Assert.IsType<NodePipeline>(target1);
        NodePipeline nodePipeline = (NodePipeline)target1;
        Assert.IsType<NodeEcho>(nodePipeline.LeftStatement);
        Assert.Equal("123", ((NodeEcho)nodePipeline.LeftStatement).Message.Trim());
        Assert.Equal("&&", nodePipeline.Ope);
        Assert.IsType<NodeEcho>(nodePipeline.RightStatement);
        Assert.Equal("456", ((NodeEcho)nodePipeline.RightStatement).Message);
    }

    [Fact]
    public void ParsePipelineIfFailsThen()
    {
        string input = """
        echo 123 || echo 456
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        var target1 = statements[0];
        Assert.IsType<NodePipeline>(target1);
        NodePipeline nodePipeline = (NodePipeline)target1;
        Assert.IsType<NodeEcho>(nodePipeline.LeftStatement);
        Assert.Equal("123", ((NodeEcho)nodePipeline.LeftStatement).Message.Trim());
        Assert.Equal("||", nodePipeline.Ope);
        Assert.IsType<NodeEcho>(nodePipeline.RightStatement);
        Assert.Equal("456", ((NodeEcho)nodePipeline.RightStatement).Message);
    }

    [Fact]
    public void ParsePipelineIfSucceedsThenFailsThen()
    {
        string input = """
        echo 123 && echo 456 || echo 789
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        var target1 = statements[0];
        Assert.IsType<NodePipeline>(target1);
        NodePipeline nodePipeline1 = (NodePipeline)target1;

        Assert.IsType<NodePipeline>(nodePipeline1.LeftStatement);
        Assert.IsType<NodeEcho>(nodePipeline1.RightStatement);
        Assert.Equal("||", nodePipeline1.Ope);
        Assert.Equal("789", ((NodeEcho)nodePipeline1.RightStatement).Message.Trim());

        NodePipeline nodePipeline1_1 = (NodePipeline)nodePipeline1.LeftStatement;
        Assert.IsType<NodeEcho>(nodePipeline1_1.LeftStatement);
        Assert.Equal("123", ((NodeEcho)nodePipeline1_1.LeftStatement).Message.Trim());
        Assert.Equal("&&", nodePipeline1_1.Ope);
        Assert.IsType<NodeEcho>(nodePipeline1_1.RightStatement);
        Assert.Equal("456", ((NodeEcho)nodePipeline1_1.RightStatement).Message.Trim());
    }

    [Fact]
    public void ParsePipelineOutput()
    {
        string input = """
        echo 123 | echo 456 | echo 789
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        var target1 = statements[0];
        Assert.IsType<NodePipeline>(target1);
        NodePipeline nodePipeline1 = (NodePipeline)target1;

        Assert.IsType<NodePipeline>(nodePipeline1.LeftStatement);
        Assert.IsType<NodeEcho>(nodePipeline1.RightStatement);
        Assert.Equal("|", nodePipeline1.Ope);
        Assert.Equal("789", ((NodeEcho)nodePipeline1.RightStatement).Message.Trim());

        NodePipeline nodePipeline1_1 = (NodePipeline)nodePipeline1.LeftStatement;
        Assert.IsType<NodeEcho>(nodePipeline1_1.LeftStatement);
        Assert.Equal("123", ((NodeEcho)nodePipeline1_1.LeftStatement).Message.Trim());
        Assert.Equal("|", nodePipeline1_1.Ope);
        Assert.IsType<NodeEcho>(nodePipeline1_1.RightStatement);
        Assert.Equal("456", ((NodeEcho)nodePipeline1_1.RightStatement).Message.Trim());
    }

    [Fact]
    public void ParsePipelineThenRun()
    {
        string input = """
        echo 123 & echo 456 & rem My Comment
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        var target1 = statements[0];
        Assert.IsType<NodePipeline>(target1);
        NodePipeline nodePipeline1 = (NodePipeline)target1;

        Assert.IsType<NodePipeline>(nodePipeline1.LeftStatement);
        Assert.IsType<NodeComment>(nodePipeline1.RightStatement);
        Assert.Equal("&", nodePipeline1.Ope);
        Assert.Equal("My Comment", ((NodeComment)nodePipeline1.RightStatement).Text);

        NodePipeline nodePipeline1_1 = (NodePipeline)nodePipeline1.LeftStatement;
        Assert.IsType<NodeEcho>(nodePipeline1_1.LeftStatement);
        Assert.Equal("123", ((NodeEcho)nodePipeline1_1.LeftStatement).Message.Trim());
        Assert.Equal("&", nodePipeline1_1.Ope);
        Assert.IsType<NodeEcho>(nodePipeline1_1.RightStatement);
        Assert.Equal("456", ((NodeEcho)nodePipeline1_1.RightStatement).Message.Trim());
    }

    [Fact]
    public void ParseEchoWithRedirection()
    {
        string input = """
        echo Hello, world! > output.log
        echo Test Text >> "output-%YYYYMMDD%.txt"
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(2, statements.Length);

        var target1 = statements[0];
        Assert.IsType<NodeEcho>(target1);
        NodeEcho statement1 = (NodeEcho)target1;
        Assert.Equal("Hello, world! ", statement1.Message);
        Assert.True(statement1.Redirect is not null);
        Assert.Equal(">", statement1.Redirect?.Mode);
        Assert.Equal("output.log", statement1.Redirect?.Filename);

        var target2 = statements[1];
        Assert.IsType<NodeEcho>(target2);
        NodeEcho statement2 = (NodeEcho)target2;
        Assert.Equal("Test Text ", statement2.Message);
        Assert.True(statement2.Redirect is not null);
        Assert.Equal(">>", statement2.Redirect?.Mode);
        Assert.Equal("output-%YYYYMMDD%.txt", statement2.Redirect?.Filename);
    }

    [Fact]
    public void ParseEchoWithRedirectionHandles()
    {
        string input = """
        echo Hello, world! 1> output.log
        echo Test Text 2>> "output-%YYYYMMDD%.txt"
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(2, statements.Length);

        var target1 = statements[0];
        Assert.IsType<NodeEcho>(target1);
        NodeEcho statement1 = (NodeEcho)target1;
        Assert.Equal("Hello, world! ", statement1.Message);
        Assert.True(statement1.Redirect is not null);
        Assert.Equal("1>", statement1.Redirect?.Mode);
        Assert.Equal("output.log", statement1.Redirect?.Filename);

        var target2 = statements[1];
        Assert.IsType<NodeEcho>(target2);
        NodeEcho statement2 = (NodeEcho)target2;
        Assert.Equal("Test Text ", statement2.Message);
        Assert.True(statement2.Redirect is not null);
        Assert.Equal("2>>", statement2.Redirect?.Mode);
        Assert.Equal("output-%YYYYMMDD%.txt", statement2.Redirect?.Filename);
    }
}
