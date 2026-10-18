using UnityEngine;

public class Katana : MeleeWeapon
{
    [Header("Katana")]
    [SerializeField, Range(30f, 90f)]
    float m_AttackAngle = 60f; // Half-angle, 90 degrees is full area in front of player

    public override bool Attack(Transform origin)
    {
        if (!CanAttack) return false;
        m_AttackCooldownRemaining = TimeBetweenAttacks;
        Collider[] enemies = GetEnemiesInAttackRadius(origin);
        DidHitEnemy = enemies.Length > 0;
        foreach (Collider enemy in enemies)
        {
            if (!IsEnemyInAngle(origin, enemy)) continue;
            HealthComponent healthComponent = enemy.gameObject.GetComponent<HealthComponent>();
            if (healthComponent) healthComponent.TakeDamage(Damage);
            Rigidbody rb = enemy.gameObject.GetComponent<Rigidbody>();
            if (rb)
            {
                Vector3 toEnemy = (enemy.bounds.center - origin.position).normalized;
                Vector3 force = toEnemy * KnockbackForce;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
        return true;
    }

    public override void UpdateWeapon(float deltaTime)
    {
        if (m_AttackCooldownRemaining > 0f) m_AttackCooldownRemaining -= deltaTime;
    }

    Collider[] GetEnemiesInAttackRadius(Transform origin) => Physics.OverlapSphere(
        position: origin.position,
        radius: AttackRange,
        layerMask: HittableLayers,
        queryTriggerInteraction: QueryTriggerInteraction.Collide
    );

    bool IsEnemyInAngle(Transform origin, Collider enemy) =>
        IsPointInAngle(origin, enemy.bounds.center)
        || IsPointInAngle(origin, enemy.transform.position);

    bool IsPointInAngle(Transform origin, Vector3 point)
    {
        Vector3 toPoint = point - origin.position;
        if (toPoint.sqrMagnitude < 0.0001f) return true;
        float cosAngle = Mathf.Cos(m_AttackAngle * Mathf.Deg2Rad);
        return Vector3.Dot(origin.forward, toPoint.normalized) >= cosAngle;
    }
}
