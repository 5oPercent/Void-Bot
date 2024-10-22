using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Void.Bot.Message.Dto;

namespace Void.Bot.Api.ActionFilters;

public class UnifyResponseFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not VoidBotResponse)
        {
            context.Result = new JsonResult(new VoidBotResponse<object>
            {
                Data = context.Result is ObjectResult objectResult ? objectResult.Value : context.Result,
                Code = HttpStatusCode.OK,
                Message = nameof(HttpStatusCode.OK).ToLower()
            });
        }
        
        base.OnActionExecuted(context);
    }
}

public class UnifyResponseExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = new JsonResult(new VoidBotResponse<object>
        {
            Data = context.Result,
            Code = HttpStatusCode.InternalServerError,
            Message = context.Exception.Message
        });
        
        context.ExceptionHandled = true;
    }
}