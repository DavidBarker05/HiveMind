using UnityEngine;
using Util.UnityUtils;

public class Bat : MeleeWeapon
{
    [Header("Bat")]
    [SerializeField, Min(0.01f)]
    float m_SphereCastRadius = 0.5f;

    public override bool Attack(Transform origin)
    {
        if (!CanAttack) return false;
        m_AttackCooldownRemaining = TimeBetweenAttacks;
        if (Hit(origin, out RaycastHit hitInfo))
        {
            HealthComponent healthComponent = hitInfo.GetComponent<HealthComponent>();
            if (healthComponent) healthComponent.TakeDamage(Damage);
            Rigidbody rb = hitInfo.GetComponent<Rigidbody>();
            if (rb)
            {
                Vector3 toEnemy = (hitInfo.point - origin.position).normalized;
                Vector3 force = toEnemy * KnockbackForce;
                rb.AddForce(force, ForceMode.Impulse);
            }
            DidHitEnemy = true;
        }
        else DidHitEnemy = false;
        return true;
    }

    public override void UpdateWeapon(float deltaTime)
    {
        if (m_AttackCooldownRemaining > 0f) m_AttackCooldownRemaining -= deltaTime;
    }

    bool Hit(Transform origin, out RaycastHit hitInfo) => Physics.SphereCast(
        origin: origin.position,
        radius: m_SphereCastRadius,
        direction: origin.forward,
        hitInfo: out hitInfo,
        maxDistance: AttackRange,
        layerMask: HittableLayers,
        queryTriggerInteraction: QueryTriggerInteraction.Collide
    );
}
