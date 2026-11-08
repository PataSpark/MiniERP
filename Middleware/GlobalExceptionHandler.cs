using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MiniERP.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "處理請求時發生未預期錯誤：{Path}",
            httpContext.Request.Path);

        int statusCode = StatusCodes.Status500InternalServerError;
        string message = "伺服器發生錯誤，請稍後再試。";

        if (exception is DbUpdateException dbException)
        {
            var sqlException = dbException.InnerException
                as SqlException;

            if (sqlException != null)
            {
                switch (sqlException.Number)
                {
                    case 2601:
                    case 2627:
                        statusCode = StatusCodes.Status409Conflict;
                        message = "資料已存在，請檢查是否有重複的欄位值。";
                        break;

                    case 547:
                        statusCode = StatusCodes.Status409Conflict;
                        message = "資料仍有關聯紀錄，或指定的關聯資料不存在。";
                        break;
                }
            }
        }

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == 409
                ? "資料衝突"
                : "伺服器錯誤",
            Detail = message,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}