using System.Threading.Tasks;

namespace AiRag.Api.Adapters;

public interface ILLMAdapter
{
    Task<string> GenerateAsync(string prompt);
}
