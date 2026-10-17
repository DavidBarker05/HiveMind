using UnityEngine;
using Util.UnityUtils;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    float m_ExplosionTime = 4.5f; // Grenades irl explode within 4-5 seconds iirc
    [SerializeField, Min(0.1f)]
    float m_ExplosionRadius = 5f;

    bool m_bPrimed = false;
    int m_Damage;
    LayerMask m_DamageableLayers;
    LayerMask m_HittableLayers;
    float m_CurrentTime;

    public void Init(int damage, LayerMask damageableLayers, LayerMask hittableLayers)
    {
        m_Damage = damage;
        m_DamageableLayers = damageableLayers;
        m_HittableLayers = hittableLayers;
        m_CurrentTime = 0f;
        m_bPrimed = true;
    }

    void Update()
    {
        if (!m_bPrimed) return;
        m_CurrentTime += Time.deltaTime;
        if (m_CurrentTime < m_ExplosionTime) return;
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_DamageableLayers, QueryTriggerInteraction.Collide);
        foreach (Collider enemy in hitEnemies)
        {
            if (Physics.Linecast(
                start: transform.position,
                end: enemy.bounds.center,
                hitInfo: out RaycastHit hitInfo,
                layerMask: m_HittableLayers,
                queryTriggerInteraction: QueryTriggerInteraction.Collide))
            {
                HealthComponent healthComponent = hitInfo.GetComponent<HealthComponent>();
                if (healthComponent) healthComponent.TakeDamage(m_Damage);
            }
        }
        Destroy(gameObject);
    }
}
