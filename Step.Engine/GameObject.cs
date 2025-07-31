using ImGuiNET;
using Serilog;
using Step.Engine.Editor;
using System.Diagnostics;

namespace Step.Engine;

public class GameObject(string name = nameof(GameObject))
{
	public string Name { get; init; } = name;

	[Export]
	public bool Enabled { get; set; } = true;

	public Transform LocalTransform = new();

	public GameObject? Parent => _parent;

	protected GameObject? _parent;
	protected List<GameObject> children = [];
	protected List<IScriptComponent> scripts = [];

	private bool _markedAsFree;

	public Vector2f LocalPosition
	{
		get => LocalTransform.Position;
		set => LocalTransform.Position = value;
	}

	public Vector2f GlobalPosition
	{
		get => GetGlobalMatrix().ExtractTranslation().Xy();
		set
		{
			if (_parent != null)
			{
				var worldInverse = _parent.GetGlobalMatrix().Inverted();
				LocalTransform.Position = (new Vector4f(value, 0f, 1f) * worldInverse).Xy();
			}
			else
			{
				LocalTransform.Position = value;
			}
		}
	}

	public bool MarkedAsFree => _markedAsFree;

	public void Start()
	{
		foreach (var child in children)
		{
			child.Start();
		}

		foreach (var script in scripts)
		{
			script.Initialize(this);
			script.OnStart();
		}

		OnStart();
	}

	public void End()
	{
		foreach (var child in children)
		{
			child.End();
		}

		foreach (var script in scripts)
		{
			script.OnEnd();
		}

		OnEnd();
	}

	public void AddChild(GameObject child)
	{
		Debug.Assert(this != child, "WTF??");
		child._parent?.RemoveChild(child);
		child._parent = this;
		children.Add(child);
	}

	public void RemoveChild(GameObject child)
	{
		Debug.Assert(this != child, "WTF??");
		if (!children.Contains(child))
		{
			return;
		}
		children.Remove(child);
		child._parent = null;
	}

	public void ClearChildren()
	{
		for (int i = 0; i < children.Count; i++)
		{
			RemoveChild(children[i]);
		}
	}

	public void Update(float deltaTime)
	{
		if (_markedAsFree || !Enabled)
		{
			return;
		}

		OnUpdate(deltaTime);
		foreach (var child in children)
		{
			child.Update(deltaTime);
		}
		foreach (var script in scripts)
		{
			script.Update(deltaTime);
		}
		OnUpdateEnd();
	}

	public void Draw()
	{
		if (!Enabled)
		{
			return;
		}

		OnRender();
		foreach (var child in children)
		{
			child.Draw();
		}
		OnRenderEnd();
	}

	public void DebugDraw()
	{
		OnDebugDraw();

		if (ImGui.CollapsingHeader("Transform"))
		{
			EditOf.Render(LocalTransform);
		}

		if (children.Count > 0)
		{
			ImGui.SeparatorText($"Children : {children.Count}");
		}

		foreach (var child in children)
		{
			if (ImGui.TreeNodeEx(child.Name))
			{
				child.DebugDraw();
				ImGui.TreePop();
			}
		}
	}

	public Matrix4f GetGlobalMatrix()
	{
		Matrix4f localMat = LocalTransform.GetLocalMatrix();

		if (_parent == null)
		{
			return localMat;
		}
		else
		{
			return localMat * _parent.GetGlobalMatrix();
		}
	}

	public void QueueFree()
	{
		if (!_markedAsFree)
		{
			_markedAsFree = true;
			CallDeferred(() =>
			{
				_parent?.RemoveChild(this);
				End();
			});
		}
		else
		{
			Log.Logger.Warning($"{nameof(QueueFree)} Already called...");
		}
	}

	public void CallDeferred(Action action)
	{
		GameRoot.I.Defer(action);
	}

	public bool Contains<T>() where T : class
	{
		return children.OfType<T>().FirstOrDefault() != null;
	}

	public T GetChildOf<T>() where T : GameObject
	{
		foreach (var child in children)
		{
			if (child is T typedChild)
				return typedChild;
		}
		throw new ArgumentException($"Child of type {typeof(T).Name} not found in {Name}");
	}

	public T GetChildOf<T>(string name) where T : GameObject
	{
		foreach (var child in children)
		{
			if (child is T typedChild && typedChild.Name == name)
				return typedChild;
		}
		throw new ArgumentException($"Child of type {typeof(T).Name} with name {name} not found in {Name}");
	}

	public IEnumerable<T> GetChildsOf<T>() where T : GameObject
	{
		return children.OfType<T>();
	}

	public void AddScript(IScriptComponent script)
	{
		scripts.Add(script);
	}

	public void RemoveScript(IScriptComponent script)
	{
		scripts.Remove(script);
	}

	public T? GetScript<T>() where T : class, IScriptComponent
	{
		return scripts.OfType<T>().FirstOrDefault();
	}

	public IEnumerable<T> GetScripts<T>() where T : class, IScriptComponent
	{
		return scripts.OfType<T>();
	}

	protected virtual void OnDebugDraw() { }

	protected virtual void OnStart() { }

	protected virtual void OnEnd() { }

	protected virtual void OnUpdate(float deltaTime) { }

	protected internal virtual void OnUpdateEnd() { }

	protected virtual void OnRender() { }

	protected internal virtual void OnRenderEnd() { }

	// Загрузка GameObject из YAML файла
	public static T Load<T>(string pathToYaml) where T : GameObject
	{
		return (T)LoadInternal(pathToYaml);
	}

	private static GameObject LoadInternal(string pathToYaml)
	{
		var fullPath = Path.Combine(Assets.AssetsFolder, pathToYaml);
		if (!File.Exists(fullPath))
			throw new FileNotFoundException($"Scene file not found: {fullPath}");

		var yamlContent = File.ReadAllText(fullPath);
		var stream = new StringReader(yamlContent);
		var yaml = new YamlDotNet.RepresentationModel.YamlStream();
		yaml.Load(stream);

		var document = yaml.Documents[0];
		var rootNode = document.RootNode;
		
		return CreateGameObjectFromYaml(rootNode);
	}

	private static GameObject CreateGameObjectFromYaml(YamlNode node)
	{
		var mapping = (YamlMappingNode)node;
		
		// Получаем имя и тип
		var name = mapping["name"].ToString();
		var type = mapping.ContainsKey("type") ? mapping["type"].ToString() : "GameObject";
		
		// Создаем GameObject нужного типа с YAML данными
		var gameObject = GameObjectTypes.CreateByType(type, name, mapping);
		
		// Устанавливаем базовые свойства
		if (mapping.ContainsKey("enabled"))
			gameObject.Enabled = bool.Parse(mapping["enabled"].ToString());
		
		// Устанавливаем трансформ
		if (mapping.ContainsKey("transform"))
		{
			var transformNode = (YamlMappingNode)mapping["transform"];
			if (transformNode.ContainsKey("position"))
			{
				var pos = transformNode["position"].ToString();
				gameObject.LocalTransform.Position = ParseVector2f(pos);
			}
			if (transformNode.ContainsKey("rotation"))
				gameObject.LocalTransform.Rotation = float.Parse(transformNode["rotation"].ToString());
			if (transformNode.ContainsKey("scale"))
			{
				var scale = transformNode["scale"].ToString();
				gameObject.LocalTransform.Scale = ParseVector2f(scale);
			}
		}

		// Обрабатываем ссылку на сцену
		if (mapping.ContainsKey("scene_path"))
		{
			var childScene = LoadInternal(mapping["scene_path"].ToString());
			childScene.Name = name;
			return childScene;
		}

		// Добавляем скрипты
		if (mapping.ContainsKey("scripts"))
		{
			var scriptsNode = (YamlSequenceNode)mapping["scripts"];
			var scriptLoader = new Resources.ScriptLoader();
			foreach (YamlMappingNode scriptNode in scriptsNode)
			{
				var script = scriptLoader.LoadScript(
					scriptNode["assembly"].ToString(),
					scriptNode["class"].ToString(),
					new Dictionary<string, object>() // TODO: Parse properties
				);
				gameObject.AddScript(script);
			}
		}

		// Добавляем дочерние объекты
		if (mapping.ContainsKey("children"))
		{
			var childrenNode = (YamlSequenceNode)mapping["children"];
			foreach (YamlMappingNode childNode in childrenNode)
			{
				var child = CreateGameObjectFromYaml(childNode);
				gameObject.AddChild(child);
			}
		}

		return gameObject;
	}
	
	private static Vector2f ParseVector2f(string str)
	{
		// Парсим "[x, y]" в Vector2f
		var clean = str.Trim('[', ']');
		var parts = clean.Split(',');
		return new Vector2f(float.Parse(parts[0]), float.Parse(parts[1]));
	}
}
