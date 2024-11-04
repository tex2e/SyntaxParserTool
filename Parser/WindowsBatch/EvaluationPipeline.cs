
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
}
