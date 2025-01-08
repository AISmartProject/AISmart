using System.Threading.Tasks;

namespace AISmart.Rag.Agent;

public class ProgrammerAgent
{
    private readonly RagProvider _ragProvider;
    public ProgrammerAgent(RagProvider ragProvider)
    {
        _ragProvider = ragProvider;
    }

    public Task ProcessTextAsync(string text) => _ragProvider.StoreTextAsync(text);
    public Task<string> AskAsync(string question) => _ragProvider.RetrieveAnswerAsync(question);
}

public class BackendAgent : ProgrammerAgent
{
    public BackendAgent(RagProvider ragProvider) : base(ragProvider)
    {
    }
}

public class FrontendAgent : ProgrammerAgent
{
    public FrontendAgent(RagProvider ragProvider) : base(ragProvider)
    {
    }
}