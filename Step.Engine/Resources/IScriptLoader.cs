namespace Step.Engine.Resources;

public interface IScriptLoader
{
	IScriptComponent LoadScript(string assemblyName, string className, Dictionary<string, object> properties);
	void UnloadScript(string scriptName);
	void ReloadScripts();
} 