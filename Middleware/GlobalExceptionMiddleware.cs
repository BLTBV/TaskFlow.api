using System.Net;
using System.Text.Json;

namespace TaskFlow.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            ctx.Response.ContentType = "application/problem+json";

            var (status, title) = ex switch
            {
                ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad request"),
                InvalidOperationException => ((int)HttpStatusCode.Conflict, "Conflict"),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not found"),
                _ => ((int)HttpStatusCode.InternalServerError, "Server error")
            };

            ctx.Response.StatusCode = status;

            var problem = new
            {
                type = "about:blank",
                title,
                status,
                detail = ex.Message,
                traceId = ctx.TraceIdentifier
            };

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}