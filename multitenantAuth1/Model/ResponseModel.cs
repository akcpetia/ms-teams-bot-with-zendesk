using System.Text.Json;

namespace multitenantAuth1.Model
{
    public class ResponseModel
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public dynamic Data { get; set; }
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
