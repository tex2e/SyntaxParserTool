
using Parser.WindowsBatch;
using Sprache;

namespace Parser.Tests;

public class UnitTestWindowsBatchParserIfStatement
{
    [Fact]
    public void Truth()
    {
        Assert.Equal("1", "1");
    }

    [Theory]
    [InlineData("\"%TEST%\" == \"1\"", "\"%TEST%\"", "\"1\"")]
    [InlineData("\"%TEST%\"==\"1\"",   "\"%TEST%\"", "\"1\"")]
    [InlineData("{=%TEST%=}=={=1=}", "{=%TEST%=}", "{=1=}")]
    public void ParseIfCompareString(string inputComparison, string expectedLeft, string expectedRight)
    {
        // if "%TEST%" == "1" set flag=true
        string input = $"""
        if {inputComparison} set flag=true
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        Assert.IsType<NodeIfStatement>(statements[0]);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal(expectedLeft, condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal(expectedRight, condition1.RightLiteral);
        // true statement
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Single(trueStatements);
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
        Assert.Single(statements);
        Assert.IsType<NodeIfStatement>(statements[0]);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("%TEST%", condition1.LeftLiteral);
        Assert.Equal(inputOperator, condition1.Ope);
        Assert.Equal("123", condition1.RightLiteral);
        // true statement
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Single(trueStatements);
        NodeSetVariable statement1_1 = (NodeSetVariable)trueStatements[0];
        Assert.Equal("flag", statement1_1.Name);
        Assert.Equal("true", statement1_1.Value);
    }

    [Fact]
    public void ParseIfNotEqual()
    {
        string input = """
        if NOT "%TEST%" == "1" set flag=true
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Single(statements);
        Assert.IsType<NodeIfStatement>(statements[0]);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        Assert.IsType<NodeNegatedCondition>(statement1.Condition);
        NodeNegatedCondition condition1 = (NodeNegatedCondition)statement1.Condition;
        NodeComparison condition1_1 = (NodeComparison)condition1.Condition;
        Assert.Equal("\"%TEST%\"", condition1_1.LeftLiteral);
        Assert.Equal("==", condition1_1.Ope);
        Assert.Equal("\"1\"", condition1_1.RightLiteral);
        // true statement
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Single(trueStatements);
        NodeSetVariable statement1_1 = (NodeSetVariable)trueStatements[0];
        Assert.Equal("flag", statement1_1.Name);
        Assert.Equal("true", statement1_1.Value);
    }

    [Fact]
    public void ParseIfExist()
    {
        string input = $"""
        IF EXIST c:\path\to\my.exe set flag=true
        if exist "d:\path\to\Program Files\my.exe" set flag=true
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(2, statements.Length);
        Assert.IsType<NodeIfStatement>(statements[0]);
        Assert.IsType<NodeIfStatement>(statements[1]);

        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeExists condition1 = (NodeExists)statement1.Condition;
        Assert.Equal(@"c:\path\to\my.exe", condition1.Path);
        // true statement
        IStatement[] statements1TrueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Single(statements1TrueStatements);
        NodeSetVariable statement1_1 = (NodeSetVariable)statements1TrueStatements[0];
        Assert.Equal("flag", statement1_1.Name);
        Assert.Equal("true", statement1_1.Value);

        NodeIfStatement statement2 = (NodeIfStatement)statements[1];
        NodeExists condition2 = (NodeExists)statement2.Condition;
        Assert.Equal(@"d:\path\to\Program Files\my.exe", condition2.Path);
        // true statement
        IStatement[] statements2TrueStatements = statement2.WhenTrueStatements.ToArray();
        Assert.Single(statements2TrueStatements);
        NodeSetVariable statement2_1 = (NodeSetVariable)statements2TrueStatements[0];
        Assert.Equal("flag", statement2_1.Name);
        Assert.Equal("true", statement2_1.Value);
    }

    [Fact]
    public void ParseIfNotExist()
    {
        string input = $"""
        IF NOT EXIST c:\path\to\my.exe set flag=true
        if not exist "d:\path\to\Program Files\my.exe" set flag=true
        """;
        BatchFile result = WindowsBatchParser.BatchFile.Parse(input);
        var statements = result.Statements.ToArray();
        Assert.Equal(2, statements.Length);
        Assert.IsType<NodeIfStatement>(statements[0]);
        Assert.IsType<NodeIfStatement>(statements[1]);

        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        Assert.IsType<NodeNegatedCondition>(statement1.Condition);
        NodeNegatedCondition condition1 = (NodeNegatedCondition)statement1.Condition;
        NodeExists condition1_1 = (NodeExists)condition1.Condition;
        Assert.Equal(@"c:\path\to\my.exe", condition1_1.Path);
        // true statement
        IStatement[] statements1TrueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Single(statements1TrueStatements);
        NodeSetVariable statement1_1 = (NodeSetVariable)statements1TrueStatements[0];
        Assert.Equal("flag", statement1_1.Name);
        Assert.Equal("true", statement1_1.Value);

        NodeIfStatement statement2 = (NodeIfStatement)statements[1];
        Assert.IsType<NodeNegatedCondition>(statement2.Condition);
        NodeNegatedCondition condition2 = (NodeNegatedCondition)statement2.Condition;
        NodeExists condition2_1 = (NodeExists)condition2.Condition;
        Assert.Equal(@"d:\path\to\Program Files\my.exe", condition2_1.Path);
        // true statement
        IStatement[] statements2TrueStatements = statement2.WhenTrueStatements.ToArray();
        Assert.Single(statements2TrueStatements);
        NodeSetVariable statement2_1 = (NodeSetVariable)statements2TrueStatements[0];
        Assert.Equal("flag", statement2_1.Name);
        Assert.Equal("true", statement2_1.Value);
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
        Assert.Single(statements);
        Assert.IsType<NodeIfStatement>(statements[0]);

        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("{%foobar%}", condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal("{123}", condition1.RightLiteral);
        // true block
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Equal(2, trueStatements.Length);
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
        Assert.Single(statements);
        Assert.IsType<NodeIfStatement>(statements[0]);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        Assert.IsType<NodeComparison>(statement1.Condition);
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("{%foobar%}", condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal("{123}", condition1.RightLiteral);
        // true block
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Equal(2, trueStatements.Length);
        NodeComment statement1_1 = (NodeComment)trueStatements[0];
        Assert.Equal("OK!", statement1_1.Text);
        NodeSetVariable statement1_2 = (NodeSetVariable)trueStatements[1];
        Assert.Equal("flag", statement1_2.Name);
        Assert.Equal("true", statement1_2.Value);
        // false block
        IStatement[] falseStatements = statement1.WhenFalseStatements.ToArray();
        Assert.Equal(2, falseStatements.Length);
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
        Assert.Single(statements);
        Assert.IsType<NodeIfStatement>(statements[0]);
        NodeIfStatement statement1 = (NodeIfStatement)statements[0];
        Assert.IsType<NodeComparison>(statement1.Condition);
        NodeComparison condition1 = (NodeComparison)statement1.Condition;
        Assert.Equal("{%foobar%}", condition1.LeftLiteral);
        Assert.Equal("==", condition1.Ope);
        Assert.Equal("{123}", condition1.RightLiteral);
        // true block
        IStatement[] trueStatements = statement1.WhenTrueStatements.ToArray();
        Assert.Equal(2, trueStatements.Length);
        NodeComment statement1_1 = (NodeComment)trueStatements[0];
        Assert.Equal("OK!", statement1_1.Text);
        NodeSetVariable statement1_2 = (NodeSetVariable)trueStatements[1];
        Assert.Equal("flag", statement1_2.Name);
        Assert.Equal("true", statement1_2.Value);
        // false block
        IStatement[] falseStatements = statement1.WhenFalseStatements.ToArray();
        Assert.Single(falseStatements);
        NodeSetVariable statement2_1 = (NodeSetVariable)falseStatements[0];
        Assert.Equal("flag", statement2_1.Name);
        Assert.Equal("false", statement2_1.Value);
    }
}
