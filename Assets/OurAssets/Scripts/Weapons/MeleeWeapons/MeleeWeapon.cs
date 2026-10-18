using UnityEngine;

public abstract class MeleeWeapon : Weapon
{
    [field: Header("Melee Weapon")]
    [field: SerializeField, Min(0.01f)]
    public float AttackRange { get; private set; }
    [field: SerializeField, Min(0f)]
    public float KnockbackForce { get; private set; }
    [field: SerializeField]
    public bool DidHitEnemy { get; protected set; }
}
