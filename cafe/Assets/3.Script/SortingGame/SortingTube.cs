using System.Collections.Generic;
using UnityEngine;

public class SortingTube : MonoBehaviour
{
    public int capacity = 4;
    private Stack<SortingItem> _items = new Stack<SortingItem>();
    public Transform[] slotAnchors; // Positions for items in the tube

    public int ItemCount => _items.Count;
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
        if (slotAnchors != null && slotAnchors.Length > _items.Count)
        {
            return slotAnchors[_items.Count].position;
        }
        // Fallback: simple vertical offset
        return transform.position + Vector3.up * (_items.Count * 0.8f);
    }
    
    public Vector3 GetTopItemPosition()
    {
        if (IsEmpty) return transform.position;
        return _items.Peek().transform.position;
    }
}
