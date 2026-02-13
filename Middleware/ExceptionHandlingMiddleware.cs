using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace UserManagementAPI.Middleware;

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred. Path: {Path}", context.Request.Path);

        var problemDetails = new
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "An unexpected error occurred",
            Detail = "We're sorry — something went wrong on our end.",
            Instance = context.Request.Path.ToString()
        };

        // Handle specific exceptions
        if (exception is ArgumentException or ArgumentNullException)
        {
            problemDetails = new
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Bad Request",
                Detail = "Invalid request parameters.",
                Instance = context.Request.Path.ToString()
            };
        }
        else if (exception is KeyNotFoundException)
        {
            problemDetails = new
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Not Found",
                Detail = "The requested resource was not found.",
                Instance = context.Request.Path.ToString()
            };
        }
        else if (exception is UnauthorizedAccessException)
        {
            problemDetails = new
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Unauthorized",
                Detail = "Authentication is required.",
                Instance = context.Request.Path.ToString()
            };
        }
        else if (exception is ValidationException validationEx)
        {
            problemDetails = new
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Error",
                Detail = validationEx.Message,
                Instance = context.Request.Path.ToString()
            };
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problemDetails.Status;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}