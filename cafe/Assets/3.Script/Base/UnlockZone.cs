using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UnlockZone : MonoBehaviour
{
    [Header("연결 설정")]
    public FurnitureDataHolder targetFurniture; // 이 구역이 해금할 가구
    public float unlockRange = 2.0f;
    public LayerMask playerLayer;

    [Header("UI 연결")]
    public TextMeshProUGUI priceText;
    public Image progressFill;

    private float _currentPaid = 0;
    private float _totalPrice;
    private bool _isUnlocked = false;

    void Start()
    {
        if (targetFurniture == null) return;

        _totalPrice = targetFurniture.data.price;
        UpdateUI();

        // 이미 해금된 상태라면 가구는 켜고 언락존은 끈다
        if (targetFurniture.data.isUnlocked)
        {
            targetFurniture.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 플레이어가 범위 안에 있으면 결제 진행
        if (Physics.CheckSphere(transform.position, unlockRange, playerLayer))
        {
            TryPay();
        }
    }

    void TryPay()
    {
        if (MoneyManager.Instance.currentMoney <= 0) return;

        // 초당 지불 속도 (프레임 단위 계산)
        int amount = Mathf.Max(1, Mathf.FloorToInt(150f * Time.deltaTime));

        if (MoneyManager.Instance.TrySpendMoney(amount))
        {
            _currentPaid += amount;
            UpdateUI();

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

        // 1. 데이터 상태 변경 및 가구 활성화
        targetFurniture.data.isUnlocked = true;
        targetFurniture.gameObject.SetActive(true);

        // 2. 가구 등장 애니메이션 (살짝 커지는 효과)
        targetFurniture.transform.localScale = Vector3.zero;
        targetFurniture.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        // 3. 매니저에게 다음 해금 요소 활성화를 요청
        UnlockManager.Instance.RefreshUnlockZones();

        // 4. 언락존 제거
        gameObject.SetActive(false);

    }

    void UpdateUI()
    {
        if (priceText) priceText.text = $"{Mathf.Max(0, _totalPrice - _currentPaid)}G";
        if (progressFill) progressFill.fillAmount = _currentPaid / _totalPrice;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, unlockRange);
    }
}