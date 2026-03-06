using UnityEngine;
using DG.Tweening;

public class SortingItem : MonoBehaviour
{
    public DessertType dessertType;
    

    private Vector3 _originalPosition;
    private bool _isUp;
    private Tween _floatTween;
    private Vector3 _baseScale = Vector3.one;

    private void Awake()
    {
        // 물리 충돌로 인해 아이템이 튕겨나가는 것을 방지
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true; // 레이캐스트 감지는 되면서 물리적 충돌은 무시
        }
    }

    private void Start()
    {
        // 1. 유기적인 배치를 위해 랜덤 회전 추가
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
    }

    public void InitializeScale(float targetUnitSize)
    {
        // 현재 스케일을 1로 리셋하고 실제 렌더러 크기 측정
        transform.localScale = Vector3.one;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            Bounds combinedBounds = renderers[0].bounds;
            foreach (var r in renderers) combinedBounds.Encapsulate(r.bounds);

            float maxDim = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
            if (maxDim > 0.01f)
            {
                float factor = targetUnitSize / maxDim;
                _baseScale = Vector3.one * factor;
            }
        }
        
        transform.localScale = _baseScale;
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
        seq.Append(transform.DOScaleY(_baseScale.y * 0.7f, 0.05f));
        seq.Append(transform.DOScaleY(_baseScale.y * 1.1f, 0.1f));
        seq.Append(transform.DOScaleY(_baseScale.y * 1.0f, 0.1f));
        
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
            // 3. 선택 시 부유 연출 (정규화된 스케일 기준)
            transform.DOMoveY(_originalPosition.y + 1.2f, 0.3f).SetEase(Ease.OutBack);
            transform.DOScale(_baseScale * 1.2f, 0.3f);
            
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
            transform.DOScale(_baseScale, 0.2f);
            transform.DOMove(_originalPosition, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    public void Celebrate()
    {
        // 4. 완료 시 제자리 회전 연출
        transform.DOJump(transform.position, 1.5f, 1, 0.5f);
        transform.DORotate(new Vector3(0, 360f, 0), 0.5f, RotateMode.FastBeyond360);
    }

    public void Shake()
    {
        // 5. 잘못된 이동 시 흔들림 연출
        transform.DOShakePosition(0.4f, new Vector3(0.3f, 0, 0), 10, 90, false, true);
        
        // 시각적으로 빨간색 피드백 (Shader가 _Color 속성을 지원하는 경우만)
        var renderers = GetComponentsInChildren<Renderer>();
        foreach(var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                r.material.DOKill();
                r.material.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
            }
        }
    }
}
