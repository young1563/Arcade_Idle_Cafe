using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum BuildState { None, Placing, Removing }

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("UI 연출 설정")]
    public RectTransform buildPanel;
    public float panelShowY = 50f;
    public float panelHideY = -200f;
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
        if (buildPanel == null)
        {
            Debug.LogError("[BuildManager] buildPanel이 연결되지 않았습니다!");
            return;
        }
        buildPanel.anchoredPosition = new Vector2(buildPanel.anchoredPosition.x, panelHideY);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 'B' 키 토글
        if (keyboard.bKey.wasPressedThisFrame)
        {
            Debug.Log("[BuildManager] B 키 입력: 패널 토글");
            ToggleBuildPanel();
        }

        // 1. 모드 전환 및 취소 단축키
        if (keyboard.xKey.wasPressedThisFrame) StartRemoveMode();
        if (keyboard.escapeKey.wasPressedThisFrame) CancelMode();

        // 마우스가 UI 위에 있다면 설치/철거 로직 차단
        if (EventSystem.current.IsPointerOverGameObject()) return;

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
        bool isShowing = buildPanel.anchoredPosition.y > (panelHideY + 10f); // 대략적인 판정

        if (isShowing) HidePanel();
        else ShowPanel();
    }

    public void ShowPanel() => buildPanel.DOAnchorPosY(panelShowY, tweenDuration).SetEase(Ease.OutBack);
    public void HidePanel() => buildPanel.DOAnchorPosY(panelHideY, tweenDuration).SetEase(Ease.InSine);

    void UpdatePlacingLogic(Keyboard keyboard)
    {
        UpdatePreviewPosition();

        if (_previewInstance == null) return;

        if (keyboard.rKey.wasPressedThisFrame)
        {
            _previewInstance.transform.Rotate(0, 90, 0);
            Debug.Log("[BuildManager] R 키 입력: 미리보기 회전");
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[BuildManager] Space 키 입력: 설치 시도");
            PlaceObject();
        }
    }

    void UpdateRemovingLogic(Keyboard keyboard)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildingLayer))
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                Debug.Log($"[BuildManager] Space 키 입력: {hit.collider.gameObject.name} 철거");
                RemoveObject(hit.collider.gameObject);
            }
        }
    }

    void UpdatePreviewPosition()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // 1. 빨간 선이 씬 뷰에서 바닥을 향해 뻗어 나가는지 확인
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // 바닥을 찾았을 때만 로그를 찍어봅니다.
            // Debug.Log($"[BuildManager] 바닥 감지됨: {hit.collider.name} / 위치: {hit.point}");

            float x = Mathf.Round(hit.point.x / gridSize) * gridSize;
            float z = Mathf.Round(hit.point.z / gridSize) * gridSize;

            if (_previewInstance != null)
            {
                if (!_previewInstance.activeSelf)
                {
                    Debug.Log("[BuildManager] 프리뷰 활성화됨");
                    _previewInstance.SetActive(true);
                }
                _previewInstance.transform.position = new Vector3(x, 0.5f, z); // Y값을 조금 더 높여서 테스트
            }
        }
        else
        {
            // 바닥을 못 찾으면 프리뷰를 끕니다.
            if (_previewInstance != null && _previewInstance.activeSelf)
            {
                Debug.LogWarning("[BuildManager] 바닥 레이어를 감지하지 못해 프리뷰를 숨깁니다.");
                _previewInstance.SetActive(false);
            }
        }
    }

    public void StartBuildMode(GameObject prefab, int price)
    {
        if (prefab == null)
        {
            Debug.LogError("[BuildManager] 전달된 프리팹이 Null입니다!");
            return;
        }

        Debug.Log($"[BuildManager] 건설 모드 진입: {prefab.name} (가격: {price})");
        CancelMode();

        _currentPrefab = prefab;
        _currentPrice = price;
        currentState = BuildState.Placing;

        _previewInstance = Instantiate(prefab);
        _previewInstance.name = "BuildPreview_" + prefab.name;
        ApplyGhostEffect(_previewInstance);
    }

    public void StartRemoveMode()
    {
        if (currentState == BuildState.Removing)
        {
            Debug.Log("[BuildManager] 철거 모드 종료");
            CancelMode();
            return;
        }

        Debug.Log("[BuildManager] 철거 모드 진입 (X 키)");
        CancelMode();
        currentState = BuildState.Removing;
    }

    void PlaceObject()
    {
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("[BuildManager] MoneyManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        if (MoneyManager.Instance.TrySpendMoney(_currentPrice))
        {
            GameObject NewObj = Instantiate(_currentPrefab, _previewInstance.transform.position, _previewInstance.transform.rotation);
            Debug.Log($"[BuildManager] 설치 완료: {NewObj.name} (잔액: {MoneyManager.Instance.currentMoney})");
        }
        else
        {
            Debug.LogWarning($"[BuildManager] 설치 실패: 돈이 부족합니다! (필요: {_currentPrice}, 보유: {MoneyManager.Instance.currentMoney})");
        }
    }

    void RemoveObject(GameObject target)
    {
        Debug.Log($"[BuildManager] 오브젝트 삭제: {target.name}");
        Destroy(target);
    }

    public void CancelMode()
    {
        if (currentState != BuildState.None) Debug.Log("[BuildManager] 모든 모드 취소 및 상태 초기화");
        currentState = BuildState.None;
        if (_previewInstance != null) Destroy(_previewInstance);
    }

    void ApplyGhostEffect(GameObject obj)
    {
        // 콜라이더 비활성화 로그
        int colCount = 0;
        foreach (var col in obj.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
            colCount++;
        }
        Debug.Log($"[BuildManager] 프리뷰 Ghost 효과 적용: {colCount}개의 콜라이더 비활성화");
    }
}