
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
}
