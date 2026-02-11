using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Counter : MonoBehaviour
{
    [Header("Display Settings")]
    public Transform counterStackPoint;
    public int columns = 2;      // 한 줄에 들어가는 개수
    public int rows = 2;         // 줄의 개수 (두 줄)
    public float xSpacing = 0.4f;
    public float zSpacing = 0.4f;
    public float yOffset = 0.2f; // 층간 높이
    public int maxCounterCapacity = 10; // 2줄 * 5층 = 총 10개

    [Header("Logic Settings")]
    public float transferSpeed = 0.1f; // 아이템이 옮겨지는 간격
    public List<GameObject> counterItems = new List<GameObject>();

    private float _timer;

    void OnTriggerStay(Collider other)
    {
        // 플레이어가 카운터에 있고, 카운터에 공간이 있을 때
        if (other.CompareTag("Player") && counterItems.Count < maxCounterCapacity)
        {
            _timer += Time.deltaTime;
            if (_timer >= transferSpeed)
            {
                PlayerStack playerStack = other.GetComponent<PlayerStack>();
                if (playerStack != null && playerStack.stackedItems.Count > 0)
                {
                    // 플레이어 스택에서 하나 꺼내기
                    GameObject item = playerStack.RemoveStack();
                    if (item != null)
                    {
                        PlaceOnCounter(item);
                    }
                }
                _timer = 0;
            }
        }
    }

    void PlaceOnCounter(GameObject item)
    {
        int index = counterItems.Count;
        counterItems.Add(item);

        // 카운터 그리드 좌표 계산
        int layer = index / (columns * rows);
        int remaining = index % (columns * rows);
        int row = remaining / columns;
        int col = remaining % columns;

        Vector3 targetLocalPos = new Vector3(col * xSpacing, layer * yOffset, row * zSpacing);

        // 부모 설정 및 이동 연출
        item.transform.SetParent(counterStackPoint);
        item.transform.DOLocalJump(targetLocalPos, 2f, 1, 0.2f).OnComplete(() => {
            item.transform.localRotation = Quaternion.identity;
        });
    }

    // 손님이 가져갈 때 호출할 함수
    public GameObject GiveToCustomer()
    {
        if (counterItems.Count == 0) return null;

        int lastIndex = counterItems.Count - 1;
        GameObject item = counterItems[lastIndex];
        counterItems.RemoveAt(lastIndex);

        item.transform.SetParent(null);
        return item;
    }
}