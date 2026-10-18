using UnityEngine;

public class HealthPickup : Pickup
{
    [Header("Health Pickup")]
    [SerializeField, Range(0f, 1f)]
    float m_PercentageOfMaxHealthGained = 0.25f;

    protected override void OnPickupCollected(Collider other)
    {
        HealthComponent healthComponent = other.gameObject.GetComponent<HealthComponent>();
        if (!healthComponent) return;
        int healthGained = Mathf.RoundToInt(healthComponent.MaxHealth * m_PercentageOfMaxHealthGained);
        healthComponent.ReceiveHealth(healthGained);
        OnCollectionCompleted?.Invoke();
    }
}
