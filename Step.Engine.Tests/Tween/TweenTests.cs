using Step.Engine.Tween;
using TweenFactory = Step.Engine.Tween.Tween;

namespace Step.Engine.Tests.Tween;

public sealed class TweenTests
{
	[Fact]
	public void FloatTween_InterpolatesAndCompletes()
	{
		float current = -1f;
		var tween = new FloatTween(0f, 10f, 1f, value => current = value);

		tween.Update(0f);
		Assert.Equal(0f, current);

		tween.Update(0.5f);
		Assert.Equal(5f, current, 3);
		Assert.False(tween.IsFinished);

		tween.Update(0.5f);
		Assert.True(tween.IsFinished);
		Assert.Equal(10f, current);
	}

	[Fact]
	public void FloatTween_ZeroDuration_CompletesImmediately()
	{
		float current = 0f;
		var tween = new FloatTween(0f, 3f, 0f, value => current = value);

		tween.Update(0f);

		Assert.True(tween.IsFinished);
		Assert.Equal(3f, current);
	}

	[Fact]
	public void IntervalTween_WaitsFullDuration()
	{
		var tween = new IntervalTween(0.2f);

		tween.Update(0.05f);
		Assert.False(tween.IsFinished);

		tween.Update(0.1f);
		Assert.False(tween.IsFinished);

		tween.Update(0.06f);
		Assert.True(tween.IsFinished);

		tween.Update(0.5f);
		Assert.True(tween.IsFinished);
	}

	[Fact]
	public void TweenSequence_RunsStepsInOrder()
	{
		var events = new List<string>();
		var sequence = TweenFactory.Sequence(
			TweenFactory.Callback(() => events.Add("start")),
			TweenFactory.Interval(0.1f),
			TweenFactory.Callback(() => events.Add("end")));

		sequence.Update(0f);
		Assert.Equal([ "start" ], events);
		Assert.False(sequence.IsFinished);

		sequence.Update(0.05f);
		Assert.Single(events);

		sequence.Update(0.05f);
		Assert.True(sequence.IsFinished);
		Assert.Equal([ "start", "end" ], events);
	}

	[Fact]
	public void TweenPlayer_StopsAndCleansTweens()
	{
		var player = new TweenPlayer();
		var looping = new RecordingTween();
		var oneShot = new RecordingTween(finishAfterFirstUpdate: true);

		player.Play(looping);
		player.Play(oneShot);

		player.Update(0.1f);
		Assert.Equal(1, looping.UpdateCount);
		Assert.Equal(1, oneShot.UpdateCount);
		Assert.True(oneShot.IsFinished);

		player.Update(0.1f);
		Assert.Equal(1, oneShot.UpdateCount);
		var beforeStopUpdates = looping.UpdateCount;

		player.Stop(looping);
		player.Update(0.1f);
		Assert.Equal(beforeStopUpdates, looping.UpdateCount);
	}

	private sealed class RecordingTween(bool finishAfterFirstUpdate = false) : ITween
	{
		public int UpdateCount { get; private set; }
		public bool IsFinished { get; private set; }

		public void Update(float deltaTime)
		{
			if (IsFinished)
			{
				return;
			}

			UpdateCount++;
			if (finishAfterFirstUpdate)
			{
				IsFinished = true;
			}
		}
	}
}

