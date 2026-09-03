using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SportPneus.Api.Infrastructure.Errors;

public sealed class ConflictException(string message) : Exception(message);
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception is ConflictException ? StatusCodes.Status409Conflict : StatusCodes.Status500InternalServerError;
        if (status == 500) logger.LogError(exception, "Unhandled request failure");
        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = status == 409 ? "Conflito" : "Erro interno",
                Detail = exception is ConflictException ce ? ce.Message : (environment.IsDevelopment() ? exception.ToString() : "Não foi possível concluir a operação.")
            },
            Exception = exception
        });
    }
}
