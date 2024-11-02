using SyntaxParserTool.Parser.Csv;
using Sprache;

namespace Parser.Tests;

public class UnitTestCsvParser
{
    [Fact]
    public void ParsesSimpleList()
    {
        var input = "a,b";
        var r = CsvParser.Csv.Parse(input);
        Assert.Single(r);

        var l1 = r.First().ToArray();
        Assert.Equal(2, l1.Length);
        Assert.Equal("a", l1[0]);
        Assert.Equal("b", l1[1]);
    }

    [Fact]
    public void ParsesListWithEmptyEnding()
    {
        var input = "a,b,";
        var r = CsvParser.Csv.Parse(input);
        Assert.Single(r);

        var l1 = r.First().ToArray();
        Assert.Equal(3, l1.Length);
        Assert.Equal("a", l1[0]);
        Assert.Equal("b", l1[1]);
        Assert.Equal("", l1[2]);
    }

    [Fact]
    public void ParsesListWithNewlineEnding()
    {
        var input = "a,b," + Environment.NewLine;
        var r = CsvParser.Csv.Parse(input);
        Assert.Single(r);

        var l1 = r.First().ToArray();
        Assert.Equal(3, l1.Length);
        Assert.Equal("a", l1[0]);
        Assert.Equal("b", l1[1]);
        Assert.Equal("", l1[2]);
    }

    [Fact]
    public void ParsesLines()
    {
        var input = "a,b,c" + Environment.NewLine + "d,e,f";
        var r = CsvParser.Csv.Parse(input);
        Assert.Equal(2, r.Count());

        var l1 = r.First().ToArray();
        Assert.Equal(3, l1.Length);
        Assert.Equal("a", l1[0]);
        Assert.Equal("b", l1[1]);
        Assert.Equal("c", l1[2]);

        var l2 = r.Skip(1).First().ToArray();
        Assert.Equal(3, l2.Length);
        Assert.Equal("d", l2[0]);
        Assert.Equal("e", l2[1]);
        Assert.Equal("f", l2[2]);
    }

    [Fact]
    public void IgnoresTrailingNewline()
    {
        var input = "a,b,c" + Environment.NewLine + "d,e,f" + Environment.NewLine;
        var r = CsvParser.Csv.Parse(input);
        Assert.Equal(2, r.Count());
    }

    [Fact]
    public void IgnoresCommasInQuotedCells()
    {
        var input = "a,\"b,c\"";
        var r = CsvParser.Csv.Parse(input);
        Assert.Equal(2, r.First().Count());
    }

    [Fact]
    public void RecognisesDoubledQuotesAsSingleLiteral()
    {
        var input = "a,\"b\"\"c\"";
        var r = CsvParser.Csv.Parse(input);
        Assert.Equal("b\"c", r.First().ToArray()[1]);
    }

    [Fact]
    public void AllowsNewLinesInQuotedCells()
    {
        var input = "a,b,\"c" + Environment.NewLine + "d\"";
        var r = CsvParser.Csv.Parse(input);
        Assert.Single(r);
    }

    [Fact]
    public void IgnoresEmbeddedQuotesWhenNotFirstCharacter()
    {
        var input = "a\"b";
        var r = CsvParser.Csv.Parse(input);
        Assert.Equal("a\"b", r.First().First());
    }
}
