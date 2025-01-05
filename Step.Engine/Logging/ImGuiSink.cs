using ImGuiNET;
using Serilog.Core;
using Serilog.Events;

namespace Step.Engine.Logging;

public sealed class ImGuiSink(IFormatProvider? formatProvider) : ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		var message = logEvent.RenderMessage(formatProvider);
		var output = $"[{logEvent.Level}] {message}";

		ImGui.DebugLog($"{output}\n");
	}
}