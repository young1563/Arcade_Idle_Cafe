using UnityEngine;
using UnityEngine.InputSystem; // 새 입력 시스템 네임스페이스 추가
using UnityEngine.EventSystems;
using DG.Tweening; // 연출을 위한 DOTween

public enum BuildState { None, Placing, Removing }

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("UI 연출 설정")]
    public RectTransform buildPanel; // 하단 패널 (RectTransform)
    public float panelShowY = 50f;   // 패널이 보일 때의 Y 위치
    public float panelHideY = -200f; // 패널이 숨겨질 때의 Y 위치 (화면 밖)
    public float tweenDuration = 0.4f;

    [Header("설정")]
    public float gridSize = 1.0f;
    public LayerMask groundLayer;
    public LayerMask buildingLayer;

    public BuildState currentState = BuildState.None;

    private GameObject _currentPrefab;
    private GameObject _previewInstance;
    private int _currentPrice;

    void Awake() => Instance = this;

    void Start()
    {
        // 시작 시 패널 숨기기
        buildPanel.anchoredPosition = new Vector2(buildPanel.anchoredPosition.x, panelHideY);
    }

    void Update()
    {
        // 키보드 인스턴스가 있는지 먼저 확인
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 'B' 키를 눌러 건설 모드(패널) 토글
        if (keyboard.bKey.wasPressedThisFrame)
        {
            ToggleBuildPanel();
        }

        // 마우스가 UI 위에 있다면 설치/철거 로직 실행 안 함
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // 1. 모드 전환 및 취소 단축키
        if (keyboard.xKey.wasPressedThisFrame) StartRemoveMode();
        if (keyboard.escapeKey.wasPressedThisFrame) CancelMode();

        // 2. 각 모드별 로직
        if (currentState == BuildState.Placing)
        {
            UpdatePlacingLogic(keyboard);
        }
        else if (currentState == BuildState.Removing)
        {
            UpdateRemovingLogic(keyboard);
        }
    }

    public void ToggleBuildPanel()
    {
        bool isShowing = buildPanel.anchoredPosition.y > 0;

        if (isShowing)
        {
            HidePanel();
            CancelMode(); // 패널 닫을 때 현재 잡고 있는 건물도 취소
        }
        else
        {
            ShowPanel();
        }

    }
    public void ShowPanel()
    {
        buildPanel.DOAnchorPosY(panelShowY, tweenDuration).SetEase(Ease.OutBack);
    }

    public void HidePanel()
    {
        buildPanel.DOAnchorPosY(panelHideY, tweenDuration).SetEase(Ease.InSine);
    }

    void UpdatePlacingLogic(Keyboard keyboard)
    {
        UpdatePreviewPosition();

        // R키: 회전
        if (keyboard.rKey.wasPressedThisFrame)
        {
            _previewInstance.transform.Rotate(0, 90, 0);
        }

        // Space키: 설치
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            PlaceObject();
        }
    }

    void UpdateRemovingLogic(Keyboard keyboard)
    {
        // 마우스 위치는 새로운 시스템에서도 Mouse.current로 가져올 수 있지만, 
        // 카메라 레이캐스트는 기존 방식을 섞어 써도 무방합니다.
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildingLayer))
        {
            // Space키: 철거
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                RemoveObject(hit.collider.gameObject);
            }
        }
    }

    // --- 이하 기존 위치 계산 및 설치 로직 동일 ---
    void UpdatePreviewPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float x = Mathf.Round(hit.point.x / gridSize) * gridSize;
            float z = Mathf.Round(hit.point.z / gridSize) * gridSize;
            if (_previewInstance != null)
                _previewInstance.transform.position = new Vector3(x, 0.1f, z);
        }
    }

    public void StartBuildMode(GameObject prefab, int price)
    {
        CancelMode();
        _currentPrefab = prefab;
        _currentPrice = price;
        currentState = BuildState.Placing;
        _previewInstance = Instantiate(prefab);
        ApplyGhostEffect(_previewInstance);
    }

    public void StartRemoveMode()
    {
        if (currentState == BuildState.Removing) { CancelMode(); return; }
        CancelMode();
        currentState = BuildState.Removing;
    }

    void PlaceObject()
    {
        if (MoneyManager.Instance.TrySpendMoney(_currentPrice))
        {
            Instantiate(_currentPrefab, _previewInstance.transform.position, _previewInstance.transform.rotation);
        }
    }

    void RemoveObject(GameObject target)
    {
        Destroy(target);
    }

    public void CancelMode()
    {
        currentState = BuildState.None;
        if (_previewInstance != null) Destroy(_previewInstance);
    }

    void ApplyGhostEffect(GameObject obj)
    {
        foreach (var col in obj.GetComponentsInChildren<Collider>()) col.enabled = false;
        // 프리벤터 스크립트 등 추가 컴포넌트 비활성화 로직 필요시 추가
    }
}