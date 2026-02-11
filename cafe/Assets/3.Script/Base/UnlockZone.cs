using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UnlockZone : MonoBehaviour
{
    [Header("핵심 설정")]
    public FurnitureDataHolder targetFurniture; // 이 구역이 해금할 가구 (직접 드래그)
    public float unlockRange = 2.0f;           // 플레이어 감지 범위
    public LayerMask playerLayer;             // 플레이어 레이어 (Player로 설정 권장)

    [Header("충전(Hold) 설정")]
    [Tooltip("결제를 시작하기 위해 구역 내에서 머물러야 하는 시간")]
    public float requiredHoldTime = 1.0f;
    private float _stayTimer = 0f;            // 머문 시간 측정용

    [Header("UI 연결")]
    public TextMeshProUGUI priceText;         // 남은 가격 텍스트
    public Image progressFill;                // 결제 진행 게이지 (Image Type: Filled)
    public GameObject canvasObj;              // UI 캔버스 오브젝트

    [Header("애니메이션 설정 (DOTween)")]
    public float animationDuration = 0.6f;    // 가구 생성 애니메이션 시간
    public Ease bounceEaseType = Ease.OutBack; // 생성 시 튕기는 효과

    private float _currentPaid = 0;           // 현재 지불된 금액
    private float _totalPrice;                // 가구의 총 가격
    private bool _isUnlocked = false;         // 중복 해금 방지 플래그

    void Start()
    {
        if (targetFurniture == null)
        {
            Debug.LogError($"{gameObject.name}에 Target Furniture가 연결되지 않았습니다!");
            return;
        }

        // 가구 데이터에서 총 가격 가져오기
        _totalPrice = targetFurniture.data.price;
        UpdateUI();

        // [중요] 이미 해금된 가구라면 언락존은 즉시 사라짐
        if (targetFurniture.data.isUnlocked)
        {
            targetFurniture.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            // 아직 해금 전이라면 가구는 숨겨진 상태로 시작
            targetFurniture.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_isUnlocked) return;

        // 플레이어 감지
        Collider[] hit = Physics.OverlapSphere(transform.position, unlockRange, playerLayer);

        if (hit.Length > 0)
        {
            _stayTimer += Time.deltaTime;

            // 설정한 Hold 시간을 넘었을 때만 결제 진행
            if (_stayTimer >= requiredHoldTime)
            {
                TryUnlock();
            }

            // (선택) 1초 대기 중에도 게이지가 차오르는 연출을 원한다면 아래 로직 활용
            // if (_currentPaid <= 0) progressFill.fillAmount = _stayTimer / requiredHoldTime;
        }
        else
        {
            // 구역을 벗어나면 대기 시간 초기화
            _stayTimer = 0f;
            if (_currentPaid <= 0) progressFill.fillAmount = 0;
        }
    }

    void OnDisable()
    {
        // 오브젝트가 꺼질 때 실행 중인 트윈 중단 (메모리 누수 방지)
        if (targetFurniture != null)
        {
            targetFurniture.transform.DOKill();
        }
    }

    void TryUnlock()
    {
        // 플레이어의 현재 돈이 0 이하이거나 가구가 이미 해금 중이면 리턴
        if (MoneyManager.Instance.currentMoney <= 0) return;

        // 초당 100원 속도로 차감 (조절 가능)
        float paySpeed = 100f;
        int amountToPay = Mathf.Max(1, Mathf.FloorToInt(paySpeed * Time.deltaTime));

        // 돈 지불 시도
        if (MoneyManager.Instance.TrySpendMoney(amountToPay))
        {
            _currentPaid += amountToPay;
            UpdateUI();

            // 가격을 모두 지불했다면 해금 실행
            if (_currentPaid >= _totalPrice)
            {
                DoUnlock();
            }
        }
    }

    void DoUnlock()
    {
        if (_isUnlocked) return;
        _isUnlocked = true;

        // 데이터 상태 변경 (저장용)
        targetFurniture.data.isUnlocked = true;

        // 가구 등장 애니메이션 실행
        AnimateSpawnDOTween();
    }

    void AnimateSpawnDOTween()
    {
        Transform targetTransform = targetFurniture.transform;

        // 0에서 원래 크기로 커지는 애니메이션
        targetFurniture.gameObject.SetActive(true);
        Vector3 originalScale = targetTransform.localScale; // 현재 씬에 설정된 스케일 저장
        targetTransform.localScale = Vector3.zero;

        targetTransform.DOScale(originalScale, animationDuration)
            .SetEase(bounceEaseType)
            .OnComplete(OnUnlockAnimationComplete);
    }

    void OnUnlockAnimationComplete()
    {
        // 다음 해금 목표를 갱신하도록 매니저에 알림
        if (UnlockManager.Instance != null)
        {
            UnlockManager.Instance.RefreshUnlockZones();
        }

        // 임무 완료 후 언락존 비활성화
        gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        if (priceText)
        {
            float remaining = Mathf.Max(0, _totalPrice - _currentPaid);
            priceText.text = $"{Mathf.CeilToInt(remaining)}G";
        }

        if (progressFill)
        {
            progressFill.fillAmount = _currentPaid / _totalPrice;
        }
    }

    // 씬 뷰에서 범위를 시각적으로 확인하기 위함
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, unlockRange);
    }
}