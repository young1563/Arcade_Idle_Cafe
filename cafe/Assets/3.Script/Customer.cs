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

        // 즉시 코루틴 실행 (Invoke 대신 코루틴이 더 제어하기 쉽습니다)
        StartCoroutine(ExitRoutine());
    }

    IEnumerator ExitRoutine()
    {
        // 1. 사전 설정
        if (TryGetComponent(out Collider col)) col.enabled = false;
        agent.avoidancePriority = 99;
        agent.speed *= 1.3f;

        yield return new WaitForSeconds(0.5f); // 물건 받고 잠시 대기

        // 2. 경유지(ExitStartPoint)로 이동
        if (_exitStartPoint != null)
        {
            Debug.Log($"{gameObject.name} : 경유지로 이동 시작 -> {_exitStartPoint.name}");
            agent.SetDestination(_exitStartPoint.position);

            // 목적지에 도착할 때까지 대기 (안전장치 포함)
            float timeout = 5f; // 5초 안에 도착 못하면 강제 다음 단계
            while (agent.pathPending || agent.remainingDistance > 0.5f)
            {
                timeout -= Time.deltaTime;
                if (timeout <= 0) break;
                yield return null;
            }
        }

        // 3. 최종 출구로 이동
        if (_exitPoint != null)
        {
            Debug.Log($"{gameObject.name} : 최종 출구로 이동 시작");
            agent.SetDestination(_exitPoint.position);
        }

        // 4. 객체 파괴
        Destroy(gameObject, 10f);
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
        
}