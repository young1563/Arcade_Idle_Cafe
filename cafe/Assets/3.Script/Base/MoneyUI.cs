using UnityEngine;
using TMPro; // TextMeshPro 사용
using DG.Tweening; // 숫자 올라가는 애니메이션용

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    private int _lastDisplayedMoney = -1;

    void Update()
    {
        // MoneyManager의 현재 돈을 가져옴
        int currentMoney = MoneyManager.Instance.currentMoney;

        // 값이 변했을 때만 텍스트 갱신
        if (_lastDisplayedMoney != currentMoney)
        {
            UpdateMoneyText(currentMoney);
            _lastDisplayedMoney = currentMoney;
        }
    }

    void UpdateMoneyText(int targetMoney)
    {
        // 1. 단순 텍스트 변경
        // moneyText.text = string.Format("{0:#,###} G", targetMoney);

        // 2. DOTween을 사용한 숫자 카운팅 연출 (더 고급스러움)
        // 기존 숫자에서 목표 숫자까지 0.5초 동안 부드럽게 올라감
        int startValue = _lastDisplayedMoney < 0 ? 0 : _lastDisplayedMoney;

        DOTween.To(() => startValue, x => {
            moneyText.text = string.Format("{0:#,###} G", x);
        }, targetMoney, 0.5f).SetEase(Ease.OutQuad);

        // 돈이 늘어날 때 살짝 커졌다 작아지는 펀치 애니메이션
        moneyText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }
}