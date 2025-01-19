using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Serilog;
using Step.Engine.Logging;
using Step.Main;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Async(wt =>
	{
		wt.Console();
		wt.ImGuiDebugLog();
	})
	.CreateLogger();

var nativeWindowSettings = new NativeWindowSettings()
{
	Title = "Borderline",
	Vsync = VSyncMode.Adaptive,
	Flags = ContextFlags.Default | ContextFlags.Debug,
	WindowState = WindowState.Maximized,
	WindowBorder = WindowBorder.Fixed,
};

var gameSettings = GameWindowSettings.Default;

gameSettings.UpdateFrequency = 144;

using var window = new GameCompose(gameSettings, nativeWindowSettings);

window.Run();
