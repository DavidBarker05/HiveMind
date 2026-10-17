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
    [field: SerializeField, Min(1)]
    public int Damage { get; private set; }

    protected float m_AttackCooldownRemaining;

    public bool CanAttack => m_AttackCooldownRemaining <= 0f;

    public abstract bool Attack(Transform origin);

    public abstract void UpdateWeapon(float deltaTime);
}
