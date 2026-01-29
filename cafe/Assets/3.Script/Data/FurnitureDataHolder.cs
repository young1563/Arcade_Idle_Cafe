using UnityEngine;

public class FurnitureDataHolder : MonoBehaviour
{
    [Header("Furniture Data")]
    // [SerializeField]를 사용하여 인스펙터에서 수정 가능하게 합니다.
    public FurnitureEntity data;

    // 씬에서 수동으로 배치하거나 복사했을 때를 대비한 안전장치
    private void OnValidate()
    {
        // 하이어라키의 이름을 ID와 일치시켜 관리를 편하게 합니다.
        if (data != null && !string.IsNullOrEmpty(gameObject.name))
        {
            data.id = gameObject.name;
        }
    }

    // 현재 오브젝트의 물리적 수치(위치, 회전, 크기)를 데이터 객체에 동기화합니다.
    public void SyncTransformToData()
    {
        if (data == null) return;

        data.position = new Vector3Data { x = transform.position.x, y = transform.position.y, z = transform.position.z };
        data.scale = new Vector3Data { x = transform.localScale.x, y = transform.localScale.y, z = transform.localScale.z };
        data.rotation = transform.eulerAngles.y;
    }
}