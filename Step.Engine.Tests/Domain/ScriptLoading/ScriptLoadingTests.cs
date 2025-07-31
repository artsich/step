using Step.Engine;
using Step.Engine.Resources;
using Xunit;

namespace Step.Engine.Tests.Domain.ScriptLoading;

public class ScriptLoadingTests
{
	[Fact]
	public void LoadScript_ShouldCreateScriptInstance()
	{
		// Arrange
		var scriptLoader = new ScriptLoader();
		var properties = new Dictionary<string, object>
		{
			["speed"] = 5.0f,
			["jump_force"] = 10.0f,
			["health"] = 100
		};

		// Act
		var script = scriptLoader.LoadScript("Step.Main", "Step.Main.Scripts.PlayerController", properties);

		// Assert
		Assert.NotNull(script);
		Assert.Equal("Step.Main", script.AssemblyName);
		Assert.Equal("Step.Main.Scripts.PlayerController", script.ClassName);
		Assert.Equal(3, script.Properties.Count);
	}

	[Fact]
	public void GameObjectWithScript_ShouldInitializeScript()
	{
		// Arrange
		var gameObject = new GameObject("Player");
		var scriptLoader = new ScriptLoader();
		var properties = new Dictionary<string, object>
		{
			["speed"] = 5.0f,
			["jump_force"] = 10.0f,
			["health"] = 100
		};

		var script = scriptLoader.LoadScript("Step.Main", "Step.Main.Scripts.PlayerController", properties);
		gameObject.AddScript(script);

		// Act
		gameObject.Start();

		// Assert
		var playerController = gameObject.GetScript<PlayerController>();
		Assert.NotNull(playerController);
		Assert.Equal(5.0f, playerController.Speed);
		Assert.Equal(10.0f, playerController.JumpForce);
		Assert.Equal(100, playerController.Health);
	}

	[Fact]
	public void GameObjectWithMultipleScripts_ShouldInitializeAllScripts()
	{
		// Arrange
		var gameObject = new GameObject("Player");
		var scriptLoader = new ScriptLoader();

		var playerProperties = new Dictionary<string, object>
		{
			["speed"] = 5.0f,
			["jump_force"] = 10.0f,
			["health"] = 100
		};

		var healthProperties = new Dictionary<string, object>
		{
			["max_health"] = 100,
			["current_health"] = 100,
			["is_invulnerable"] = false
		};

		var playerScript = scriptLoader.LoadScript("Step.Main", "Step.Main.Scripts.PlayerController", playerProperties);
		var healthScript = scriptLoader.LoadScript("Step.Main", "Step.Main.Scripts.HealthComponent", healthProperties);

		gameObject.AddScript(playerScript);
		gameObject.AddScript(healthScript);

		// Act
		gameObject.Start();

		// Assert
		var playerController = gameObject.GetScript<PlayerController>();
		var healthComponent = gameObject.GetScript<HealthComponent>();

		Assert.NotNull(playerController);
		Assert.NotNull(healthComponent);
		Assert.Equal(100, healthComponent.MaxHealth);
		Assert.Equal(100, healthComponent.CurrentHealth);
		Assert.False(healthComponent.IsInvulnerable);
	}

	[Fact]
	public void ScriptUpdate_ShouldBeCalled()
	{
		// Arrange
		var gameObject = new GameObject("Player");
		var scriptLoader = new ScriptLoader();
		var properties = new Dictionary<string, object>
		{
			["speed"] = 5.0f,
			["jump_force"] = 10.0f,
			["health"] = 100
		};

		var script = scriptLoader.LoadScript("Step.Main", "Step.Main.Scripts.PlayerController", properties);
		gameObject.AddScript(script);
		gameObject.Start();

		// Act
		gameObject.Update(0.016f); // 60 FPS

		// Assert
		// Script should be updated without errors
		Assert.True(true); // If we reach here, no exception was thrown
	}

	[Fact]
	public void LoadInvalidScript_ShouldThrowException()
	{
		// Arrange
		var scriptLoader = new ScriptLoader();
		var properties = new Dictionary<string, object>();

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => 
			scriptLoader.LoadScript("InvalidAssembly", "InvalidClass", properties));
	}
} 