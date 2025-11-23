# Developer Guide

## Scene Graph Safety
- Never call `AddChild` or `RemoveChild` on an already attached `GameObject` while you are iterating children during the same frame.
- Use `CallDeferred` (backed by the engine’s `DeferredActionQueue`) to schedule structural changes for the next safe point.
- Exception: if a `GameObject` hasn’t been attached to the scene yet (no parent), you can configure its children directly before adding it to the hierarchy.

## Recommended Pattern
```csharp
// Wrong: removing children inside the loop
foreach (var child in children)
{
	if (child.ShouldDisappear)
	{
		RemoveChild(child); // can break iteration & physics/state
	}
}

// Right: schedule structural changes
foreach (var child in children)
{
	if (child.ShouldDisappear)
	{
		var target = child;
		CallDeferred(() =>
		{
			RemoveChild(target);
		});
	}
}
```

Use the same approach for `AddChild`, `QueueFree`, or mass updates triggered mid-frame. When in doubt: if the object already has a parent and the change is triggered during `OnUpdate` / `OnRender`, schedule it via `CallDeferred`.

## Code Comments

Avoid adding comments to explain what code does. Instead, extract logic into well-named functions or methods to make the code self-documenting. Well-named methods and functions make the code more readable and maintainable than comments that can become outdated.

