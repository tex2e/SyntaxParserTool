
using System.Text;

namespace SyntaxParserTool.WindowsBatch;

public class BatchFile(IEnumerable<Statement>? statements)
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (statements is not null)
        {
            foreach (var statement in statements)
            {
                sb.AppendLine(statement.ToString());
            }
        }
        return sb.ToString();
    }
}

public interface Statement
{
    public abstract string ToString();

}

public class Comment(string Text) : Statement
{
    public override string ToString()
    {
        return $"<rem text={{{Text}}}>";
    }
}

public class Label(string Text) : Statement
{
    public override string ToString()
    {
        return $"<label text={{{Text}}}>";
    }
}

public class SetVariable(string Name, string Value) : Statement
{
    public override string ToString()
    {
        return $"<setvariable name={{{Name}}} value={{{Value}}}";
    }
}
