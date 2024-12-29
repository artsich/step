using ImGuiNET;
using OpenTK.Mathematics;

namespace Step.Main.Editor;

public static class EditOf
{
	private class PropertyHandler
	{
		public string Name { get; set; }
		public Action<object> RenderAction { get; set; }
	}

	private static readonly Dictionary<Type, List<PropertyHandler>> HandlersCache = [];

	public static void Render<T>(T target, string? label = null, bool isNested = false) where T : class
	{
		if (target == null)
		{
			ImGui.TextColored(Vector4.UnitX.ToSystem(), $"{label ?? typeof(T).Name} is null");
			return;
		}

		if (!isNested)
		{
			label = label ?? $"{target.GetType().Name} settings";
			ImGui.Text(label);
			ImGui.Separator();
		}

		if (!HandlersCache.TryGetValue(target.GetType(), out var handlers))
		{
			handlers = CreateHandlers(typeof(T));
			HandlersCache[typeof(T)] = handlers;
		}

		foreach (var handler in handlers)
		{
			handler.RenderAction(target);
		}
	}

	private static List<PropertyHandler> CreateHandlers(Type type)
	{
		var handlers = new List<PropertyHandler>();
		var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

		foreach (var property in properties)
		{
			if (!property.CanRead || !property.CanWrite)
				continue;

			if (property
				.GetCustomAttributes(typeof(EditorPropertyAttribute), true)
				.FirstOrDefault() is not EditorPropertyAttribute attribute)
			{
				Console.WriteLine($"Editor attribute is null for property: {property.Name}");
				continue;
			}

			var propertyType = property.PropertyType;

			if (propertyType == typeof(int))
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var value = (int)property.GetValue(target);
						var floatValue = (float)value;
						if (ImGui.SliderFloat(property.Name, ref floatValue, attribute.From, attribute.To))
						{
							property.SetValue(target, (int)floatValue);
						}
					}
				});
			}
			else if (propertyType == typeof(float))
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var value = (float)property.GetValue(target);
						if (ImGui.SliderFloat(property.Name, ref value, attribute.From, attribute.To, "%.2f"))
						{
							property.SetValue(target, value);
						}
					}
				});
			}
			else if (propertyType == typeof(bool))
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var value = (bool)property.GetValue(target);
						if (ImGui.Checkbox(property.Name, ref value))
						{
							property.SetValue(target, value);
						}
					}
				});
			}
			else if (propertyType == typeof(Vector2))
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var value = (Vector2)property.GetValue(target);
						var systemVector = value.ToSystem();
						if (ImGui.DragFloat2(property.Name, ref systemVector, attribute.From, attribute.To))
						{
							property.SetValue(target, systemVector.FromSystem());
						}
					}
				});
			}
			else if (propertyType == typeof(Vector4) && attribute != null)
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var value = (Vector4)property.GetValue(target);
						var systemVector = value.ToSystem();
						if (attribute.IsColor)
						{
							if (ImGui.ColorEdit4(property.Name, ref systemVector))
							{
								property.SetValue(target, systemVector.FromSystem());
							}
						}
						else
						{
							if (ImGui.DragFloat4(property.Name, ref systemVector, attribute.From, attribute.To))
							{
								property.SetValue(target, systemVector.FromSystem());
							}
						}
					}
				});
			}
			else if (!propertyType.IsPrimitive && !propertyType.IsEnum && propertyType.IsClass)
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var nestedValue = property.GetValue(target);
						if (nestedValue != null && !HandlersCache.ContainsKey(propertyType))
						{
							HandlersCache[propertyType] = CreateHandlers(propertyType);
						}

						if (ImGui.TreeNodeEx(property.Name, ImGuiTreeNodeFlags.Framed))
						{
							Render(nestedValue, property.Name, true);
							ImGui.TreePop();
						}
					}
				});
			}
		}

		return handlers;
	}

}
