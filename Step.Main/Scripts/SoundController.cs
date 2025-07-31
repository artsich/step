using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Editor;

namespace Step.Main.Scripts;

public class SoundController : ScriptComponent
{
	[Export]
	public string SoundPath { get; set; } = "Music/dash.wav";
	
	private Sound? _sound;

	public override void OnStart()
	{
		_sound = new Sound(SoundPath);
		AddChild(_sound);
	}

	public override void Update(float deltaTime)
	{
		if (GameRoot.I.Input.IsKeyPressed(Key.Space))
		{
			_sound?.Play();
		}
	}
} 