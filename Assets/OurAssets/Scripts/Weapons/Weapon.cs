using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [field: Header("Weapon")]
    [field: SerializeField]
    public bool IsRapidFire { get; private set; }
    [field: SerializeField]
    public LayerMask HittableLayers { get; private set; }
    [field: SerializeField, Min(0.01f)]
    public float TimeBetweenAttacks { get; private set; }
    [SerializeField, Min(1)]
    int m_BaseDamage;
    [SerializeField, Min(0.001f)]
    float m_DamagePercentIncreasePerLevel = 0.1f; // 10%

    public int Damage =>
        Mathf.RoundToInt(
            m_BaseDamage
            * (1f + m_DamagePercentIncreasePerLevel * UpgradeSystem.Instance.GetUpgradeLevel(UpgradeType.Damage))
        );

    protected float m_AttackCooldownRemaining;

    public bool CanAttack => m_AttackCooldownRemaining <= 0f;

    public abstract bool Attack(Transform origin);

    public abstract void UpdateWeapon(float deltaTime);
}
