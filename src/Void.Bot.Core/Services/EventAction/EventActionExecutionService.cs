using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.EventAction;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public interface IEventActionExecutionService : IScope
{
    Task<GetEventActionExecutionPageResponse> GetExecutionPageAsync(
        GetEventActionExecutionPageRequest request,
        CancellationToken cancellationToken);
}

public class EventActionExecutionService(
    IRepository<EventActionExecution> executionRepository) : IEventActionExecutionService
{
    public async Task<GetEventActionExecutionPageResponse> GetExecutionPageAsync(
        GetEventActionExecutionPageRequest request,
        CancellationToken cancellationToken)
    {
        var query = executionRepository.QueryNoTracking()
            .WhereIF(!string.IsNullOrWhiteSpace(request.Event), x => x.Event == request.Event)
            .WhereIF(request.RuleId.HasValue, x => x.RuleId == request.RuleId)
            .WhereIF(!string.IsNullOrWhiteSpace(request.Status), x => x.Status == request.Status)
            .WhereIF(request.StartDate.HasValue, x => x.StartedAt >= request.StartDate)
            .WhereIF(request.EndDate.HasValue, x => x.StartedAt <= request.EndDate)
            .OrderByDescending(x => x.StartedAt)
            .Select(x => new EventActionExecutionDto
            {
                Id = x.Id,
                EventId = x.EventId,
                Event = x.Event,
                EventOccurredAt = x.EventOccurredAt,
                RuleId = x.RuleId,
                RuleName = x.RuleName,
                ActionIndex = x.ActionIndex,
                ActionType = x.ActionType,
                ActionVersion = x.ActionVersion,
                FailurePolicy = x.FailurePolicy,
                Status = x.Status,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                DurationMilliseconds = x.DurationMilliseconds,
                ErrorMessage = x.ErrorMessage
            });
        var paged = await query.ToPagedAsync(
            request.PageIndex, request.PageSize, cancellationToken).ConfigureAwait(false);

        return new GetEventActionExecutionPageResponse
        {
            Data = new PagedListDto<EventActionExecutionDto>
            {
                PageIndex = paged.PageIndex,
                PageSize = paged.PageSize,
                Total = paged.Total,
                TotalPages = paged.TotalPages,
                Items = paged.Items,
                HasPrevPage = paged.HasPrevPage,
                HasNextPage = paged.HasNextPage
            }
        };
    }
}