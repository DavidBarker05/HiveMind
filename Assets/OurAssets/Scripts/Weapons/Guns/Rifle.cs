using UnityEngine;
using Util.UnityUtils;

public class Rifle : Gun
{
    [Header("Rifle")]
    [SerializeField, Min(0.01f)]
    float m_AttackRange;
    [SerializeField, Min(0f)]
    float m_SpreadPerShot = 0.4f;
    [SerializeField, Min(0f)]
    float m_MaxSpread = 1.5f;
    [SerializeField, Min(0f)]
    float m_RecoveryRate = 4f;
    [SerializeField]
    ParticleSystem m_MuzzleFlash;

    float m_CurrentSpread;
    bool m_bAttacking = false;

    Vector3 GetSpreadDirection(Transform origin, float spreadDegrees)
    {
        if (spreadDegrees <= 0f) return origin.forward;
        Vector2 offset = Random.insideUnitCircle * Mathf.Tan(spreadDegrees * Mathf.Deg2Rad);
        return (origin.forward + origin.right * offset.x + origin.up * offset.y).normalized;
    }

    bool Shoot(Transform origin, Vector3 direction, out RaycastHit hitInfo) => Physics.Raycast(
        origin: origin.position,
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
        Vector3 dir = GetSpreadDirection(origin, m_CurrentSpread);
        if (Shoot(origin, dir, out RaycastHit hitInfo))
        {
            HealthComponent health = hitInfo.GetComponent<HealthComponent>();
            if (health) health.TakeDamage(Damage);
        }
        m_CurrentSpread = Mathf.Min(m_CurrentSpread + m_SpreadPerShot, m_MaxSpread);
        m_bAttacking = true;
        m_MuzzleFlash.Play();
        return true;
    }

    public float SpreadNormalised => m_MaxSpread > 0f ? m_CurrentSpread / m_MaxSpread : 0f;

    public void StopAttacking() => m_bAttacking = false;

    public override void UpdateWeapon(float deltaTime)
    {
        if (m_AttackCooldownRemaining > 0f) m_AttackCooldownRemaining -= deltaTime;
        if (m_bAttacking) return;
        m_CurrentSpread = Mathf.MoveTowards(m_CurrentSpread, 0f, m_RecoveryRate * Time.deltaTime);
    }
}
