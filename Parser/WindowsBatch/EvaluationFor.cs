
using System.CodeDom.Compiler;

namespace Parser.WindowsBatch;

public interface IForInSet {}

public class NodeForFile(
    string option, 
    string parameter, 
    string set, 
    IEnumerable<IStatement> statements) : IStatement
{
    public string Option => option;
    public string Parameter => parameter;
    public string Set => set;
    public IEnumerable<IStatement> Statements => statements;

    public override string ToString()
    {
        using var textWriter = new StringWriter();
        using var indentWriter = new IndentedTextWriter(textWriter);
        Write(indentWriter);
        return textWriter.ToString();
    }

    public void Write(IndentedTextWriter indentWriter)
    {
        indentWriter.WriteLine($"<for option={{{option}}} parameter={{{parameter}}} set={{{set}}} statements={{");
        indentWriter.Indent++;
        foreach (var statement in statements) {
            statement.Write(indentWriter);
            indentWriter.WriteLine();
        }
        indentWriter.Indent--;
        indentWriter.Write($"}}>");
    }
}
