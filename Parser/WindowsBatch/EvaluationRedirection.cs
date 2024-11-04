
namespace Parser.WindowsBatch;

class NodeRedirection(IStatement statement, string redirectMode, string filename) : IStatement
{
    public override string ToString()
    {
        return $"<redirection statement={{{statement}}} redirectMode={{{redirectMode}}} filename={{{filename}}}>";
    }
}
