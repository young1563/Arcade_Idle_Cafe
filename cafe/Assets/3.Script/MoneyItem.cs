using UnityEngine;
using DG.Tweening;

public class MoneyItem : MonoBehaviour
{
    public void FlyToPlayer(Transform playerTransform, int amount)
    {
        // 1. 돈이 생성될 때 살짝 위로 튀어 오르는 연출 (무작위 방향)
        Vector3 jumpPos = transform.position + new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f));

        transform.DOJump(jumpPos, 2f, 1, 0.4f).OnComplete(() =>
        {
            // 2. 잠시 대기 후 플레이어에게 날아감
            transform.DOMove(playerTransform.position, 0.5f).SetEase(Ease.InBack).OnUpdate(() => {
                // 플레이어가 이동 중일 수 있으므로 목적지를 계속 갱신하는 것이 좋음
                // 더 정교하게 하려면 Update에서 Lerp를 사용
            }).OnComplete(() => {
                // 3. 도착 시 돈 추가 및 파괴
                // MoneyManager.Instance.AddMoney(amount);
                Debug.Log($"돈 {amount} 획득!");

                // 간단한 팝 효과음이나 파티클을 넣으면 더 좋습니다.
                Destroy(gameObject);
            });
        });

        // 돈이 회전하면서 날아가게 설정
        transform.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360).SetLoops(-1);
    }
}