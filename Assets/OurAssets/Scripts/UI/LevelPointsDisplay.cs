using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LevelPointsDisplay : MonoBehaviour
{
    [SerializeField]
    string m_Format = "Points: {0}";

    TMP_Text m_Text;

    void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
        UpgradeSystem.Instance.OnLevelPointsChanged.AddListener(UpdateLevelPoints);
    }

    void OnEnable() => UpdateLevelPoints();

    void UpdateLevelPoints() => m_Text.text = string.Format(m_Format, UpgradeSystem.Instance.LevelPoints);
}
