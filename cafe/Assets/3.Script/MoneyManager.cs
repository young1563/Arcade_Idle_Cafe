using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance; // 싱글톤 인스턴스

    [Header("Player Data")]
    public int currentMoney = 5000; // 초기 자금 (테스트용)

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 돈을 사용할 수 있는지 확인하고 차감하는 함수
    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            // TODO: 여기서 UIManager.Instance.UpdateGold(currentMoney)를 호출하여 UI 갱신
            return true;
        }
        return false;
    }

    // 돈을 추가하는 함수 (나중에 수익 발생 시 사용)
    public void AddMoney(int amount)
    {
        currentMoney += amount;
    }
}