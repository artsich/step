using Serilog;
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

var windowOptions = WindowOptions.Default;

windowOptions.Title = "Frame";
windowOptions.VSync = false;
windowOptions.FramesPerSecond = TargetFps;
windowOptions.UpdatesPerSecond = TargetFps;
windowOptions.WindowState = WindowState.Maximized;
windowOptions.WindowBorder = WindowBorder.Hidden;
windowOptions.API = new GraphicsAPI(
	ContextAPI.OpenGL,
	ContextProfile.Core,
	ContextFlags.ForwardCompatible | ContextFlags.Debug,
	new APIVersion(4, 6));
//windowOptions.PreferredDepthBufferBits = 8;

new Engine(windowOptions)
	.Run(new GameCompose());
