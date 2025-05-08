namespace Step.Engine.Audio;

public class Sound(string path) : GameObject
{
	private readonly string _soundKey = path;

	public bool AutoPlay { get; set; } = false;

	public bool Loop { get; set; } = false;

	protected override void OnStart()
	{
		base.OnStart();

		AudioManager.Ins.LoadSound(_soundKey, path);
		if (AutoPlay)
		{
			Play();
		}
	}

	public void Play()
	{
		AudioManager.Ins.PlaySound(_soundKey, Loop);
	}
}
