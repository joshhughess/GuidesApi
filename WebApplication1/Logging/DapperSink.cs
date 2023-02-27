using Dapper;
using Serilog.Core;
using Serilog.Events;
using System.Data.SqlClient;
using System.Diagnostics;
using GuidesApi.Extensions;
using GuidesApi.Logging;

namespace Bionical.Core.Infrastructure.Logging
{
    public class DapperSink : ILogEventSink
    {
        private readonly IConfiguration _configuration;
        private readonly IFormatProvider _formatProvider;
        private readonly IHttpContextAccessor _accessor;

        public DapperSink(IConfiguration configuration, IHttpContextAccessor accessor, IFormatProvider formatProvider = null)
        {
            _configuration = configuration;
            _accessor = accessor;
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var logEntry = new LogEntry()
                {
                    Id = Guid.NewGuid().ToString(),
                    CorrelationId = Activity.Current?.Id ?? _accessor.HttpContext?.TraceIdentifier,
                    Timestamp = logEvent.Timestamp,
                    Level = $"{(int)logEvent.Level} - {logEvent.Level}",
                    Message = logEvent.RenderMessage(_formatProvider),
                    Exception = logEvent.Exception?.ToString(),
                    User = _accessor.HttpContext?.User?.GetUserName() ?? "System",
                    UserId = _accessor.HttpContext?.User?.GetUserId()
                };

                var query = @"
                    INSERT INTO Logs (Id, CorrelationId, Timestamp, Level, Message, Exception, [User], UserId)
                    VALUES (@Id, @CorrelationId, @Timestamp, @Level, @Message, @Exception, @User, @UserId)
                ";

                connection.Execute(query, logEntry);
            }
        }
    }
}
