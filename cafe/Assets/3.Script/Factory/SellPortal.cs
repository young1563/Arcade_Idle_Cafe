using UnityEngine;
using DG.Tweening;

public class SellPortal : MonoBehaviour
{
    [Header("경제 설정")]
    public int pricePerItem = 10;

    [Header("포탈 연출")]
    public Transform portalCenter;    // 파티클의 중심점 (디저트가 사라질 지점)
    public ParticleSystem portalEffect; // 상시 돌아가는 포탈 파티클
    public ParticleSystem sellEffect;   // 아이템이 판매될 때 터지는 추가 효과 (선택)

    private void OnTriggerEnter(Collider other)
    {
        // "FactoryItem" 태그를 사용 중이라면 그대로 유지, 혹은 "Dessert"로 변경
        if (other.CompareTag("FactoryItem"))
        {
            SellItem(other.gameObject);
        }
    }

    void SellItem(GameObject item)
    {
        // 1. 돈 지급
        MoneyManager.Instance.AddMoney(pricePerItem);

        // 2. 물리 및 충돌 비활성화
        if (item.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        item.GetComponent<Collider>().enabled = false;

        // 3. 포탈 중심으로 흡수되는 연출 (DOTween)
        Vector3 targetPos = portalCenter != null ? portalCenter.position : transform.position;

        // 아이템을 파티클 안쪽으로 살짝 이동시키며 크기를 줄임
        item.transform.DOMove(targetPos, 0.4f).SetEase(Ease.InBack);
        item.transform.DOScale(Vector3.zero, 4f).OnComplete(() => {

            // 4. 아이템이 완전히 사라지는 순간 "짠" 하는 파티클 (선택 사항)
            if (sellEffect != null) sellEffect.Play();

            Destroy(item);
        });
    }
}