using Bionical.Core.Infrastructure.Logging;
using Serilog;
using Serilog.Configuration;

namespace GuidesApi.Extensions
{
    public static class LoggerSinkExtensions
    {
        public static LoggerConfiguration DapperSink(this LoggerSinkConfiguration config, IConfiguration configuration, IHttpContextAccessor accessor, IFormatProvider formatProvider = null)
        {
            return config.Sink(new DapperSink(configuration, accessor, formatProvider));
        }
    }
}
