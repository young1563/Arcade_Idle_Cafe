using UnityEngine;
using DG.Tweening;

public class SortingItem : MonoBehaviour
{
    public DessertType dessertType;
    
    public void MoveTo(Vector3 targetPosition, System.Action onComplete = null)
    {
        // Simple arc movement: Up -> Side -> Down
        Sequence seq = DOTween.Sequence();
        Vector3 peak = new Vector3((transform.position.x + targetPosition.x) / 2, 
                                   Mathf.Max(transform.position.y, targetPosition.y) + 2f, 
                                   (transform.position.z + targetPosition.z) / 2);
        
        seq.Append(transform.DOMove(peak, 0.25f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMove(targetPosition, 0.25f).SetEase(Ease.InQuad));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void Select(bool selected)
    {
        if (selected)
        {
            transform.DOScale(1.2f, 0.2f);
            transform.DOMoveY(transform.position.y + 0.5f, 0.2f);
        }
        else
        {
            transform.DOScale(1.0f, 0.2f);
            // Height will be reset by MoveTo or Manual return
        }
    }
}
