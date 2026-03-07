using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public float cursorSize = 64f; 
    public Vector2 hotspot = Vector2.zero;

    [Header("Effects")]
    public GameObject clickEffectPrefab;

    private Camera _mainCam;

    private void Awake()
    {
        Cursor.visible = false;
        _mainCam = Camera.main;
    }

    private void Update()
    {
        // 클릭 감지 (New Input System)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SpawnClickEffect();
        }
    }

    private void SpawnClickEffect()
    {
        if (clickEffectPrefab == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // 3D 공간에 이펙트를 배치하기 위해 월드 좌표로 변환
        // 카메라로부터의 거리는 10 정도로 설정
        Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
        
        GameObject effect = Instantiate(clickEffectPrefab, worldPos, Quaternion.identity);
        
        // 일정 시간 후 파티클 자동 삭제 (보통 FX에는 자동 삭제 로직이 있지만 안전하게 추가)
        Destroy(effect, 2f);
    }

    private void OnGUI()
    {
        if (cursorTexture != null && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            Rect cursorRect = new Rect(
                mousePos.x - hotspot.x, 
                Screen.height - mousePos.y - hotspot.y, 
                cursorSize, 
                cursorSize
            );

            GUI.depth = -1000;
            GUI.DrawTexture(cursorRect, cursorTexture);
        }
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }
}
