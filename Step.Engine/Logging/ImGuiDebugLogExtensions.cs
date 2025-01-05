using Serilog;
using Serilog.Configuration;

namespace Step.Engine.Logging;

public static class ImGuiDebugLogExtensions
{
	public static LoggerConfiguration ImGuiDebugLog(
		this LoggerSinkConfiguration loggerSinkConfiguration)
	{
		return loggerSinkConfiguration.Sink(new ImGuiSink(null));
	}
}
