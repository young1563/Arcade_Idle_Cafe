using UnityEngine;

[ExecuteInEditMode]
public class SortingGameSceneSetup : MonoBehaviour
{
    [Header("Settings")]
    public Sprite backgroundSprite;
    public float targetAspectRatio = 9f / 16f; // 1080x1920
    public float cameraSize = 8.5f;

    private GameObject _bgObject;

    private void Start()
    {
        SetupScene();
    }

    [ContextMenu("Setup Now")]
    public void SetupScene()
    {
        // 1. 카메라 설정
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = cameraSize;
            mainCam.transform.position = new Vector3(0, 5f, -10f); // 튜브들이 잘 보이게 약간 위로

            // 이벤트 시스템을 위한 물리 레이캐스터 추가
            if (mainCam.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>() == null)
                mainCam.gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
        }

        // 2. 이벤트 시스템 확인/생성
        UnityEngine.EventSystems.EventSystem es = UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            es = esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

        // 기존의 구형 StandaloneInputModule 제거 (새로운 Input System과 충돌)
        var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (oldModule != null)
        {
            if (Application.isPlaying) Object.Destroy(oldModule);
            else Object.DestroyImmediate(oldModule);
        }

        // 새로운 Input System용 모듈 추가
        if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
        {
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 3. 배경 오브젝트 생성/찾기
        if (_bgObject == null)
        {
            _bgObject = GameObject.Find("SortingGameBackground");
            if (_bgObject == null)
            {
                _bgObject = new GameObject("SortingGameBackground");
            }
        }

        SpriteRenderer sr = _bgObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = _bgObject.AddComponent<SpriteRenderer>();

        sr.sprite = backgroundSprite;
        sr.sortingOrder = -100; // 가장 뒤로
        _bgObject.transform.position = new Vector3(0, 5f, 10f); // 카메라보다 뒤, 아이템보다 뒤

        // 3. 배경 스케일 계산 (화면에 꽉 채우기)
        if (backgroundSprite != null)
        {
            float spriteWidth = backgroundSprite.bounds.size.x;
            float spriteHeight = backgroundSprite.bounds.size.y;

            float worldScreenHeight = mainCam.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight * mainCam.aspect;

            // 배경이 잘리지 않게 가로/세로 중 큰 쪽에 맞춤
            float scaleX = worldScreenWidth / spriteWidth;
            float scaleY = worldScreenHeight / spriteHeight;
            float finalScale = Mathf.Max(scaleX, scaleY);

            _bgObject.transform.localScale = new Vector3(finalScale, finalScale, 1f);
        }
        
        Debug.Log("Sorting Game Scene Setup Complete!");
    }
}
