using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SortingTube : MonoBehaviour
{
    public int capacity = 4;
    public List<Transform> slotAnchors = new List<Transform>();
    private Stack<SortingItem> _items = new Stack<SortingItem>();

    private void Awake()
    {
        InitializeSlots();
    }

    public void InitializeSlots()
    {
        // 1. null 항목 제거
        slotAnchors.RemoveAll(item => item == null);

        // 2. 리스트가 비어있다면 자식들을 검색
        if (slotAnchors.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("slot"))
                {
                    slotAnchors.Add(child);
                }
            }
        }
        
        // 3. 외곽 경계선 생성
        CreateBoundaryVisuals();

        // 4. 클릭 감지를 위한 콜라이더 업데이트
        UpdateCollider();

        // 5. 여전히 비어있다면 경고
        if (slotAnchors.Count == 0)
        {
            Debug.LogError($"{gameObject.name}: No slot anchors found! Items will stack at pivot.");
        }
    }

    private void CreateBoundaryVisuals()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr == null) lr = gameObject.AddComponent<LineRenderer>();

        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.useWorldSpace = false;
        lr.positionCount = 4;
        lr.startColor = lr.endColor = new Color(1, 1, 1, 0.4f);
        
        // 기본 재질 설정 (없으면 분홍색으로 보일 수 있음)
        lr.material = new Material(Shader.Find("Sprites/Default"));

        // 슬롯 높이에 따른 U자형 경계선 좌표 설정
        float w = 0.7f; // 반폭
        float bottomY = slotAnchors.Count > 0 ? slotAnchors[0].localPosition.y - 0.5f : -0.5f;
        float topY = slotAnchors.Count > 0 ? slotAnchors[capacity - 1].localPosition.y + 0.8f : capacity * 1.0f;

        lr.SetPosition(0, new Vector3(-w, topY, 0));
        lr.SetPosition(1, new Vector3(-w, bottomY, 0));
        lr.SetPosition(2, new Vector3(w, bottomY, 0));
        lr.SetPosition(3, new Vector3(w, topY, 0));
    }

    private void UpdateCollider()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc == null) bc = gameObject.AddComponent<BoxCollider>();

        // 튜브 너비와 높이에 맞춰 콜라이더 크기 조정 (z축은 0.2로 얇게 설정)
        float w = 1.4f; // 클릭 가능 너비
        float bottomY = slotAnchors.Count > 0 ? slotAnchors[0].localPosition.y - 0.5f : -0.5f;
        float topY = slotAnchors.Count > 0 ? slotAnchors[capacity - 1].localPosition.y + 0.8f : capacity * 1.0f;
        float height = topY - bottomY;

        bc.size = new Vector3(w, height, 0.2f);
        bc.center = new Vector3(0, bottomY + (height / 2), 0);
        
        // Raycast 통과 방지를 위해 IsTrigger는 꺼둠 (GameManager에서 RaycastHit 사용)
        bc.isTrigger = false;
    }

    public bool IsFull => _items.Count >= capacity;
    public bool IsEmpty => _items.Count == 0;

    public SortingItem PeekItem() => _items.Count > 0 ? _items.Peek() : null;

    public bool CanPush(SortingItem item)
    {
        if (IsFull) return false;
        if (IsEmpty) return true;
        
        // Rules: Must match the top item type
        return PeekItem().dessertType == item.dessertType;
    }

    public void Push(SortingItem item)
    {
        _items.Push(item);
        item.transform.SetParent(transform);
        // Visual movement will be handled by GameManager or Item itself with DOTween
    }

    public SortingItem Pop()
    {
        if (IsEmpty) return null;
        return _items.Pop();
    }

    public bool IsComplete()
    {
        if (IsEmpty) return true;
        if (!IsFull) return false;

        DessertType firstType = PeekItem().dessertType;
        foreach (var item in _items)
        {
            if (item.dessertType != firstType) return false;
        }
        return true;
    }

    public Vector3 GetNextSlotPosition()
    {
        if (slotAnchors != null && slotAnchors.Count > _items.Count)
        {
            Transform slot = slotAnchors[_items.Count];
            if (slot != null) return slot.position;
        }
        
        // Fallback: 인스펙터 설정이 잘못되었을 때를 대비한 자동 위치 계산
        Debug.LogWarning($"{gameObject.name}: Slot anchor at index {_items.Count} is missing, using fallback position.");
        return transform.position + Vector3.up * (_items.Count * 1.0f + 0.5f);
    }
    
    public Vector3 GetTopItemPosition()
    {
        if (IsEmpty) return transform.position;
        return _items.Peek().transform.position;
    }

    public void Shake()
    {
        transform.DOShakePosition(0.3f, new Vector3(0.2f, 0, 0), 10, 90, false, true);
    }
}
