using System.Reflection;

namespace Step.Engine;

public static class GameObjectTypes
{
	private static readonly Dictionary<string, Type> _creatableTypes = [];

	static GameObjectTypes()
	{
		// Находим все типы которые наследуют GameObject и не абстрактные
		var gameObjectType = typeof(GameObject);
		var types = gameObjectType.Assembly.GetTypes()
			.Where(t => 
				gameObjectType.IsAssignableFrom(t) && 
				!t.IsAbstract) // Включаем GameObject
			.ToArray();
			
		// Регистрируем по полному имени (namespace + имя)
		foreach (var type in types)
		{
			var fullName = $"{type.Namespace}.{type.Name}";
			_creatableTypes[fullName] = type;
			_creatableTypes[type.Name] = type; // И по короткому имени для обратной совместимости
		}
	}
	
	public static GameObject CreateByType(string typeName, string name, YamlNode? yamlData = null)
	{
		if (_creatableTypes.TryGetValue(typeName, out var type))
		{
			var gameObject = (GameObject)Activator.CreateInstance(type, name)!;
			
			// Если объект поддерживает сериализацию и есть данные
			if (gameObject is ISerializable serializable && yamlData != null)
			{
				serializable.DeserializeFromYaml(yamlData);
			}
			
			return gameObject;
		}
		
		// Fallback - обычный GameObject
		return new GameObject(name);
	}
	
	public static IEnumerable<string> GetAvailableTypes()
	{
		return _creatableTypes.Keys;
	}
	
	public static bool IsValidType(string typeName)
	{
		return _creatableTypes.ContainsKey(typeName);
	}
} 