using UnityEngine;
using TMPro; // 가격 표시용
using UnityEngine.UI; // 게이지 표시용
using DG.Tweening;

public class UnlockZone : MonoBehaviour
{
    [Header("설정")]
    public FurnitureDataHolder targetFurniture; // 이 구역이 해금할 가구
    public float unlockRange = 2.0f; // 감지 범위
    public LayerMask playerLayer;

    [Header("UI 연결")]
    public TextMeshProUGUI priceText;
    public Image progressFill;
    public GameObject canvasObj;

    [Header("애니메이션 설정 (DOTween)")]
    // AnimationCurve는 삭제하고, DOTween의 Ease 타입을 사용합니다.
    [Tooltip("애니메이션 재생 시간")]
    public float animationDuration = 0.6f;
    [Tooltip("탄성 효과 종류 (OutBack 추천)")]
    public Ease bounceEaseType = Ease.OutBack; // '통!' 튀는 효과의 핵심

    private float currentPaid = 0;
    private float totalPrice;
    private bool isUnlocked = false;

    void Start()
    {
        if (targetFurniture == null) return;

        totalPrice = targetFurniture.data.price;
        UpdateUI();

        // 이미 해금된 상태라면 이 구역은 파괴
        if (targetFurniture.data.isUnlocked)
        {
            targetFurniture.gameObject.SetActive(true); // 가구는 보이고
            gameObject.SetActive(false); // 언락존은 파괴(비활성화)
        }
        // 2. 아직 해금되지 않은 상태라면 (추가된 부분)
        else
        {
            targetFurniture.gameObject.SetActive(false); // 가구를 숨깁니다.
        }
    }

    void Update()
    {
        if (isUnlocked) return;

        // 플레이어 감지 (단순 거리 체크 또는 Collider)
        Collider[] hit = Physics.OverlapSphere(transform.position, unlockRange, playerLayer);
        if (hit.Length > 0)
        {
            TryUnlock();
        }
    }

    // 안전장치: 오브젝트가 꺼지거나 파괴될 때 실행 중인 트윈을 정리합니다.
    void OnDisable()
    {
        // 혹시 애니메이션 중에 언락존이 꺼지더라도 타겟의 트윈을 멈춰서 에러 방지
        if (targetFurniture != null)
        {
            targetFurniture.transform.DOKill();
        }
    }

    void TryUnlock()
    {
        if (MoneyManager.Instance.currentMoney <= 0) return;

        // 초당 50원이 빠져나가게 설정 (예시)
        float paySpeed = 50f;
        int amountToPay = Mathf.Max(1, Mathf.FloorToInt(paySpeed * Time.deltaTime));

        // 실제 결제 시도
        if (MoneyManager.Instance.TrySpendMoney(amountToPay))
        {
            currentPaid += amountToPay;
            UpdateUI();

            if (currentPaid >= totalPrice)
            {
                DoUnlock();
            }
        }
    }

    void DoUnlock()
    {
        if (isUnlocked) return; // 중복 실행 방지

        isUnlocked = true;
        targetFurniture.data.isUnlocked = true;

        // DOTween 애니메이션 시작 함수 호출
        AnimateSpawnDOTween();        
    }
    // 코루틴 대신 DOTween을 사용한 애니메이션 함수
    void AnimateSpawnDOTween()
    {
        Transform targetTransform = targetFurniture.transform;

        // 1. 가구를 활성화하되, 크기를 0으로 시작해서 안 보이게 함
        targetFurniture.gameObject.SetActive(true);
        targetTransform.localScale = Vector3.zero;

        // 2. 목표 스케일 (원래 저장된 크기) 가져오기
        Vector3 finalScale = targetFurniture.data.scale.ToVector3();

        // 3. DOTween 스케일 애니메이션 실행
        // "0에서 finalScale까지 animationDuration 동안 커져라"
        targetTransform.DOScale(finalScale, animationDuration)
            .SetEase(bounceEaseType) // <-- 여기가 '통!' 튀는 마법의 한 줄!
            .OnComplete(OnUnlockAnimationComplete); // 애니메이션 끝나면 이 함수 실행

        // (선택사항) 효과음 재생 위치
        // if (spawnSound != null) AudioSource.PlayClipAtPoint(spawnSound, transform.position);
    }

    // 애니메이션이 끝났을 때 호출될 콜백 함수
    void OnUnlockAnimationComplete()
    {
        Debug.Log($"{targetFurniture.data.prefabName} 해금 애니메이션 완료 (DOTween)!");

        // 다음 구역 활성화 요청 (UnlockManager가 있다면)
        if (UnlockManager.Instance != null)
        {
            UnlockManager.Instance.RefreshUnlockZones();
        }

        // 임무를 다한 언락존 비활성화
        gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        if (priceText) priceText.text = $"{(int)(totalPrice - currentPaid)}G";
        if (progressFill) progressFill.fillAmount = currentPaid / totalPrice;
    }

    // 에디터에서 범위를 보기 위함
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, unlockRange);
    }
}