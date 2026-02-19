using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildButton : MonoBehaviour
{
    public GameObject buildingPrefab; // 이 버튼이 지을 프리팹
    public int price = 100;           // 설치 가격
    public TextMeshProUGUI priceText; // 가격 표시 UI

    void Start()
    {
        if (priceText != null) priceText.text = $"{price}G";

        // 버튼 클릭 이벤트 연결
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        // BuildManager에게 이 프리팹으로 건설 모드 시작 요청
        BuildManager.Instance.StartBuildMode(buildingPrefab, price);
    }
}