using Shared.Enums;

namespace Shared.DTOs

{
    public class JobDto
    {
        public int Id { get; set; }
        public JobType Type { get; set; }
        public OperationType Operation { get; set; }
        public ActionType Action { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime Date { get; set; }
    }
}