using Silk.NET.Windowing;
using Step.Engine;
using Step.Main;

const int TargetFps = 144;

var windowOptions = WindowOptions.Default with
{
	Title = "Frame",
	VSync = false,
	FramesPerSecond = TargetFps,
	UpdatesPerSecond = TargetFps,
	WindowBorder = WindowBorder.Fixed,
	WindowState = WindowState.Maximized,
	API = new GraphicsAPI(
		ContextAPI.OpenGL,
		ContextProfile.Core,
		ContextFlags.ForwardCompatible | ContextFlags.Debug,
		new APIVersion(4, 1))
	//PreferredDepthBufferBits = 8;
};

new Engine(windowOptions)
	.Run(new GameCompose());
