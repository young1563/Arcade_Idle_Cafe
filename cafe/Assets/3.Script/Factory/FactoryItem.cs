using UnityEngine;

public class FactoryItem : MonoBehaviour
{
    public string itemId; // CSV의 ID와 매칭
    private Transform _targetTransform;
    private float _speed = 1.0f;

    // 다음 목적지(벨트의 끝점)를 설정
    public void SetTarget(Transform target, float speed)
    {
        _targetTransform = target;
        _speed = speed;
    }

    void Update()
    {
        if (_targetTransform == null) return;

        // 타겟 방향으로 이동
        transform.position = Vector3.MoveTowards(transform.position, _targetTransform.position, _speed * Time.deltaTime);

        // 도착하면 타겟 해제 (벨트가 다음 타겟을 지정해줄 것임)
        if (Vector3.Distance(transform.position, _targetTransform.position) < 0.01f)
        {
            _targetTransform = null;
        }
    }
}