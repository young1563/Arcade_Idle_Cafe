using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public Transform endpoint; // 벨트의 끝 지점 (프리팹 자식으로 생성 권장)
    public float moveSpeed = 1.5f;
    private FactoryItem _currentItem;

    // 입구에 아이템이 들어오면 호출
    private void OnTriggerEnter(Collider other)
    {
        // 1. 트리거 자체가 작동하는지 확인
        Debug.Log($"{other.name}이(가) 벨트에 닿았습니다.");

        if (other.CompareTag("FactoryItem"))
        {
            // 2. 태그 인식 확인
            Debug.Log("FactoryItem 태그 인식 성공!");

            _currentItem = other.GetComponent<FactoryItem>();
            if (_currentItem != null)
            {
                _currentItem.SetTarget(endpoint, moveSpeed);
                Debug.Log("아이템에 타겟 지점을 전달했습니다.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_currentItem != null && other.gameObject == _currentItem.gameObject)
        {
            _currentItem = null;
        }
    }

    // 레이캐스트를 이용해 다음 벨트/기계가 있는지 확인하는 기능 (확장용)
    public bool HasNextStructure()
    {
        RaycastHit hit;
        // 출구 방향으로 레이를 쏴서 다음 구조체 감지
        return Physics.Raycast(endpoint.position, transform.forward, out hit, 0.6f);
    }
}