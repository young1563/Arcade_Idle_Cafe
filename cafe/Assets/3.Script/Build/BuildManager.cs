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

    private FacilityData _currentData;
    private GameObject _previewInstance;

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

        if (keyboard.bKey.wasPressedThisFrame)
        {
            ToggleBuildPanel();
        }

        if (keyboard.xKey.wasPressedThisFrame) StartRemoveMode();
        if (keyboard.escapeKey.wasPressedThisFrame) CancelMode();

        if (EventSystem.current.IsPointerOverGameObject()) return;

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
        bool isShowing = buildPanel.anchoredPosition.y > (panelHideY + 10f);
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
        }

        if (keyboard.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceObject();
        }
    }

    void UpdateRemovingLogic(Keyboard keyboard)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildingLayer))
        {
            if (keyboard.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
            {
                RemoveObject(hit.collider.gameObject);
            }
        }
    }

    void UpdatePreviewPosition()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float x = Mathf.Round(hit.point.x / gridSize) * gridSize;
            float z = Mathf.Round(hit.point.z / gridSize) * gridSize;

            if (_previewInstance != null)
            {
                if (!_previewInstance.activeSelf) _previewInstance.SetActive(true);
                _previewInstance.transform.position = new Vector3(x, 0.1f, z);
            }
        }
        else
        {
            if (_previewInstance != null && _previewInstance.activeSelf) _previewInstance.SetActive(false);
        }
    }

    public void StartBuildMode(FacilityData data)
    {
        if (data == null || data.prefab == null) return;

        CancelMode();
        _currentData = data;
        currentState = BuildState.Placing;

        _previewInstance = Instantiate(data.prefab);
        _previewInstance.name = "Preview_" + data.facilityName;
        ApplyGhostEffect(_previewInstance);
    }

    public void StartRemoveMode()
    {
        if (currentState == BuildState.Removing)
        {
            CancelMode();
            return;
        }
        CancelMode();
        currentState = BuildState.Removing;
    }

    void PlaceObject()
    {
        if (_currentData == null) return;

        if (Physics.CheckBox(_previewInstance.transform.position, new Vector3(0.4f, 0.4f, 0.4f), _previewInstance.transform.rotation, buildingLayer))
        {
            Debug.LogWarning("이미 설비가 존재합니다.");
            return;
        }

        if (MoneyManager.Instance.TrySpendMoney(_currentData.price))
        {
            GameObject newObj = Instantiate(_currentData.prefab, _previewInstance.transform.position, _previewInstance.transform.rotation);
            
            var facility = newObj.GetComponent<BaseFacility>();
            if (facility != null) facility.facilityData = _currentData;
            
            Debug.Log($"{_currentData.facilityName} 설치 완료");
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
        _currentData = null;
    }

    void ApplyGhostEffect(GameObject obj)
    {
        foreach (var col in obj.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
    }
}
