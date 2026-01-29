using UnityEngine;
using TMPro; // 가격 표시용
using UnityEngine.UI; // 게이지 표시용

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
            gameObject.SetActive(false);
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

    void TryUnlock()
    {
        // 1. 플레이어 보유 자산 체크 (예시: MoneyManager.Instance.Money)
        // 실제 구현 시 MoneyManager와 연동이 필요합니다.
        int playerMoney = 1000; // 임시 데이터

        if (playerMoney > 0)
        {
            // 초당 결제 로직 (예시)
            float payAmount = totalPrice * Time.deltaTime;
            currentPaid += payAmount;

            UpdateUI();

            if (currentPaid >= totalPrice)
            {
                DoUnlock();
            }
        }
    }

    void DoUnlock()
    {
        isUnlocked = true;
        targetFurniture.data.isUnlocked = true;
        targetFurniture.gameObject.SetActive(true); // 가구 나타남

        // 효과음/파티클 재생 위치
        Debug.Log($"{targetFurniture.data.prefabName} 해금 완료!");

        // 다음 순서 가구 활성화를 Manager에 알림
        // UnlockManager.Instance.CheckNextUnlock();

        gameObject.SetActive(false); // 구역 사라짐
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