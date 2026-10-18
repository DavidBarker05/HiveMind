using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadMenuButton : MonoBehaviour
{
    [SerializeField]
    int m_MainMenuSceneIndex = 0;
    [SerializeField]
    LoadingScreen m_LoadingScreen;

    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        Time.timeScale = 1f;
        m_LoadingScreen.SceneIndexToLoad = m_MainMenuSceneIndex;
        m_LoadingScreen.gameObject.SetActive(true);
    });
}
