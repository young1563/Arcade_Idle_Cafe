using UnityEngine;
using TMPro;
using DG.Tweening;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    private int _lastDisplayedMoney = -1;
    private Tween _punchTween; // 트윈 참조 저장

    void Update()
    {
        int currentMoney = MoneyManager.Instance.currentMoney;

        if (_lastDisplayedMoney != currentMoney)
        {
            // 돈이 줄어들 때(해금 중)는 애니메이션 없이 숫자만 갱신하고,
            // 돈이 늘어날 때(판매 등)만 펀치 효과를 주도록 분기 처리합니다.
            bool isIncreasing = currentMoney > _lastDisplayedMoney;

            UpdateMoneyText(currentMoney, isIncreasing);
            _lastDisplayedMoney = currentMoney;
        }
    }

    void UpdateMoneyText(int targetMoney, bool animate)
    {
        // 숫자 카운팅은 항상 부드럽게
        DOTween.To(() => _lastDisplayedMoney < 0 ? 0 : _lastDisplayedMoney,
            x => moneyText.text = string.Format("{0:#,###} G", x),
            targetMoney, 0.2f);

        if (animate)
        {
            // 이전 애니메이션이 돌고 있다면 강제 종료 후 초기화
            moneyText.transform.DOKill();
            moneyText.transform.localScale = Vector3.one;

            // 살짝 커지는 효과 실행
            moneyText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 5, 1);
        }
    }
}