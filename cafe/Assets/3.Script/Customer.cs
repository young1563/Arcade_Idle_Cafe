using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Customer : MonoBehaviour
{
    public NavMeshAgent agent;
    private Transform _exitPoint;
    private bool _isSatisfied = false;

    public void Init(Transform targetPos, Transform exitPoint)
    {
        _exitPoint = exitPoint;
        MoveTo(targetPos.position);
    }

    public void MoveTo(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    public void GetItem(GameObject itemPrefab, Transform hand)
    {
        _isSatisfied = true;
        // 디저트를 손에 쥐는 연출
        GameObject item = Instantiate(itemPrefab, hand);
        item.transform.localPosition = Vector3.zero;

        // 잠시 후 퇴장
        Invoke("Leave", 1.0f);
    }

    void Leave()
    {
        agent.SetDestination(_exitPoint.position);
        Destroy(gameObject, 10f); // 나중에 씬에서 제거
    }
}