namespace Step.Engine.Tween;

public delegate float EasingFunc(float t);

public interface ITween
{
	bool IsFinished { get; }
	void Update(float deltaTime);
}
