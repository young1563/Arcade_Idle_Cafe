using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    public NavMeshAgent agent;
    private Transform _exitPoint;
    private Transform _exitStartPoint;

    private bool _isLeaving = false;
    private bool _movedToBypass = false;

    public void Init(Transform targetPos, Transform exitPoint, Transform exitStartPoint)
    {
        _exitPoint = exitPoint;
        _exitStartPoint = exitStartPoint;
        MoveTo(targetPos.position);
    }

    public void MoveTo(Vector3 pos)
    {
        if (agent.isOnNavMesh) agent.SetDestination(pos);
    }

    public void GetItem(GameObject itemPrefab, Transform hand)
    {
        Instantiate(itemPrefab, hand).transform.localPosition = Vector3.zero;

        // 0.5초 뒤에 퇴장 시작
        Invoke("StartExit", 0.5f);
    }

    void StartExit()
    {
        if (TryGetComponent(out Collider col)) col.enabled = false;
        agent.speed *= 1.3f;
        _isLeaving = true;

        // 첫 번째 목적지를 경유지로 설정
        MoveTo(_exitStartPoint.position);
    }

    void Update()
    {
        if (!_isLeaving) return;

        // 경유지에 거의 도착했는지 체크 (단순 거리 계산)
        if (!_movedToBypass)
        {
            float dist = Vector3.Distance(transform.position, _exitStartPoint.position);
            if (dist < 0.8f)
            {
                _movedToBypass = true;
                MoveTo(_exitPoint.position); // 최종 출구로 목적지 교체
                Destroy(gameObject, 10f);    // 넉넉히 뒤에 파괴
            }
        }
    }
}