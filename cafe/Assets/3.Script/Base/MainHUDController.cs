using UnityEngine;
using UnityEngine.UIElements;

public class MainHUDController : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _root;

    // UI Elements
    private Label _moneyLabel;
    private Label _gemLabel;
    private Label _tokenLabel;
    private Label _levelLabel;
    private Label _progressLabel;
    private VisualElement _progressBarFill;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        _root = _uiDocument.rootVisualElement;

        // Find Elements
        _moneyLabel = _root.Q<Label>("lbl-money");
        _gemLabel = _root.Q<Label>("lbl-gem");
        _tokenLabel = _root.Q<Label>("lbl-token");
        _levelLabel = _root.Q<Label>("lbl-level");
        _progressLabel = _root.Q<Label>("lbl-progress");
        _progressBarFill = _root.Q<VisualElement>("progress-bar-fill");

        // Register Button Callbacks
        _root.Q<Button>("btn-settings").clicked += () => Debug.Log("Settings Opened");
        _root.Q<Button>("btn-map").clicked += () => Debug.Log("Map Opened");
        _root.Q<Button>("btn-shop").clicked += () => Debug.Log("Shop Opened");
        
        var btnPlusMoney = _root.Q<Button>(className: "btn-plus"); // Example of finding by class
        if (btnPlusMoney != null) btnPlusMoney.clicked += () => Debug.Log("Add Money Clicked");
    }

    private void Update()
    {
        // Example: Sync with MoneyManager (Updating every frame is simple for prototype, 
        // but event-driven is better for performance)
        if (MoneyManager.Instance != null)
        {
            UpdateMoney(MoneyManager.Instance.currentMoney);
        }
    }

    public void UpdateMoney(int amount)
    {
        if (_moneyLabel != null)
            _moneyLabel.text = FormatCurrency(amount);
    }

    public void UpdateLevel(int level, float progressRaw, float progressMax)
    {
        if (_levelLabel != null) _levelLabel.text = level.ToString();
        if (_progressLabel != null) _progressLabel.text = $"{(int)progressRaw}/{(int)progressMax}";
        
        if (_progressBarFill != null)
        {
            float percentage = (progressRaw / progressMax) * 100f;
            _progressBarFill.style.width = new Length(percentage, LengthUnit.Percent);
        }
    }

    private string FormatCurrency(int amount)
    {
        if (amount >= 1000)
            return (amount / 1000f).ToString("F1") + "K";
        return amount.ToString();
    }
}
