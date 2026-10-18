using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    float m_BaseMaxHealth = 100f;
    [Header("Player")]
    [SerializeField, Min(1)]
    int m_HealthPerUpgrade = 20;

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public bool IsDead => CurrentHealth == 0;
    public bool IsAlive => CurrentHealth > 0;

    public UnityEvent OnDeath;
    public UnityEvent OnDamageTaken;

    public void InitHealth(float multiplier = 1f)
    {
        MaxHealth = m_BaseMaxHealth * multiplier;
        CurrentHealth = MaxHealth;
    }

    void Start() => CurrentHealth = MaxHealth;

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead) return;
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if (CurrentHealth == 0) OnDeath?.Invoke();
        else OnDamageTaken?.Invoke();
    }

    public void ReceiveHealth(int health)
    {
        if (health <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + health, MaxHealth);
    }

    public void OnUpgradePurchased(UpgradeType upgradeType)
    {
        if (upgradeType != UpgradeType.Health) return;
        MaxHealth += m_HealthPerUpgrade;
        CurrentHealth += m_HealthPerUpgrade;
    }
}
