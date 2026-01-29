using UnityEngine;

// 이 컴포넌트는 MapLoader가 가구를 생성할 때 자동으로 붙이거나, 
// 에디터 도구에서 생성할 때 데이터 연결 고리로 사용됩니다.
public class FurnitureDataHolder : MonoBehaviour
{
    // JSON의 한 줄 정보(ID, 가격, 타입 등)를 이 변수에 담아둡니다.
    public FurnitureEntity data;
}