using ImGuiNET;

namespace Step.Engine.Editor;

public class Selector(string label)
{
	private int _selectedIndex = -1;

	public event Action<string>? OnItemSelected;

	public void Render(string[] items)
	{
		ImGui.Text(label);
		RenderCombo(items);
	}

	private void RenderCombo(string[] items)
	{
		string preview = _selectedIndex >= 0 ? items[_selectedIndex] : "Select file...";
		if (ImGui.BeginCombo("##combo", preview))
		{
			for (int i = 0; i < items.Length; i++)
			{
				RenderSelectable(items, i);
			}
			ImGui.EndCombo();
		}
	}

	private void RenderSelectable(string[] items, int index)
	{
		bool isSelected = (_selectedIndex == index);
		if (ImGui.Selectable(items[index], isSelected))
		{
			SelectItem(items, index);
		}

		if (isSelected)
		{
			ImGui.SetItemDefaultFocus();
		}
	}

	private void SelectItem(string[] items, int index)
	{
		_selectedIndex = index;
		OnItemSelected?.Invoke(items[index]);
	}
}
