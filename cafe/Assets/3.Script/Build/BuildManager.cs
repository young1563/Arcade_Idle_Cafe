using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("설치 설정")]
    public GameObject ghostPreview;   // 미리보기용 투명 프리팹
    public float gridSize = 1.0f;     // 그리드 한 칸의 크기
    public LayerMask groundLayer;     // 바닥 감지용 레이어

    private GameObject _currentBuildingPrefab; // 현재 설치하려는 건물
    private GameObject _previewInstance;

    void Awake() => Instance = this;

    void Update()
    {
        if (_previewInstance == null) return;

        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(0)) // 클릭 시 설치
        {
            PlaceObject();
        }

        if (Input.GetKeyDown(KeyCode.R)) // R키로 회전
        {
            _previewInstance.transform.Rotate(0, 90, 0);
        }
    }

    // 건설 모드 시작 (버튼 등을 통해 호출)
    public void StartBuildMode(GameObject prefab)
    {
        _currentBuildingPrefab = prefab;
        if (_previewInstance != null) Destroy(_previewInstance);

        _previewInstance = Instantiate(prefab);
        // 미리보기용이므로 충돌체나 스크립트는 끄고 반투명하게 처리하는 로직 필요
        ApplyGhostMaterial(_previewInstance);
    }

    void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // 그리드 스냅 로직: 좌표를 gridSize 단위로 반올림
            float x = Mathf.Round(hit.point.x / gridSize) * gridSize;
            float z = Mathf.Round(hit.point.z / gridSize) * gridSize;

            _previewInstance.transform.position = new Vector3(x, 0, z);
        }
    }

    void PlaceObject()
    {
        if (MoneyManager.Instance.TrySpendMoney(100)) // 설치 비용
        {
            Instantiate(_currentBuildingPrefab, _previewInstance.transform.position, _previewInstance.transform.rotation);
        }
    }

    void ApplyGhostMaterial(GameObject obj)
    {
        // 렌더러를 찾아 반투명하게 만드는 로직 (생략 가능, 단순 색상 변경 등)
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = new Color(0, 1, 0, 0.5f);
        }
    }
}