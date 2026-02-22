using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SellPortal : MonoBehaviour
{
    [Header("Pile Settings")]
    public Transform pileCenter;
    public Vector3 pileAreaSize = new Vector3(1.5f, 0.5f, 1.5f);
    public int maxPileCount = 20;
    public float sellInterval = 0.5f;

    [Header("Effects")]
    public ParticleSystem sellEffect;

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
                SellImmediate(other.gameObject);
            }
        }
    }

    private void AddToPile(GameObject itemObj)
    {
        FactoryItem item = itemObj.GetComponent<FactoryItem>();
        if (item != null) item.SetTarget(null, 0);

        Vector3 randomPos = pileCenter.position + new Vector3(
            Random.Range(-pileAreaSize.x * 0.5f, pileAreaSize.x * 0.5f),
            Random.Range(0, pileAreaSize.y),
            Random.Range(-pileAreaSize.z * 0.5f, pileAreaSize.z * 0.5f)
        );

        itemObj.transform.position = randomPos;
        itemObj.transform.rotation = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
        
        Rigidbody rb = itemObj.GetComponent<Rigidbody>();
        if (rb == null) rb = itemObj.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        _piledItems.Add(itemObj);

        if (!_isSelling)
        {
            StartCoroutine(SellRoutine());
        }
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
                if (itemToSell != null)
                {
                    _piledItems.RemoveAt(0);
                    SellImmediate(itemToSell);
                }
                else
                {
                    _piledItems.RemoveAt(0);
                }
            }
        }

        _isSelling = false;
    }

    private void SellImmediate(GameObject itemObj)
    {
        int price = 100;

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(price);
        }

        if (sellEffect != null)
        {
            sellEffect.Play();
        }

        itemObj.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => {
            if (itemObj != null) Destroy(itemObj);
        });
    }
}