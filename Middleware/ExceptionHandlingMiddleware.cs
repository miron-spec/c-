using System.Text.Json;

namespace MatchApp.Backend.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try { await next(context); }
        catch (KeyNotFoundException ex) { await Write(context, 404, ex.Message); }
        catch (InvalidOperationException ex) { await Write(context, 400, ex.Message); }
        catch (UnauthorizedAccessException ex) { await Write(context, 401, ex.Message); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await Write(context, 500, "Internal server error.");
        }
    }

    private static async Task Write(HttpContext context, int status, string message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }
}
