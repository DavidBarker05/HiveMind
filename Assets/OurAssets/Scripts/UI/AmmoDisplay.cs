using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField]
    WeaponHandlerComponent m_WeaponHandler;
    [SerializeField]
    TMP_Text m_Text;
    [SerializeField, Tooltip("{0} is the ammo in the magazine, {1} is the reserve ammo.")]
    string m_Format = "{0} / {1}";

    Gun m_LastGun;
    int m_LastMagazine = -1;
    int m_LastReserve = -1;

    void OnEnable() => Refresh(true);

    void Update() => Refresh(false);

    void Refresh(bool bForce)
    {
        if (!m_WeaponHandler || !m_Text) return;
        Gun gun = m_WeaponHandler.CurrentGun;
        int magazine = gun ? gun.MagazineAmmo : -1;
        int reserve = gun ? gun.ReserveAmmo : -1;
        if (!bForce && gun == m_LastGun && magazine == m_LastMagazine && reserve == m_LastReserve) return;
        m_LastGun = gun;
        m_LastMagazine = magazine;
        m_LastReserve = reserve;
        m_Text.text = gun ? string.Format(m_Format, magazine, reserve) : string.Empty;
    }
}
