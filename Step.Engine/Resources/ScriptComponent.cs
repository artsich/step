using System.Reflection;

namespace Step.Engine.Resources;

public abstract class ScriptComponent : IScriptComponent
{
	public string Name { get; set; } = string.Empty;
	public string AssemblyName { get; set; } = string.Empty;
	public string ClassName { get; set; } = string.Empty;
	public Dictionary<string, object> Properties { get; set; } = [];
	
	protected GameObject? GameObject { get; private set; }

	public virtual void Initialize(GameObject gameObject)
	{
		GameObject = gameObject;
		LoadProperties();
	}

	public virtual void Update(float deltaTime)
	{
		// Override in derived classes
	}

	public virtual void OnStart()
	{
		// Override in derived classes
	}

	public virtual void OnEnd()
	{
		// Override in derived classes
	}

	protected virtual void LoadProperties()
	{
		if (GameObject == null) return;

		foreach (var property in Properties)
		{
			SetProperty(property.Key, property.Value);
		}
	}

	protected virtual void SetProperty(string name, object value)
	{
		if (GameObject == null) return;

		var property = GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
		if (property != null && property.CanWrite)
		{
			try
			{
				var convertedValue = Convert.ChangeType(value, property.PropertyType);
				property.SetValue(this, convertedValue);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to set property {name}: {ex.Message}");
			}
		}
	}

	protected T? GetProperty<T>(string name, T? defaultValue = default)
	{
		if (Properties.TryGetValue(name, out var value))
		{
			try
			{
				return (T?)Convert.ChangeType(value, typeof(T));
			}
			catch
			{
				return defaultValue;
			}
		}
		return defaultValue;
	}
} 