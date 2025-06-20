using ImGuiNET;
using Serilog;
using Step.Engine.Editor;
using System.Diagnostics;

namespace Step.Engine;

public class GameObject(string name = nameof(GameObject))
{
	public string Name { get; init; } = name;

	[EditorProperty]
	public bool Enabled { get; set; } = true;

	public Transform LocalTransform = new();

	public GameObject? Parent => _parent;

	protected GameObject? _parent;
	protected List<GameObject> children = [];

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

		OnStart();
	}

	public void End()
	{
		foreach (var child in children)
		{
			child.End();
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

	protected virtual void OnDebugDraw() { }

	protected virtual void OnStart() { }

	protected virtual void OnEnd() { }

	protected virtual void OnUpdate(float deltaTime) { }

	protected internal virtual void OnUpdateEnd() { }

	protected virtual void OnRender() { }

	protected internal virtual void OnRenderEnd() { }
}
