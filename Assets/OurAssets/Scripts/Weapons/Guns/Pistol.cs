using UnityEngine;
using Util.UnityUtils;

public class Pistol : Gun
{
    [Header("Pistol")]
    [SerializeField, Min(0.01f)]
    float m_AttackRange;
    [SerializeField]
    ParticleSystem m_MuzzleFlash;

    bool Shoot(Transform origin, out RaycastHit hitInfo) => Physics.Raycast(
        origin: origin.position,
        direction: origin.forward,
        maxDistance: m_AttackRange,
        hitInfo: out hitInfo,
        layerMask: HittableLayers,
        queryTriggerInteraction: QueryTriggerInteraction.Ignore
    );

    public override bool Attack(Transform origin)
    {
        if (!CanAttack || NeedsReloading) return false;
        m_AttackCooldownRemaining = TimeBetweenAttacks;
        --MagazineAmmo;
        if (Shoot(origin, out RaycastHit hitInfo))
        {
            HealthComponent healthComponent = hitInfo.GetComponent<HealthComponent>();
            if (healthComponent) healthComponent.TakeDamage(Damage);
        }
        m_MuzzleFlash.Play();
        return true;
    }

    public override void UpdateWeapon(float deltaTime)
    {
        if (m_AttackCooldownRemaining > 0f) m_AttackCooldownRemaining -= deltaTime;
    }
}
