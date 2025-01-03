using ImGuiNET;
using OpenTK.Mathematics;
using Step.Engine.Editor;

namespace Step.Engine;

public class GameObject(string name = nameof(GameObject))
{
	public string Name { get; init; } = name;

	public Transform LocalTransform = new();

	protected GameObject? _parent;
	protected List<GameObject> children = [];

	private readonly Queue<Action> _deferredActions = [];

	public void Defer(Action action)
	{
		_deferredActions.Enqueue(action);
	}

	public void AddChild(GameObject child)
	{
		child._parent?.RemoveChild(child);

		child._parent = this;
		children.Add(child);
	}

	public void RemoveChild(GameObject child)
	{
		if (!children.Contains(child))
		{
			return;
		}
		children.Remove(child);
		child._parent = null;
	}

	public Matrix4 GetGlobalMatrix()
	{
		Matrix4 localMat = LocalTransform.GetLocalMatrix();

		if (_parent == null)
		{
			return localMat;
		}
		else
		{
			return localMat * _parent.GetGlobalMatrix();
		}
	}

	public void Update(float deltaTime)
	{
		OnUpdate(deltaTime);
		foreach (var child in children)
		{
			child.Update(deltaTime);
		}

		ProcessDeferred();
	}

	public void Draw()
	{
		OnRender();
		foreach (var child in children)
		{
			child.Draw();
		}
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
			ImGui.SeparatorText("Children");
		}

		foreach (var child in children)
		{
			if (ImGui.TreeNodeEx(child.Name, ImGuiTreeNodeFlags.DefaultOpen))
			{
				child.DebugDraw();
				ImGui.TreePop();
			}
		}
	}

	public T GetChildOf<T>() where T : GameObject
	{
		var result = children.OfType<T>().FirstOrDefault() ?? throw new ArgumentException($"{typeof(T).Name} not found...");
		return result;
	}

	public T GetChildOf<T>(string name) where T : GameObject
	{
		var result = children.OfType<T>().FirstOrDefault(x => x.Name == name) ?? throw new ArgumentException($"{typeof(T).Name} not found...");
		return result;
	}

	public IEnumerable<T> GetChildsOf<T>() where T : GameObject
	{
		return children.OfType<T>();
	}

	public void Start()
	{
		OnStart();
		foreach (var child in children)
		{
			child.Start();
		}
	}

	public void End()
	{
		OnEnd();
		foreach (var child in children)
		{
			child.End();
		}
	}

	protected virtual void OnDebugDraw() { }

	protected virtual void OnStart() { }

	protected virtual void OnEnd() { }

	protected virtual void OnUpdate(float deltaTime) { }

	protected virtual void OnRender() { }

	private void ProcessDeferred()
	{
		while (_deferredActions.TryDequeue(out var action))
		{
			action();
		}
	}
}
