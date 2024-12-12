using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Step.Main;

internal static class Program
{
	private static void Main(string[] args)
	{
		var nativeWindowSettings = new NativeWindowSettings()
		{
			ClientSize = new Vector2i(1920, 1080),
			Title = "Step",
			Flags = ContextFlags.Default | ContextFlags.Debug,
		};

		using var window = new Game(GameWindowSettings.Default, nativeWindowSettings);
		window.Run();
	}
}