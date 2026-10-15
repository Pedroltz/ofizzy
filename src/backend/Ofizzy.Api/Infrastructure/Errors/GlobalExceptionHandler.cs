using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ofizzy.Api.Infrastructure.Errors;

public sealed class ConflictException(string message) : Exception(message);

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            ConflictException => 409,
            FluentValidation.ValidationException => 400,
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => 409,
            Microsoft.EntityFrameworkCore.DbUpdateException
            {
                InnerException: Npgsql.PostgresException { SqlState: "23505" or "23503" or "40001" }
            } => 409,
            Npgsql.PostgresException { SqlState: "40001" } => 409,
            _ => 500
        };

        if (status == 500)
        {
            logger.LogError(
                exception,
                "Unhandled request failure for tenant {TenantId}, user {UserId}, request {RequestId}",
                context.Items["TenantId"],
                context.Items["UserId"],
                context.TraceIdentifier);
        }

        context.Response.StatusCode = status;

        var title = status switch
        {
            400 => "Requisição inválida",
            409 => "Conflito",
            _ => "Erro interno"
        };

        var detail = exception switch
        {
            ConflictException ce => ce.Message,
            FluentValidation.ValidationException ve => string.Join(" ", ve.Errors.Select(x => x.ErrorMessage)),
            _ => "Não foi possível concluir a operação. Verifique os dados e tente novamente."
        };

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            },
            Exception = exception
        });
    }
}
