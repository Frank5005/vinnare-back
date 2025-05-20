using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;
namespace Api.DTOs
{

    public class CreateJobRequest : BaseJob
    {
        public int AssociatedId { get; set; }
        public void Validate()
        {
            GetJobType();
            GetOperationType();

        }
        public JobType GetJobType()
        {
            if (Enum.TryParse<JobType>(JobType, true, out var parsedJob))
            {
                return parsedJob;
            }
            throw new BadRequestException("Invalid Job type.");
        }

        public OperationType GetOperationType()
        {
            if (Enum.TryParse<OperationType>(Operation, true, out var parsedOperation))
            {
                return parsedOperation;
            }
            throw new BadRequestException("Invalid Operation type.");
        }
    }

    public class CreateJobResponse : BaseJob
    {

    }

    public class ReviewJobRequest
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public void Validate()
        {
            GetActionType();

        }


        public ActionType GetActionType()
        {
            if (Enum.TryParse<ActionType>(Action, true, out var parsedAction))
            {
                return parsedAction;
            }
            throw new BadRequestException("Invalid Action type.");
        }

    }

}
