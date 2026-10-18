using System.Collections;
using UnityEngine;

public class HitFlashComponent : MonoBehaviour
{
    [SerializeField]
    Renderer m_Renderer;
    [SerializeField]
    string m_EmissionColorProperty = "_EmissionColor";
    [SerializeField, ColorUsage(showAlpha: false, hdr: true)]
    Color m_FlashColor;
    [SerializeField, Min(0.001f)]
    float m_ColorFlashDuration = 0.1f;

    Color m_OriginalColor;
    Coroutine m_FlashCoroutine;

    void Start() => m_OriginalColor = m_Renderer.material.GetColor(m_EmissionColorProperty);

    IEnumerator DoFlash()
    {
        m_Renderer.material.SetColor(m_EmissionColorProperty, m_FlashColor);
        yield return new WaitForSeconds(m_ColorFlashDuration);
        m_Renderer.material.SetColor(m_EmissionColorProperty, m_OriginalColor);
        m_FlashCoroutine = null;
    }

    public void Flash()
    {
        if (m_FlashCoroutine != null) StopCoroutine(m_FlashCoroutine);
        m_FlashCoroutine = StartCoroutine(DoFlash());
    }
}
