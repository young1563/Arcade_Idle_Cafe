using UnityEngine;
using UnityEngine.UIElements;

public class SortingHUDController : MonoBehaviour
{
    private VisualElement _root;
    private Label _levelLabel;
    private Label _progressLabel;
    private VisualElement _progressBarFill;

    private VisualElement _winPopup;
    private Label _moneyLabel;

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        _root = uiDoc.rootVisualElement;

        _levelLabel = _root.Q<Label>("lbl-level");
        _progressLabel = _root.Q<Label>("lbl-progress");
        _progressBarFill = _root.Q<VisualElement>("progress-bar-fill");
        _moneyLabel = _root.Q<Label>("lbl-money");
        _winPopup = _root.Q<VisualElement>("win-popup");

        // Hook up buttons
        _root.Q<Button>("btn-restart")?.RegisterCallback<ClickEvent>(ev => {
            SortingGameManager.Instance.LoadLevel(SortingGameManager.Instance.currentLevel);
        });

        _root.Q<Button>("btn-next")?.RegisterCallback<ClickEvent>(ev => {
            _winPopup.style.display = DisplayStyle.None;
            SortingGameManager.Instance.currentLevel++;
            SortingGameManager.Instance.LoadLevel(SortingGameManager.Instance.currentLevel);
        });

        // Register Event
        if (SortingGameManager.Instance != null)
        {
            SortingGameManager.Instance.OnLevelComplete += ShowWinPopup;
        }
    }

    private void OnDisable()
    {
        if (SortingGameManager.Instance != null)
        {
            SortingGameManager.Instance.OnLevelComplete -= ShowWinPopup;
        }
    }

    private void ShowWinPopup()
    {
        if (_winPopup != null)
            _winPopup.style.display = DisplayStyle.Flex;
    }

    private void LateUpdate()
    {
        if (SortingGameManager.Instance == null) return;

        if (_levelLabel != null)
            _levelLabel.text = SortingGameManager.Instance.currentLevel.ToString();
        
        // Sync money if MoneyManager exists
        if (MoneyManager.Instance != null && _moneyLabel != null)
            _moneyLabel.text = MoneyManager.Instance.currentMoney.ToString();
    }
}
