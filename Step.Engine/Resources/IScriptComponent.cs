namespace Step.Engine.Resources;

public interface IScriptComponent
{
	string Name { get; }
	string AssemblyName { get; }
	string ClassName { get; }
	Dictionary<string, object> Properties { get; }

	void Initialize(GameObject gameObject);
	void Update(float deltaTime);
	void OnStart();
	void OnEnd();
} 