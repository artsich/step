namespace Step.Main.Editor;

public interface IEditorView
{
	string Name { get; }

	void Update(float dt);

	void Draw();
}
