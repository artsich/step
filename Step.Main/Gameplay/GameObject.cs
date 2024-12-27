using OpenTK.Mathematics;

namespace Step.Main.Gameplay;

public class GameObject(string name = "GameObject")
{
	public string Name = name;
	public Transform localTransform = new();

	protected GameObject? _parent;
	protected List<GameObject> children = [];

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
		Matrix4 localMat = localTransform.GetLocalMatrix();

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
	}

	public void Draw()
	{
		OnRender();
		foreach (var child in children)
		{
			child.Draw();
		}
	}

	public T GetChildOf<T>() where T : GameObject
	{
		var result = children.OfType<T>().FirstOrDefault() ?? throw new ArgumentException($"{typeof(T).Name} not found...");
		return result;
	}

	public virtual void OnStart() { }

	public virtual void OnEnd() { }

	public virtual void DebugRender() { }

	protected virtual void OnUpdate(float deltaTime) { }

	protected virtual void OnRender() { }
}
