using System.Net;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Common.Exceptions;

namespace Orders.WebAPI.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (RequestValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var problem = new ValidationProblemDetails(ex.Errors)
            {
                Title = ex.Message,
                Status = context.Response.StatusCode
            };
            
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}