using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHitScreenEffect : MonoBehaviour
{
    [SerializeField]
    HealthComponent m_HealthComponent;
    [SerializeField, Range(0f, 1f)]
    float m_MaxIntensity = 0.6f;
    [SerializeField, Min(0.01f)]
    float m_FadeInDuration = 0.05f;
    [SerializeField, Min(0.01f)]
    float m_FadeOutDuration = 0.4f;
    [SerializeField, Tooltip("Layer of the runtime Volume. It must be included in the camera's Volume Mask.")]
    int m_VolumeLayer = 0;
    [SerializeField, Tooltip("Higher priority volumes override lower ones.")]
    float m_VolumePriority = 100f;

    [Header("Look")]
    [SerializeField]
    Color m_Color = new(0.8f, 0f, 0f, 1f);
    [SerializeField, Range(0f, 1f)]
    float m_ClearCentre = 0.35f;
    [SerializeField, Range(0f, 1f)]
    float m_Softness = 0.5f;
    [SerializeField, Range(0f, 1f)]
    float m_CentreTint = 0.05f;

    Volume m_Volume;
    VolumeProfile m_Profile;
    Coroutine m_Routine;

    void Awake()
    {
        if (!m_HealthComponent) m_HealthComponent = GetComponentInParent<HealthComponent>();
        BuildVolume();
    }

    void OnEnable()
    {
        if (m_HealthComponent) m_HealthComponent.OnDamageTaken.AddListener(Play);
    }

    void OnDisable()
    {
        if (m_HealthComponent) m_HealthComponent.OnDamageTaken.RemoveListener(Play);
        if (m_Routine != null) StopCoroutine(m_Routine);
        m_Routine = null;
        if (m_Volume) m_Volume.weight = 0f;
    }

    void OnDestroy()
    {
        if (m_Profile) Destroy(m_Profile);
    }

    public void Play()
    {
        if (m_Routine != null) StopCoroutine(m_Routine);
        m_Routine = StartCoroutine(DoEffect());
    }

    IEnumerator DoEffect()
    {
        float startWeight = m_Volume.weight;
        for (float t = 0f; t < m_FadeInDuration; t += Time.unscaledDeltaTime)
        {
            m_Volume.weight = Mathf.Lerp(startWeight, 1f, t / m_FadeInDuration);
            yield return null;
        }
        for (float t = 0f; t < m_FadeOutDuration; t += Time.unscaledDeltaTime)
        {
            m_Volume.weight = Mathf.Lerp(1f, 0f, t / m_FadeOutDuration);
            yield return null;
        }
        m_Volume.weight = 0f;
        m_Routine = null;
    }

    void BuildVolume()
    {
        m_Profile = ScriptableObject.CreateInstance<VolumeProfile>();
        HitScreenEffect effect = m_Profile.Add<HitScreenEffect>();
        effect.Intensity.Override(m_MaxIntensity);
        effect.Color.Override(m_Color);
        effect.ClearCentre.Override(m_ClearCentre);
        effect.Softness.Override(m_Softness);
        effect.CentreTint.Override(m_CentreTint);

        GameObject volumeObject = new("HitScreenEffectVolume") { layer = m_VolumeLayer };
        volumeObject.transform.SetParent(transform, false);
        m_Volume = volumeObject.AddComponent<Volume>();
        m_Volume.isGlobal = true;
        m_Volume.priority = m_VolumePriority;
        m_Volume.sharedProfile = m_Profile;
        m_Volume.weight = 0f;
    }
}
