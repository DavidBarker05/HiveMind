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

    Coroutine m_FlashCoroutine;
    MaterialPropertyBlock m_PropertyBlock;
    int m_EmissionColorId;

    void Awake()
    {
        m_PropertyBlock = new MaterialPropertyBlock();
        m_EmissionColorId = Shader.PropertyToID(m_EmissionColorProperty);
    }

    IEnumerator DoFlash()
    {
        m_Renderer.GetPropertyBlock(m_PropertyBlock);
        m_PropertyBlock.SetColor(m_EmissionColorId, m_FlashColor);
        m_Renderer.SetPropertyBlock(m_PropertyBlock);
        yield return new WaitForSeconds(m_ColorFlashDuration);
        m_Renderer.SetPropertyBlock(null);
        m_FlashCoroutine = null;
    }

    public void Flash()
    {
        if (m_FlashCoroutine != null) StopCoroutine(m_FlashCoroutine);
        m_FlashCoroutine = StartCoroutine(DoFlash());
    }
}
