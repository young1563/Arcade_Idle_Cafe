using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SellPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    public Transform portalCenter;    // 포탈의 중심점 (디저트가 사라질 지점)
    public float suckDuration = 0.5f; // 빨려 들어가는 시간
    public float sellInterval = 0.2f; // 판매 간격 (빠르게 연회)

    [Header("Pile Settings")]
    public Vector3 pileAreaSize = new Vector3(1f, 0.5f, 1f);
    public int maxPileCount = 20;

    [Header("Effects")]
    public ParticleSystem sellEffect; // 판매 시 터지는 이펙트

    private List<GameObject> _piledItems = new List<GameObject>();
    private bool _isSelling = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FactoryItem"))
        {
            if (_piledItems.Count < maxPileCount)
            {
                AddToPile(other.gameObject);
            }
            else
            {
                // 영역이 꽉 찼다면 즉시 판매 모드로 전환
                StartCoroutine(SuckIntoPortal(other.gameObject));
            }
        }
    }

    private void AddToPile(GameObject itemObj)
    {
        FactoryItem item = itemObj.GetComponent<FactoryItem>();
        if (item != null) item.SetTarget(null, 0);

        // 포탈 근처 대기 구역에 랜덤하게 배치
        Vector3 randomPos = transform.position + new Vector3(
            Random.Range(-pileAreaSize.x * 0.5f, pileAreaSize.x * 0.5f),
            0.2f, // 바닥에 살짝 붙여서
            Random.Range(-pileAreaSize.z * 0.5f, pileAreaSize.z * 0.5f)
        );

        itemObj.transform.position = randomPos;

        // 물리 적용 (서로 엉키는 연출)
        Rigidbody rb = itemObj.GetComponent<Rigidbody>();
        if (rb == null) rb = itemObj.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        _piledItems.Add(itemObj);

        if (!_isSelling) StartCoroutine(SellRoutine());
    }

    private IEnumerator SellRoutine()
    {
        _isSelling = true;
        while (_piledItems.Count > 0)
        {
            yield return new WaitForSeconds(sellInterval);
            if (_piledItems.Count > 0)
            {
                GameObject itemToSell = _piledItems[0];
                _piledItems.RemoveAt(0);
                if (itemToSell != null) StartCoroutine(SuckIntoPortal(itemToSell));
            }
        }
        _isSelling = false;
    }

    private IEnumerator SuckIntoPortal(GameObject itemObj)
    {
        // 판매 금액 지급
        int price = 100;
        if (MoneyManager.Instance != null) MoneyManager.Instance.AddMoney(price);

        // 물리 끄고 포탈로 빨려 들어가기
        if (itemObj.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        itemObj.GetComponent<Collider>().enabled = false;

        // 중심점으로 이동하며 크기 줄이기 + 회전 연출
        Vector3 targetPos = portalCenter != null ? portalCenter.position : transform.position;

        itemObj.transform.DOMove(targetPos, suckDuration).SetEase(Ease.InBack);
        itemObj.transform.DOScale(Vector3.zero, suckDuration).SetEase(Ease.InBack);
        itemObj.transform.DORotate(new Vector3(0, 360, 0), suckDuration, RotateMode.FastBeyond360);

        yield return new WaitForSeconds(suckDuration);

        if (sellEffect != null) sellEffect.Play();
        Destroy(itemObj);
    }
}