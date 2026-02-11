using UnityEngine;
using TMPro;
using DG.Tweening;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("UI Reference")]
    public TextMeshProUGUI moneyText;

    [Header("Settings")]
    [SerializeField] private int _currentMoney = 0;

    // 언락존에서 'currentMoney'로 접근하고 있으므로 프로퍼티 제공
    public int currentMoney => _currentMoney;

    void Awake()
    {
        if (Instance == null) Instance = this;
        UpdateUI();
    }

    /// <summary>
    /// 돈을 추가할 때 호출 (판매 완료 시)
    /// </summary>
    public void AddMoney(int amount)
    {
        _currentMoney += amount;
        UpdateUI();
        PlayMoneyEffect();
    }

    /// <summary>
    /// 언락존에서 결제를 시도할 때 호출
    /// </summary>
    /// <param name="amount">차감할 금액</param>
    /// <returns>차감 성공 여부</returns>
    public bool TrySpendMoney(int amount)
    {
        if (_currentMoney >= amount)
        {
            _currentMoney -= amount;
            UpdateUI();
            return true;
        }

        // 돈이 부족하면 실패 반환
        return false;
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = _currentMoney.ToString("N0");
    }

    private void PlayMoneyEffect()
    {
        if (moneyText == null) return;

        moneyText.transform.DOKill();
        moneyText.transform.localScale = Vector3.one;
        // 텍스트가 톡톡 튀는 효과
        moneyText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.15f);
    }
}