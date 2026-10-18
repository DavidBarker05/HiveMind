using System.Collections;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField]
    Pickup m_Pickup;
    [SerializeField, Min(0.01f)]
    float m_PickupDisableTime = 10f;

    void Start() => m_Pickup.OnCollectionCompleted.AddListener(() => StartCoroutine(RespawnPickup()));

    IEnumerator RespawnPickup()
    {
        m_Pickup.gameObject.SetActive(false);
        yield return new WaitForSeconds(m_PickupDisableTime);
        m_Pickup.gameObject.SetActive(true);
    }
}
