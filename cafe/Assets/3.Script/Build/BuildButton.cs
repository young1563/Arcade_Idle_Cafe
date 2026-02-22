using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildButton : MonoBehaviour
{
    public FacilityData data;         // 이 버튼이 지을 설비 데이터
    public TextMeshProUGUI priceText; // 가격 표시 UI

    void Start()
    {
        if (data != null && priceText != null) 
            priceText.text = $"{data.price}G";

        // 버튼 클릭 이벤트 연결
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (data == null) return;
        
        // BuildManager에게 이 데이터로 건설 모드 시작 요청
        BuildManager.Instance.StartBuildMode(data);
    }
}