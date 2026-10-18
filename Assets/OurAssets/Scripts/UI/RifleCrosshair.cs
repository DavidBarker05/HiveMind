using UnityEngine;

public class RifleCrosshair : Crosshair
{
    [SerializeField]
    RectTransform m_TopBar;
    [SerializeField]
    RectTransform m_BottomBar;
    [SerializeField]
    RectTransform m_LeftBar;
    [SerializeField]
    RectTransform m_RightBar;
    [SerializeField, Min(0f)]
    float m_MaxExtraDistance = 40f;

    Vector2 m_TopRest, m_BottomRest, m_LeftRest, m_RightRest;
    bool m_bCaptured;

    void Awake() => CaptureRestingPositions();

    void CaptureRestingPositions()
    {
        if (m_bCaptured) return;
        m_bCaptured = true;
        if (m_TopBar) m_TopRest = m_TopBar.anchoredPosition;
        if (m_BottomBar) m_BottomRest = m_BottomBar.anchoredPosition;
        if (m_LeftBar) m_LeftRest = m_LeftBar.anchoredPosition;
        if (m_RightBar) m_RightRest = m_RightBar.anchoredPosition;
    }

    public void SetSpread(float spread01)
    {
        CaptureRestingPositions();
        float extra = Mathf.Clamp01(spread01) * m_MaxExtraDistance;
        if (m_TopBar) m_TopBar.anchoredPosition = m_TopRest + Vector2.up * extra;
        if (m_BottomBar) m_BottomBar.anchoredPosition = m_BottomRest + Vector2.down * extra;
        if (m_LeftBar) m_LeftBar.anchoredPosition = m_LeftRest + Vector2.left * extra;
        if (m_RightBar) m_RightBar.anchoredPosition = m_RightRest + Vector2.right * extra;
    }
}
