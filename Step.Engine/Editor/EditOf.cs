using ImGuiNET;

namespace Step.Engine.Editor;

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
			ImGui.TextColored(Vector4f.UnitX.ToSystem(), $"{label ?? typeof(T).Name} is null");
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
				.GetCustomAttributes(typeof(ExportAttribute), true)
				.FirstOrDefault() is not ExportAttribute attribute)
			{
				continue;
			}

			var propertyType = property.PropertyType;

			if (propertyType == typeof(string))
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						string value = (string)property.GetValue(target);
						if (ImGui.InputText(property.Name, ref value, 1000))
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
						bool value = (bool)property.GetValue(target);
						if (ImGui.Checkbox(property.Name, ref value))
						{
							property.SetValue(target, value);
						}
					}
				});
			}
			else if (propertyType == typeof(int))
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						int value = (int)property.GetValue(target);
						if (ImGui.DragInt(property.Name, ref value, (int)attribute.Speed,(int)attribute.From, (int)attribute.To))
						{
							property.SetValue(target, value);
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
						if (ImGui.DragFloat(property.Name, ref value, attribute.Speed, attribute.From, attribute.To, "%.2f"))
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
			else if (propertyType == typeof(Vector2f))
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var value = (Vector2f)property.GetValue(target);
						var systemVector = value.ToSystem();
						if (ImGui.DragFloat2(property.Name, ref systemVector, attribute.Speed, attribute.From, attribute.To))
						{
							property.SetValue(target, systemVector.FromSystem());
						}
					}
				});
			}
			else if (propertyType == typeof(Vector4f) && attribute != null)
			{
				handlers.Add(new PropertyHandler
				{
					Name = property.Name,
					RenderAction = target =>
					{
						var value = (Vector4f)property.GetValue(target);
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
