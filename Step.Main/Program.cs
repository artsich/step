﻿using Silk.NET.Windowing;
using Step.Engine;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Main.Gameplay;

const int TargetFps = 144;

var windowOptions = WindowOptions.Default with
{
	Title = "Frame",
	VSync = false,
	FramesPerSecond = TargetFps,
	UpdatesPerSecond = TargetFps,
	WindowBorder = WindowBorder.Fixed,
	WindowState = WindowState.Maximized,
	//WindowState = WindowState.Fullscreen,
	API = new GraphicsAPI(
		ContextAPI.OpenGL,
		ContextProfile.Core,
		ContextFlags.ForwardCompatible | ContextFlags.Debug,
		new APIVersion(4, 1))
	//PreferredDepthBufferBits = 8;
};

// todo:
// [BUG]
// * Gamepad is using and idle state
//		-> when player take damage, moving happens because camera shakes and position of mouse is changed, so mouse position should not depends on camera shake.

// do not create second renderer for editor
// use separate camara when in editor mode...

const float TargetAspectRatio = 16f / 9f;
const float InverseTargetAspectRatio = 1f / TargetAspectRatio;
const float GameCameraWidth = 320f;
const float GameCameraHeight = GameCameraWidth * InverseTargetAspectRatio;

//engine.AddEditor(new EffectsEditor(_crtEffect));
new Engine(windowOptions)
	.AddEditor((engine) => 
		new ParticlesEditor(
			engine.Window.FramebufferSize, 
			new Camera2d(GameCameraWidth, GameCameraHeight)))
	.Run((engine) => new GameScene(engine, GameCameraWidth, GameCameraHeight));
