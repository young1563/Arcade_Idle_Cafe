using UnityEngine;
using DG.Tweening;

public class SortingItem : MonoBehaviour
{
    public DessertType dessertType;
    

    private Vector3 _originalPosition;
    private bool _isUp;
    private Tween _floatTween;

    private void Start()
    {
        // 1. 유기적인 배치를 위해 랜덤 회전 추가
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
    }

    public void MoveTo(Vector3 targetPosition, System.Action onComplete = null)
    {
        _isUp = false;
        transform.DOKill();
        
        Sequence seq = DOTween.Sequence();
        Vector3 peak = new Vector3((transform.position.x + targetPosition.x) / 2, 
                                   Mathf.Max(transform.position.y, targetPosition.y) + 2.5f, 
                                   (transform.position.z + targetPosition.z) / 2);
        
        // 이동 애니메이션
        seq.Append(transform.DOMove(peak, 0.3f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMove(targetPosition, 0.25f).SetEase(Ease.InQuad));
        
        // 2. 착지 탄성 (Squash & Stretch)
        seq.Append(transform.DOScaleY(0.7f, 0.05f)); // 눌렸다가
        seq.Append(transform.DOScaleY(1.1f, 0.1f));  // 튀어오르고
        seq.Append(transform.DOScaleY(1.0f, 0.1f));  // 복구
        
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void Select(bool selected)
    {
        if (selected)
        {
            if (_isUp) return;
            _isUp = true;
            _originalPosition = transform.position;
            
            transform.DOKill();
            // 3. 선택 시 부유 연출
            transform.DOMoveY(_originalPosition.y + 1.2f, 0.3f).SetEase(Ease.OutBack);
            transform.DOScale(1.15f, 0.3f);
            
            // 계속 떠 있는 느낌의 루프
            _floatTween = transform.DOMoveY(_originalPosition.y + 1.4f, 0.6f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            if (!_isUp) return;
            _isUp = false;
            _floatTween?.Kill();

            transform.DOKill();
            transform.DOScale(1.0f, 0.2f);
            transform.DOMove(_originalPosition, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    public void Celebrate()
    {
        // 4. 완료 시 제자리 회전 연출
        transform.DOJump(transform.position, 1.5f, 1, 0.5f);
        transform.DORotate(new Vector3(0, 360f, 0), 0.5f, RotateMode.FastBeyond360);
    }
}
