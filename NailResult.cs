using System.Net;

namespace Mjolnir
{
    public class NailResult
    {
        public string NailId { get; set; }
        public string DurationMs { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
        public string CorrelationId { get; set; }
        public string ReasonPhrase { get; set; }
    }
}