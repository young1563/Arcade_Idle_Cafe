using UnityEngine;
using UnityEngine.UIElements;

public class SortingHUDController : MonoBehaviour
{
    private VisualElement _root;
    private Label _levelLabel;
    private Label _progressLabel;
    private VisualElement _progressBarFill;

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        _root = uiDoc.rootVisualElement;

        _levelLabel = _root.Q<Label>("lbl-level");
        _progressLabel = _root.Q<Label>("lbl-progress");
        _progressBarFill = _root.Q<VisualElement>("progress-bar-fill");

        // Hook up buttons
        var btnSettings = _root.Q<Button>("btn-settings");
        if (btnSettings != null)
        {
            btnSettings.clicked += () => SortingGameManager.Instance.LoadLevel(SortingGameManager.Instance.currentLevel);
            // Treat settings as "Restart" for now
        }
    }

    private void LateUpdate()
    {
        if (SortingGameManager.Instance == null) return;

        if (_levelLabel != null)
            _levelLabel.text = SortingGameManager.Instance.currentLevel.ToString();

        // Maybe show win progress?
        if (_progressLabel != null)
            _progressLabel.text = "SORTING...";
    }
}
