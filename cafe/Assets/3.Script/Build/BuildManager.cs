using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public enum BuildState { None, Placing, Removing }

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("UI 연출 및 안내")]
    public RectTransform buildPanel;
    public TextMeshProUGUI guideText;
    public float panelShowY = 50f;
    public float panelHideY = -200f;
    public float tweenDuration = 0.4f;

    [Header("설정")]
    public float gridSize = 1.0f;
    public LayerMask groundLayer;
    public LayerMask buildingLayer;
    public Color validColor = new Color(0, 1, 0, 0.4f);
    public Color invalidColor = new Color(1, 0, 0, 0.4f);

    public BuildState currentState = BuildState.None;

    private FacilityData _currentData;
    private GameObject _previewInstance;
    private MeshRenderer[] _previewRenderers;
    private bool _canPlace;

    void Awake() => Instance = this;

    void Start()
    {
        if (buildPanel != null)
        {
            buildPanel.anchoredPosition = new Vector2(buildPanel.anchoredPosition.x, panelHideY);
        }
        UpdateGuideText("");
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.bKey.wasPressedThisFrame) ToggleBuildPanel();
        if (keyboard.xKey.wasPressedThisFrame) StartRemoveMode();
        if (keyboard.escapeKey.wasPressedThisFrame) CancelMode();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (currentState == BuildState.Placing) UpdatePlacingLogic(keyboard);
        else if (currentState == BuildState.Removing) UpdateRemovingLogic(keyboard);
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

        if (keyboard.rKey.wasPressedThisFrame) _previewInstance.transform.Rotate(0, 90, 0);

        if (keyboard.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_canPlace) PlaceObject();
            else Debug.LogWarning("Cannot place here!");
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

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float x = Mathf.Round(hit.point.x / gridSize) * gridSize;
            float z = Mathf.Round(hit.point.z / gridSize) * gridSize;

            if (_previewInstance != null)
            {
                if (!_previewInstance.activeSelf) _previewInstance.SetActive(true);
                _previewInstance.transform.position = new Vector3(x, 0.1f, z);
                
                CheckPlacementValidity();
            }
        }
        else if (_previewInstance != null)
        {
            _previewInstance.SetActive(false);
        }
    }

    void CheckPlacementValidity()
    {
        // 설치 가능 여부 체크 (0.48f 반경으로 겹침 확인)
        _canPlace = !Physics.CheckBox(_previewInstance.transform.position, new Vector3(0.48f, 0.48f, 0.48f), _previewInstance.transform.rotation, buildingLayer);
        
        Color targetColor = _canPlace ? validColor : invalidColor;

        if (_previewRenderers != null)
        {
            foreach (var rend in _previewRenderers)
            {
                foreach (var mat in rend.materials) mat.color = targetColor;
            }
        }
    }

    public void StartBuildMode(FacilityData data)
    {
        if (data == null || data.prefab == null) return;

        CancelMode();
        _currentData = data;
        currentState = BuildState.Placing;

        _previewInstance = Instantiate(data.prefab);
        _previewRenderers = _previewInstance.GetComponentsInChildren<MeshRenderer>();
        ApplyGhostEffect(_previewInstance);
        
        UpdateGuideText("<b>[ BUILD MODE ]</b>\n[Space/LMB] Place\n[R] Rotate \n [Esc] Cancel");
    }

    public void StartRemoveMode()
    {
        CancelMode();
        currentState = BuildState.Removing;
        UpdateGuideText("<b><color=red>[ REMOVE MODE ]</color></b>\n[Space/LMB] Remove\n[Esc] Cancel");
    }

    void PlaceObject()
    {
        if (MoneyManager.Instance.TrySpendMoney(_currentData.price))
        {
            GameObject newObj = Instantiate(_currentData.prefab, _previewInstance.transform.position, _previewInstance.transform.rotation);
            var facility = newObj.GetComponent<BaseFacility>();
            if (facility != null) facility.facilityData = _currentData;
        }
    }

    void RemoveObject(GameObject target) => Destroy(target);

    public void CancelMode()
    {
        currentState = BuildState.None;
        if (_previewInstance != null) Destroy(_previewInstance);
        _currentData = null;
        UpdateGuideText("");
    }

    void UpdateGuideText(string text)
    {
        if (guideText != null) guideText.text = text;
    }

    void ApplyGhostEffect(GameObject obj)
    {
        foreach (var col in obj.GetComponentsInChildren<Collider>()) col.enabled = false;
        if (_previewRenderers == null) return;

        foreach (var rend in _previewRenderers)
        {
            foreach (var mat in rend.materials)
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
    }
}
