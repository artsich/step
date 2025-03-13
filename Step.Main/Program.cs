using Serilog;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Step.Engine;
using Step.Engine.Logging;
using Step.Main;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Async(wt =>
	{
		wt.Console();
		wt.ImGuiDebugLog();
	})
	.CreateLogger();

const int TargetFps = 144;

var monitorInfo = GetPrimaryMonitorInfo();

var windowOptions = WindowOptions.Default with
{
	Title = "Frame",
	VSync = false,
	FramesPerSecond = TargetFps,
	UpdatesPerSecond = TargetFps,
	WindowBorder = WindowBorder.Fixed,
	WindowState = WindowState.Maximized,
	//Size = monitorInfo.Size,
	API = new GraphicsAPI(
		ContextAPI.OpenGL,
		ContextProfile.Core,
		ContextFlags.ForwardCompatible | ContextFlags.Debug,
		new APIVersion(4, 6))
	//PreferredDepthBufferBits = 8;
};


new Engine(windowOptions)
	.Run(new GameCompose());


MonitorInfo GetPrimaryMonitorInfo()
{
	var monitor = Silk.NET.Windowing.Monitor.GetMainMonitor(null);
	Vector2i resolution = monitor.VideoMode.Resolution ?? throw new InvalidOperationException("Wtf?????");
	return new MonitorInfo(resolution);
}
readonly record struct MonitorInfo(Vector2i Size);
