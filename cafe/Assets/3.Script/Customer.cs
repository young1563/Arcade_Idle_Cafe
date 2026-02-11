using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Customer : MonoBehaviour
{
    public NavMeshAgent agent;
    private Transform _exitPoint;
    private Transform _exitStartPoint; // 경유지 추가
    private bool _isSatisfied = false;

    // 매니저에서 초기화할 때 경유지(exitStartPoint)도 함께 받습니다.
    public void Init(Transform targetPos, Transform exitPoint, Transform exitStartPoint)
    {
        _exitPoint = exitPoint;
        _exitStartPoint = exitStartPoint;
        MoveTo(targetPos.position);
    }

    public void MoveTo(Vector3 pos)
    {
        // 에이전트가 활성화 상태일 때만 명령
        if (agent.gameObject.activeInHierarchy && agent.isOnNavMesh)
        {
            agent.SetDestination(pos);
        }
    }

    public void GetItem(GameObject itemPrefab, Transform hand)
    {
        _isSatisfied = true;
        GameObject item = Instantiate(itemPrefab, hand);
        item.transform.localPosition = Vector3.zero;

        Invoke("Leave", 0.5f); // 1초는 길 수 있어 0.5초로 조정
    }

    public void Leave()
    {
        // 1. 충돌 방지 및 우선순위 조정
        if (TryGetComponent(out Collider col)) col.enabled = false;
        agent.avoidancePriority = 99; // 다른 손님들이 이 손님을 피하지 않게 함
        agent.speed *= 1.2f;         // 퇴장 시 조금 더 빨리 걷기

        // 2. 지정된 경유지를 향해 코루틴 시작
        StartCoroutine(ExitRoutine());
    }

    IEnumerator ExitRoutine()
    {
        // [1단계] 설정된 경유지(exitStartPosition)로 이동
        if (_exitStartPoint != null)
        {
            agent.SetDestination(_exitStartPoint.position);

            // 경유지에 도착할 때까지 대기
            while (agent.pathPending || agent.remainingDistance > 0.5f)
            {
                yield return null;
            }
        }

        // [2단계] 최종 출구로 이동
        if (_exitPoint != null)
        {
            agent.SetDestination(_exitPoint.position);
        }

        // 일정 시간 후 제거 (충분히 나갈 시간 부여)
        Destroy(gameObject, 10f);
    }
}