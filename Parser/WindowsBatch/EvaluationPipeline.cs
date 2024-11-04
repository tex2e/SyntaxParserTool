
using System.CodeDom.Compiler;

namespace Parser.WindowsBatch;

public class NodePipeline(IStatement leftStatement, string ope, IStatement rightStatement) : IStatement
{
    public IStatement LeftStatement => leftStatement;
    public string Ope => ope;
    public IStatement RightStatement => rightStatement;

    public override string ToString()
    {
        return $"<pipeline left={{{leftStatement}}} ope={{{ope}}} right={{{rightStatement}}}>";
    }

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.WriteLine("<pipeline left={{");
        indentWriter.Indent++;
        leftStatement.Write(indentWriter);
        indentWriter.WriteLine();
        indentWriter.Indent--;
        indentWriter.Write("}} ");
        indentWriter.Write($"ope={{{ope}}} ");
        indentWriter.WriteLine("right={{");
        indentWriter.Indent++;
        rightStatement.Write(indentWriter);
        indentWriter.WriteLine();
        indentWriter.Indent--;
        indentWriter.Write("}}>");
    }
}
