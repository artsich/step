using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Runtime.InteropServices;

namespace Step.Main;

internal static class Program
{
	private static void Main(string[] args)
	{
		var nativeWindowSettings = new NativeWindowSettings()
		{
			ClientSize = new Vector2i(1920, 1080),
			Title = "Borderline",
			Vsync = VSyncMode.Adaptive,
			Flags = ContextFlags.Default | ContextFlags.Debug,
			WindowState = WindowState.Maximized,
		};

		var gameSettings = GameWindowSettings.Default;
		gameSettings.UpdateFrequency = 144;

		using var window = new Game(gameSettings, nativeWindowSettings);

		window.Run();
	}
}