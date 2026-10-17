using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [field: SerializeField, Min(1)]
    public int MaxHealth { get; private set; } = 100;

    public int CurrentHealth { get; private set; }

    public bool IsDead => CurrentHealth == 0;
    public bool IsAlive => CurrentHealth > 0;

    void Start() => CurrentHealth = MaxHealth;

    public UnityEvent OnDeath;

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if (CurrentHealth == 0) OnDeath?.Invoke();
    }

    public void ReceiveHealth(int health)
    {
        if (health <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + health, MaxHealth);
    }
}
