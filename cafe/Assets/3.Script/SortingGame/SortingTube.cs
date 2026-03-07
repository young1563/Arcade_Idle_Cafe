using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SortingTube : MonoBehaviour, IPointerDownHandler
{
    public int capacity = 4;
    public List<Transform> slotAnchors = new List<Transform>();
    private Stack<SortingItem> _items = new Stack<SortingItem>();

    private void Awake()
    {
        // 최상위 오브젝트와 모든 자식의 레이어를 Default(0)로 설정하여 클릭 보장
        gameObject.layer = 0;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = 0;
        }
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
                if (child.name.ToLower().Contains("slot") || child.name.ToLower().Contains("anchor"))
                {
                    slotAnchors.Add(child);
                }
            }
        }

        // 3. Y 좌표 기준으로 정렬 (바닥부터 위로)
        slotAnchors.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));
        
        // 4. 외곽 경계선 생성
        CreateBoundaryVisuals();

        // 5. 클릭 감지를 위한 콜라이더 업데이트
        UpdateCollider();

        // 6. 여전히 비어있다면 경고
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

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭 감지 즉시 로그 출력 (문제 해결 확인용)
        Debug.Log($"[SortingTube] {gameObject.name} clicked!");

        // 클릭 피드백: 살짝 눌리는 느낌
        transform.DOKill();
        transform.DOScale(transform.localScale * 0.95f, 0.1f).SetLoops(2, LoopType.Yoyo);

        if (SortingGameManager.Instance != null)
        {
            SortingGameManager.Instance.OnTubeClicked(this);
        }
    }

    private void UpdateCollider()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc == null) bc = gameObject.AddComponent<BoxCollider>();

        // 클릭 영역을 넉넉하게 설정 (더 키움)
        float w = 2.6f; 
        float bottomY = slotAnchors.Count > 0 ? slotAnchors[0].localPosition.y - 0.5f : -0.5f;
        float topY = slotAnchors.Count > 0 ? slotAnchors[capacity - 1].localPosition.y + 2.0f : capacity * 2.0f;
        float height = topY - bottomY;

        bc.size = new Vector3(w, height, 1.0f);
        bc.center = new Vector3(0, bottomY + (height / 2), 0);
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
        float spacing = SortingGameManager.Instance != null ? SortingGameManager.Instance.itemTargetSize : 1.2f;
        return transform.position + Vector3.up * (_items.Count * spacing + spacing * 0.5f);
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
