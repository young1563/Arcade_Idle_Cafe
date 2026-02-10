using UnityEngine;

[ExecuteInEditMode] // 에디터 모드에서도 이름 동기화가 작동하도록 설정
public class FurnitureDataHolder : MonoBehaviour
{
    [Header("가구 정보 설정")]
    public FurnitureEntity data;

    private void OnValidate()
    {
        // 1. 데이터 객체가 비어있으면 새로 생성
        if (data == null) data = new FurnitureEntity();

        // 2. 하이어라키 상의 이름을 데이터 ID와 자동으로 일치시킴
        // 이렇게 하면 나중에 특정 가구를 ID로 찾기 매우 편리합니다.
        data.id = gameObject.name;
    }

    private void Awake()
    {
        // 게임 시작 시, 이미 해금된 가구가 아니라면 꺼둡니다.
        // 배경 소품이나 기본 가구는 인스펙터에서 isUnlocked를 체크해두면 됩니다.
        if (Application.isPlaying)
        {
            gameObject.SetActive(data.isUnlocked);
        }
    }
}