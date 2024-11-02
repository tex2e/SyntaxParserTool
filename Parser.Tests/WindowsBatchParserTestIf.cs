using SyntaxParserTool.Parser.WindowsBatch;
using Sprache;

namespace Parser.Tests;

public class UnitTestWindowsBatchParserIfStatement
{
    [Fact]
    public void Truth()
    {
        Assert.Equal("1", "1");
    }

    [Fact]
    public void ParseIfCompareString()
    {
        string input = """
        if "%TEST%" == "1" set flag=true
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.True(statements.Length >= 1);
        Assert.True(statements[0] is NodeIfStatement);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("\"%TEST%\"", condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal("\"1\"", condition1.RightLiteral);
        // true statement
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.True(trueStatements.Length >= 1);
        NodeSetVariable statement1_1 = (NodeSetVariable)trueStatements[0];
        Assert.Equal("flag", statement1_1.Name);
        Assert.Equal("true", statement1_1.Value);
    }

    [Theory]
    [InlineData("equ")]
    [InlineData("EQU")]
    [InlineData("neq")]
    [InlineData("NEQ")]
    [InlineData("gtr")]
    [InlineData("GTR")]
    [InlineData("lss")]
    [InlineData("LSS")]
    [InlineData("leq")]
    [InlineData("LEQ")]
    [InlineData("geq")]
    [InlineData("GEQ")]
    public void ParseIfCompareNumber(string inputOperator)
    {
        // if %TEST% equ 123 set flag=true
        string input = $"""
        if %TEST% {inputOperator} 123 set flag=true
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.True(statements.Length >= 1);
        Assert.True(statements[0] is NodeIfStatement);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("%TEST%", condition1.LeftLiteral);
        Assert.Equal(inputOperator, condition1.Ope);
        Assert.Equal("123", condition1.RightLiteral);
        // true statement
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.True(trueStatements.Length >= 1);
        NodeSetVariable statement1_1 = (NodeSetVariable)trueStatements[0];
        Assert.Equal("flag", statement1_1.Name);
        Assert.Equal("true", statement1_1.Value);
    }

    [Fact]
    public void ParseIfBlock()
    {
        string input = """
        IF {%foobar%} == {123} (
            rem OK!
            set flag=true
        )
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.True(statements.Length >= 1);
        Assert.True(statements[0] is NodeIfStatement);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("{%foobar%}", condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal("{123}", condition1.RightLiteral);
        // true block
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.True(trueStatements.Length >= 2);
        NodeComment statement1_1 = (NodeComment)trueStatements[0];
        Assert.Equal("OK!", statement1_1.Text);
        NodeSetVariable statement1_2 = (NodeSetVariable)trueStatements[1];
        Assert.Equal("flag", statement1_2.Name);
        Assert.Equal("true", statement1_2.Value);
    }

    [Fact]
    public void ParseIfBlockElse()
    {
        string input = """
        if {%foobar%} == {123} (
            rem OK!
            set flag=true
        ) else (
            rem NG!
            set flag=false
        )
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.True(statements.Length >= 1);
        Assert.True(statements[0] is NodeIfStatement);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("{%foobar%}", condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal("{123}", condition1.RightLiteral);
        // true block
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.True(trueStatements.Length >= 2);
        NodeComment statement1_1 = (NodeComment)trueStatements[0];
        Assert.Equal("OK!", statement1_1.Text);
        NodeSetVariable statement1_2 = (NodeSetVariable)trueStatements[1];
        Assert.Equal("flag", statement1_2.Name);
        Assert.Equal("true", statement1_2.Value);
        // false block
        IStatement[] falseStatements = statement1.WhenFalseStatements.ToArray();
        Assert.True(falseStatements.Length >= 2);
        NodeComment statement2_1 = (NodeComment)falseStatements[0];
        Assert.Equal("NG!", statement2_1.Text);
        NodeSetVariable statement2_2 = (NodeSetVariable)falseStatements[1];
        Assert.Equal("flag", statement2_2.Name);
        Assert.Equal("false", statement2_2.Value);
    }

    [Fact]
    public void ParseIfBlockElseOneline()
    {
        string input = """
        if {%foobar%} == {123} (
            rem OK!
            set flag=true
        ) else set flag=false
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.True(statements.Length >= 1);
        Assert.True(statements[0] is NodeIfStatement);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("{%foobar%}", condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal("{123}", condition1.RightLiteral);
        // true block
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.True(trueStatements.Length >= 2);
        NodeComment statement1_1 = (NodeComment)trueStatements[0];
        Assert.Equal("OK!", statement1_1.Text);
        NodeSetVariable statement1_2 = (NodeSetVariable)trueStatements[1];
        Assert.Equal("flag", statement1_2.Name);
        Assert.Equal("true", statement1_2.Value);
        // false block
        IStatement[] falseStatements = statement1.WhenFalseStatements.ToArray();
        Assert.True(falseStatements.Length >= 1);
        NodeSetVariable statement2_1 = (NodeSetVariable)falseStatements[0];
        Assert.Equal("flag", statement2_1.Name);
        Assert.Equal("false", statement2_1.Value);
    }
}
