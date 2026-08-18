namespace AiRag.Api.Services
{
    public class PrecomputedEmbeddingMissingException : System.Exception
    {
        public PrecomputedEmbeddingMissingException() { }
        public PrecomputedEmbeddingMissingException(string message) : base(message) { }
        public PrecomputedEmbeddingMissingException(string message, System.Exception inner) : base(message, inner) { }
    }
}