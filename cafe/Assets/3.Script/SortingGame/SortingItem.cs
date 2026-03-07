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
        // 프리팹 내부의 모든 리지드바디 비활성화
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 모든 콜라이더를 Trigger로 변경하여 충돌 폭발 방지
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.isTrigger = true;
        }

        // 레이캐스트 감지 레이어(2: Ignore Raycast)로 설정하여 튜브 클릭을 방해하지 않음
        gameObject.layer = 2;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = 2;
        }
    }

    private void Start()
    {
        // 모든 디저트가 정면을 바라보도록 통일 (랜덤 회전 제거)
        transform.rotation = Quaternion.identity;
    }

    public void InitializeScale(float targetUnitSize)
    {
        // 1. 스케일을 1로 리셋하여 순수 월드 경계 측정 준비
        transform.localScale = Vector3.one;
        
        // 2. 프리팹 내부의 가시적인 렌더러들만 추출하여 정확한 Bounds 계산
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        bool foundValidBounds = false;
        Bounds combinedBounds = new Bounds();

        foreach (var r in renderers)
        {
            // 파티클, 빈 렌더러, 트레일 등 시각적 크기와 무관한 것들은 제외
            if (r is ParticleSystemRenderer || r is TrailRenderer || !r.enabled) continue;
            
            if (!foundValidBounds)
            {
                combinedBounds = r.bounds;
                foundValidBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(r.bounds);
            }
        }

        if (foundValidBounds)
        {
            // 3. 평면 크기(X, Z) 중 큰 쪽을 기준으로 맞춤
            float currentSize = Mathf.Max(combinedBounds.size.x, combinedBounds.size.z);
            
            if (currentSize > 0.001f)
            {
                // 타겟 크기에 맞추기 위한 배율 계산
                float factor = targetUnitSize / currentSize;
                _baseScale = Vector3.one * factor;
            }
        }
        
        // 4. 최종 스케일 적용
        transform.localScale = _baseScale;
    }

    public void MoveTo(Vector3 targetPosition, System.Action onComplete = null)
    {
        _isUp = false;
        _floatTween?.Kill(); 
        transform.DOKill();
        
        Sequence seq = DOTween.Sequence();
        Vector3 peak = new Vector3((transform.position.x + targetPosition.x) / 2, 
                                   Mathf.Max(transform.position.y, targetPosition.y) + 2.5f, 
                                   (transform.position.z + targetPosition.z) / 2);
        
        // 이동 애니메이션
        seq.Append(transform.DOMove(peak, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(_baseScale, 0.3f)); // 이동하는 동안 원래 크기로 복구
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
            // 3. 선택 시 부유 연출 (높이를 확실히 2.5f로 상향)
            transform.DOMoveY(_originalPosition.y + 2.5f, 0.4f).SetEase(Ease.OutBack);
            transform.DOScale(_baseScale * 1.3f, 0.4f);
            
            // 계속 떠 있는 느낌의 루프
            _floatTween = transform.DOMoveY(_originalPosition.y + 2.7f, 0.6f)
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
