using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;
    private MachineData selectedData;
    private GameObject previewObj;
    public LayerMask groundLayer;

    void Awake() => Instance = this;

    void Update()
    {
        if (selectedData == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            // 그리드 스냅 (1단위 정수 좌표)
            Vector3 gridPos = new Vector3(Mathf.Round(hit.point.x), 0, Mathf.Round(hit.point.z));
            if (previewObj != null) previewObj.transform.position = gridPos;

            // 회전 (R키)
            if (Input.GetKeyDown(KeyCode.R))
                previewObj.transform.Rotate(0, 90, 0);

            // 좌클릭 시 배치
            if (Input.GetMouseButtonDown(0))
                TryPlace(gridPos);

            // 우클릭 시 취소
            if (Input.GetMouseButtonDown(1))
                CancelPlacement();
        }
    }

    public void StartPlacement(string id)
    {
        // 인벤토리 수량 확인
        if (!FactoryInventory.Instance.inventoryItems.ContainsKey(id) || FactoryInventory.Instance.inventoryItems[id] <= 0)
        {
            Debug.Log("설비 수량이 부족합니다.");
            return;
        }

        selectedData = FactoryDataManager.Instance.machineTable[id];

        if (previewObj != null) Destroy(previewObj);
        GameObject prefab = Resources.Load<GameObject>(selectedData.prefabPath);
        previewObj = Instantiate(prefab);

        // 프리팹 투명화 처리를 위해 반투명 머티리얼 적용 권장
    }

    void TryPlace(Vector3 pos)
    {
        // 중복 배치 체크 (Physics.CheckSphere 등 활용 가능)
        if (Physics.CheckBox(pos, new Vector3(0.4f, 0.4f, 0.4f)))
        {
            Debug.Log("이미 다른 설비가 있습니다.");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(selectedData.prefabPath);
        Instantiate(prefab, pos, previewObj.transform.rotation);

        // 수량 차감
        FactoryInventory.Instance.inventoryItems[selectedData.id]--;

        // 수량이 다 떨어지면 모드 종료
        if (FactoryInventory.Instance.inventoryItems[selectedData.id] <= 0)
            CancelPlacement();
    }

    void CancelPlacement()
    {
        selectedData = null;
        if (previewObj != null) Destroy(previewObj);
    }
}