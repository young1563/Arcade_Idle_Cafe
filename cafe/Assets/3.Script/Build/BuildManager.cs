using UnityEngine;
using UnityEngine.InputSystem; // 새 입력 시스템 네임스페이스 추가

public enum BuildState { None, Placing, Removing }

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("설정")]
    public float gridSize = 1.0f;
    public LayerMask groundLayer;
    public LayerMask buildingLayer;

    public BuildState currentState = BuildState.None;

    private GameObject _currentPrefab;
    private GameObject _previewInstance;
    private int _currentPrice;

    void Awake() => Instance = this;

    void Update()
    {
        // 키보드 인스턴스가 있는지 먼저 확인
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

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