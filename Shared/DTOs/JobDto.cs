using System.Text.Json.Serialization;
using Shared.Enums;

namespace Shared.DTOs

{
    public class JobDto
    {
        public int Id { get; set; }
        public JobType Type { get; set; }
        public OperationType Operation { get; set; }
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime Date { get; set; }
    }

    public class BaseJob
    {
        [JsonPropertyName("type")]

        public string JobType { get; set; } = "";
        public int id { get; set; }
        public string Operation { get; set; } = "";
        public int ElementId { get; set; }
        public int AssociatedId { get; set; }

    }
    public class ViewJobResponse : BaseJob
    {

    }
}