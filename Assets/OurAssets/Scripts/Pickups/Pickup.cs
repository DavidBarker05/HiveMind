using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum WaveType
{
    Sine,
    Cosine
}

// SphereCollider is actually the most performant collider, so just use it
[RequireComponent(typeof(SphereCollider))]
public abstract class Pickup : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField, Range(0f, 360f)]
    float m_DegreesPerSecond = 30f;
    [Header("Vertical Movement")]
    [SerializeField]
    WaveType m_WaveType = WaveType.Cosine; // By default starts at 0 y so probably best
    [SerializeField, Range(0f, 1f)]
    float m_Amplitude = 0.1f;
    [SerializeField, Range(0f, 2f)]
    float m_Frequency = 1f;

    Vector3 m_StartingLocalPosition;
    float m_CurrentTime = 0f;

    public UnityEvent OnCollectionCompleted;

    void Awake()
    {
        m_StartingLocalPosition = transform.localPosition;
        GetComponent<SphereCollider>().isTrigger = true;
    }

    void Update()
    {
        m_CurrentTime += Time.deltaTime;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + m_DegreesPerSecond * Time.unscaledDeltaTime, transform.localEulerAngles.z);
        transform.localPosition = new Vector3(m_StartingLocalPosition.x, m_StartingLocalPosition.y + Wave(), m_StartingLocalPosition.z);
    }

    protected abstract void OnPickupCollected(Collider other);

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
            OnPickupCollected(other);
    }

    float Wave()
    {
        if (m_WaveType == WaveType.Sine) return Mathf.Sin(m_CurrentTime * m_Frequency) * m_Amplitude;
        return Mathf.Cos(m_CurrentTime * m_Frequency) * m_Amplitude;
    }
}
