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
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401
            });
        }
        catch (ForbiddenException ex)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Forbidden",
                Status = 403,
                Detail = ex.Message
            });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = ex.Message
                }
            );
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Bad Request",
                Status = 400,
                Detail = ex.Message
            });
        }
    }
}