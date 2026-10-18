using UnityEngine;
using Util.UnityUtils;

public class Shotgun : Gun
{
    [Header("Shotgun")]
    [SerializeField, Min(0.01f)]
    float m_AttackRange;
    [SerializeField, Min(1)]
    int m_NumBullets = 7;
    [SerializeField, Min(0f)]
    float m_BulletSpread = 5f;
    [SerializeField]
    ParticleSystem m_MuzzleFlash;
    [SerializeField, Min(0.001f)]
    float m_BulletRadius = 0.05f;

    Vector3 GetSpreadDirection(Transform origin, float spreadDegrees)
    {
        if (spreadDegrees <= 0f) return origin.forward;
        Vector2 offset = Random.insideUnitCircle * Mathf.Tan(spreadDegrees * Mathf.Deg2Rad);
        return (origin.forward + origin.right * offset.x + origin.up * offset.y).normalized;
    }

    bool Shoot(Transform origin, Vector3 direction, out RaycastHit hitInfo) => Physics.SphereCast(
        origin: origin.position,
        radius: m_BulletRadius,
        direction: direction,
        hitInfo: out hitInfo,
        maxDistance: m_AttackRange,
        layerMask: HittableLayers,
        queryTriggerInteraction: QueryTriggerInteraction.Ignore
    );

    public override bool Attack(Transform origin)
    {
        if (!CanAttack || NeedsReloading) return false;
        m_AttackCooldownRemaining = TimeBetweenAttacks;
        --MagazineAmmo;
        for (int i = 0; i < m_NumBullets; ++i)
        {
            Vector3 dir = GetSpreadDirection(origin, m_BulletSpread);
            if (Shoot(origin, dir, out RaycastHit hitInfo))
            {
                HealthComponent health = hitInfo.GetComponent<HealthComponent>();
                if (health) health.TakeDamage(Damage);
            }
        }
        m_MuzzleFlash.Play();
        return true;
    }

    public override void UpdateWeapon(float deltaTime)
    {
        if (m_AttackCooldownRemaining > 0f) m_AttackCooldownRemaining -= deltaTime;
    }
}
