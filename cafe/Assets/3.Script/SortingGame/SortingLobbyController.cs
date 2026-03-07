using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SortingLobbyController : MonoBehaviour
{
    private VisualElement _root;
    private Button _btnStart;
    private Button _btnQuit;

    [Header("Settings")]
    public string inGameSceneName = "StackGame"; // 인게임 씬 이름 확인

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        _root = uiDoc.rootVisualElement;

        _btnStart = _root.Q<Button>("btn-start");
        _btnQuit = _root.Q<Button>("btn-quit");

        if (_btnStart != null)
        {
            _btnStart.RegisterCallback<ClickEvent>(OnStartClicked);
        }

        if (_btnQuit != null)
        {
            _btnQuit.RegisterCallback<ClickEvent>(OnQuitClicked);
        }
    }

    private void OnStartClicked(ClickEvent evt)
    {
        Debug.Log("Starting Game...");
        // 씬 전환 전 효과음이나 페이드 효과를 추가할 수 있습니다.
        SceneManager.LoadScene(inGameSceneName);
    }

    private void OnQuitClicked(ClickEvent evt)
    {
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
