
namespace Parser.WindowsBatch;

public class NodeRedirection(IStatement statement, string redirectMode, string filename) : IStatement
{
    public override string ToString()
    {
        return $"<redirection statement={{{statement}}} redirectMode={{{redirectMode}}} filename={{{filename}}}>";
    }
}

public class NodePipeline(IStatement leftStatement, string ope, IStatement rightStatement) : IStatement
{
    public IStatement LeftStatement => leftStatement;
    public string Ope => ope;
    public IStatement RightStatement => rightStatement;

    public override string ToString()
    {
        return $"<pipeline left={{{leftStatement}}} ope={{{ope}}} filename={{{rightStatement}}}>";
    }
}
