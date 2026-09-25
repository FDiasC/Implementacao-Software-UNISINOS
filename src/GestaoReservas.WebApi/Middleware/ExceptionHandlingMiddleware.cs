using GestaoReservas.WebApi.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestaoReservas.WebApi.Middleware;

/// <summary>Tratamento centralizado de erros: converte exceções em respostas HTTP JSON.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            await EscreverProblemaAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (RegraDeNegocioException ex)
        {
            await EscreverProblemaAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client encerrou a requisição = não há resposta a enviar.
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await EscreverProblemaAsync(context, StatusCodes.Status409Conflict, "Já existe um registro com o mesmo valor em um campo que deve ser único.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            await EscreverProblemaAsync(context, StatusCodes.Status409Conflict, "A operação viola um vínculo com outro registro.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
            await EscreverProblemaAsync(context, StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado ao processar a requisição.");
        }
    }

    private static async Task EscreverProblemaAsync(HttpContext context, int statusCode, string mensagem)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = mensagem,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
