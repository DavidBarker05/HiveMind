using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour
{
    [SerializeField]
    Upgrade m_Upgrade;
    [SerializeField]
    TMP_Text m_Text;

    Button m_Button;

    string m_UpgradeName;
    string m_PurchasedLevelText;
    string m_CostText;

    void Awake()
    {
        m_Button = GetComponent<Button>();
        m_UpgradeName = UpgradeHelper.UpgradeDisplayName(m_Upgrade);
        m_CostText = $"{m_Upgrade.PointsCost} Points";
        m_Button.onClick.AddListener(() => UpgradeSystem.Instance.PurchaseUpgrade(m_Upgrade));
        UpgradeSystem.Instance.OnLevelPointsChanged.AddListener(UpdateButton);
    }

    void OnEnable() => UpdateButton();

    void OnDestroy()
    {
        UpgradeSystem system = UpgradeSystem.InstanceOrNull;
        if (system) system.OnLevelPointsChanged.RemoveListener(UpdateButton);
    }

    void UpdateButton()
    {
        m_PurchasedLevelText =
            UpgradeHelper.IsUpgradeOneTimePurchase(m_Upgrade)
            ? (
                UpgradeSystem.Instance.HasBeenPurchased(m_Upgrade)
                ? "Purchased"
                : $"Purchase"
            )
            : $"Level {UpgradeSystem.Instance.GetUpgradeLevel(m_Upgrade)}";
        m_Text.text = $"{m_UpgradeName}\n{m_PurchasedLevelText}\n{m_CostText}";
        m_Button.interactable = UpgradeSystem.Instance.CanPurchaseUpgrade(m_Upgrade);
    }
}
