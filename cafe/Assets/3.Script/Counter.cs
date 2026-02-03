using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Counter : MonoBehaviour
{
    public Transform sellPoint; // 아이템이 날아와서 사라질 지점
    public float sellInterval = 0.1f; // 판매 속도
    public int pricePerItem = 10;

    private bool _isSelling = false;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !_isSelling)
        {
            PlayerStack stack = other.GetComponent<PlayerStack>();
            if (stack != null && stack.stackedItems.Count > 0)
            {
                StartCoroutine(SellRoutine(stack));
            }
        }
    }

    IEnumerator SellRoutine(PlayerStack stack)
    {
        _isSelling = true;

        GameObject item = stack.RemoveStack();
        if (item != null)
        {
            // 판매 연출: 판매대로 날아감
            item.transform.SetParent(null);
            item.transform.DOJump(sellPoint.position, 2f, 1, 0.2f).OnComplete(() => {
                Destroy(item);
                // MoneyManager.Instance.AddMoney(pricePerItem); // 돈 매니저 연결 시 사용
                Debug.Log($"아이템 판매! +{pricePerItem}");
            });
        }

        yield return new WaitForSeconds(sellInterval);
        _isSelling = false;
    }
}