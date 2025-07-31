using YamlDotNet.Serialization;

namespace Step.Engine;

public class YamlSceneData
{
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = "GameObject"; // Дефолт
	public bool Enabled { get; set; } = true;
	public YamlTransformData? Transform { get; set; }
	public List<YamlScriptData>? Scripts { get; set; }
	public List<YamlSceneData>? Children { get; set; }
	
	// Ссылка на сцену (для type: GameObject + scene_path)
	public string? ScenePath { get; set; }
	
	// Sprite2D properties
	public int? Layer { get; set; }
	public Vector4f? Color { get; set; }
	public string? Texture { get; set; }
	public Vector2f? Pivot { get; set; }
	
	// KinematicBody2D properties
	public Vector2f? Velocity { get; set; }
	
	// UI properties
	public string? Text { get; set; }
	public Vector2f? Size { get; set; }
	public Vector4f? TextColor { get; set; }
	public TextAlignment? TextAlignment { get; set; }
	public string? FontPath { get; set; }
}

public class YamlTransformData
{
	public Vector2f Position { get; set; }
	public float Rotation { get; set; }
	public Vector2f Scale { get; set; } = new(1, 1);
}

public class YamlScriptData
{
	public string Name { get; set; } = string.Empty;
	public string Assembly { get; set; } = string.Empty;
	public string Class { get; set; } = string.Empty;
	public Dictionary<string, object>? Properties { get; set; }
} 