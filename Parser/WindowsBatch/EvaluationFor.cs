
using System.CodeDom.Compiler;

namespace SyntaxParserTool.Parser.WindowsBatch;

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
        using var output = new StringWriter();
        using var writer = new IndentedTextWriter(output);
        writer.WriteLine($"<for option={{{option}}} parameter={{{parameter}}} set={{{set}}}");
        writer.WriteLine($"statements={{");
        writer.Indent++;
        foreach (var statement in statements) {
            writer.WriteLine(statement.ToString());
        }
        writer.Indent--;
        writer.WriteLine($">");
        return output.ToString();
    }
}
