using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField]
    GameObject m_Gun;
    [SerializeField]
    Transform m_WeaponSlot;

    void Start()
    {
        m_Gun.transform.SetParent(m_WeaponSlot, true);
        m_Gun.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
