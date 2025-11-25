namespace Step.Main.Gameplay.TowerDefense.Core;

public sealed class Health
{
	public float MaxHealth { get; private set; }
	public float CurrentHealth { get; private set; }

	public event Action<float, float>? HealthChanged;

	public Health(float maxHealth)
	{
		if (maxHealth <= 0f || float.IsNaN(maxHealth) || float.IsInfinity(maxHealth))
			throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be a finite value greater than zero.");

		MaxHealth = maxHealth;
		CurrentHealth = maxHealth;
	}

	public void ApplyDamage(float amount)
	{
		if (amount <= 0f || CurrentHealth <= 0f)
			return;

		float previous = CurrentHealth;
		CurrentHealth = MathF.Max(0f, CurrentHealth - amount);

		if (MathF.Abs(previous - CurrentHealth) <= float.Epsilon)
			return;

		HealthChanged?.Invoke(CurrentHealth, MaxHealth);
	}

	public bool Heal(float amount)
	{
		if (amount <= 0f || CurrentHealth <= 0f)
			return false;

		float previous = CurrentHealth;
		CurrentHealth = MathF.Min(MaxHealth, CurrentHealth + amount);

		if (MathF.Abs(previous - CurrentHealth) <= float.Epsilon)
			return false;

		HealthChanged?.Invoke(CurrentHealth, MaxHealth);
		return true;
	}

	public bool IncreaseMaxHealth(float amount, bool refillToFull = true)
	{
		if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
			return false;

		MaxHealth = MathF.Max(0f, MaxHealth + amount);

		if (refillToFull)
		{
			CurrentHealth = MaxHealth;
		}
		else
		{
			CurrentHealth = MathF.Min(CurrentHealth, MaxHealth);
		}

		HealthChanged?.Invoke(CurrentHealth, MaxHealth);
		return true;
	}
}

