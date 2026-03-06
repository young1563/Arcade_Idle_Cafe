using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using UnityEngine.InputSystem;

public class SortingHUDController : MonoBehaviour
{
    public static SortingHUDController Instance;

    private VisualElement _root;
    private Label _levelLabel;
    
    private VisualElement _winPopup;
    private VisualElement _tutorialOverlay;
    private VisualElement _tutorialHand;
    private Label _tutorialLabel;

    private void Awake()
    {
        Instance = this;
    }

    public void HideTutorial()
    {
        if (_tutorialOverlay != null)
        {
            _tutorialOverlay.style.display = DisplayStyle.None;
            DOTween.Kill(_tutorialHand);
        }
    }

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        _root = uiDoc.rootVisualElement;

        _levelLabel = _root.Q<Label>("lbl-level");
        _winPopup = _root.Q<VisualElement>("win-popup");
        _tutorialOverlay = _root.Q<VisualElement>("tutorial-overlay");
        _tutorialHand = _root.Q<VisualElement>("tutorial-hand");
        _tutorialLabel = _root.Q<Label>("lbl-tutorial");

        // Hook up buttons
        _root.Q<Button>("btn-restart")?.RegisterCallback<ClickEvent>(ev => {
            SortingGameManager.Instance.LoadLevel(SortingGameManager.Instance.currentLevel);
        });

        _root.Q<Button>("btn-next")?.RegisterCallback<ClickEvent>(ev => {
            _winPopup.style.display = DisplayStyle.None;
            SortingGameManager.Instance.currentLevel++;
            SortingGameManager.Instance.LoadLevel(SortingGameManager.Instance.currentLevel);
            CheckTutorial();
        });

        // Register Event
        if (SortingGameManager.Instance != null)
        {
            SortingGameManager.Instance.OnLevelComplete += ShowWinPopup;
        }

        CheckTutorial();
    }

    private void CheckTutorial()
    {
        if (SortingGameManager.Instance.currentLevel == 1)
        {
            _tutorialOverlay.style.display = DisplayStyle.Flex;
            StartHandAnimation();
        }
        else
        {
            _tutorialOverlay.style.display = DisplayStyle.None;
            DOTween.Kill(_tutorialHand); // Ensure animation is stopped if tutorial is hidden
        }
    }

    private void StartHandAnimation()
    {
        // Kill any existing animation to prevent conflicts
        DOTween.Kill(_tutorialHand);

        // Simple Left-Right loop for the hand icon
        _tutorialHand.style.left = new Length(30, LengthUnit.Percent);
        DOTween.To(() => _tutorialHand.resolvedStyle.left,
                   x => _tutorialHand.style.left = new Length(x, LengthUnit.Pixel),
                   Screen.width * 0.7f, 1.5f)
               .SetLoops(-1, LoopType.Yoyo)
               .SetEase(Ease.InOutQuad)
               .SetTarget(_tutorialHand); // Set target for easier killing
    }

    private void OnDisable()
    {
        if (SortingGameManager.Instance != null)
        {
            SortingGameManager.Instance.OnLevelComplete -= ShowWinPopup;
        }
        DOTween.Kill(_tutorialHand); // Ensure animation is stopped when disabled
    }

    private void ShowWinPopup()
    {
        if (_winPopup != null)
            _winPopup.style.display = DisplayStyle.Flex;
    }

    private void LateUpdate()
    {
        if (SortingGameManager.Instance == null) return;

        // Dynamic Tutorial logic
        if (_tutorialOverlay != null && _tutorialOverlay.style.display == DisplayStyle.Flex)
        {
            if (SortingGameManager.Instance.SelectedTube == null)
            {
                if (_tutorialLabel != null) _tutorialLabel.text = "디저트가 든 튜브를 선택하세요!";
            }
            else
            {
                if (_tutorialLabel != null) _tutorialLabel.text = "다른 튜브를 눌러서 옮기세요!";
            }

            // Hide tutorial once a level progress is made (e.g., first valid move)
            // Or just hide it after they've clearly understood (handled in GameManager if we want, 
            // but here we can just check if any tube has changed or just hide after first move)
        }

        if (_levelLabel != null)
            _levelLabel.text = SortingGameManager.Instance.currentLevel.ToString();
    }
}
