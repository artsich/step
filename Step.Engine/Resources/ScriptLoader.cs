using System.Reflection;

namespace Step.Engine.Resources;

public class ScriptLoader : IScriptLoader
{
	private readonly Dictionary<string, Assembly> _loadedAssemblies = [];
	private readonly Dictionary<string, Type> _scriptTypes = [];

	public IScriptComponent LoadScript(string assemblyName, string className, Dictionary<string, object> properties)
	{
		// Загружаем сборку, если еще не загружена
		if (!_loadedAssemblies.TryGetValue(assemblyName, out var assembly))
		{
			assembly = LoadAssembly(assemblyName);
			_loadedAssemblies[assemblyName] = assembly;
		}

		// Получаем тип скрипта
		var typeKey = $"{assemblyName}.{className}";
		if (!_scriptTypes.TryGetValue(typeKey, out var scriptType))
		{
			scriptType = assembly.GetType(className);
			if (scriptType == null)
			{
				throw new InvalidOperationException($"Script class {className} not found in assembly {assemblyName}");
			}
			_scriptTypes[typeKey] = scriptType;
		}

		// Создаем экземпляр скрипта
		var script = (IScriptComponent)Activator.CreateInstance(scriptType)!;
		script.AssemblyName = assemblyName;
		script.ClassName = className;
		script.Properties = properties;

		return script;
	}

	public void UnloadScript(string scriptName)
	{
		// TODO: Implement script unloading
	}

	public void ReloadScripts()
	{
		_loadedAssemblies.Clear();
		_scriptTypes.Clear();
	}

	private Assembly LoadAssembly(string assemblyName)
	{
		try
		{
			// Пытаемся загрузить из текущего домена
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembly = assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName);
			
			if (assembly != null)
			{
				return assembly;
			}

			// Если не найдена, загружаем из файла
			var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName}.dll");
			if (File.Exists(assemblyPath))
			{
				return Assembly.LoadFrom(assemblyPath);
			}

			throw new FileNotFoundException($"Assembly {assemblyName} not found");
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to load assembly {assemblyName}: {ex.Message}", ex);
		}
	}
} 