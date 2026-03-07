using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DG.Tweening;

public class SortingGameManager : MonoBehaviour
{
    public static SortingGameManager Instance;

    [Header("Settings")]
    public string levelDataFileName = "levels.json";
    public GameObject tubePrefab;
    public GameObject[] dessertPrefabs; // Index matches DessertType enum
    public Transform tubeParent;
    public float tubeSpacing = 2.2f;   // 세로 화면에 맞춰 간격 축소
    public float itemTargetSize = 1.3f; 
    public float tubeScale = 0.85f;    

    [Header("Runtime")]
    public int currentLevel = 1;
    private List<SortingTube> _tubes = new List<SortingTube>();
    private SortingTube _selectedTube;
    public SortingTube SelectedTube => _selectedTube;
    private bool _isBusy;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // UI 레이아웃이 확정될 때까지 한 프레임 대기 후 레벨 로드
        StartCoroutine(LoadLevelAfterLayout());
    }

    private System.Collections.IEnumerator LoadLevelAfterLayout()
    {
        yield return null; // 한 프레임 대기
        LoadLevel(currentLevel);
    }

    private void Update()
    {
        if (_isBusy) return;
    }


    public void OnTubeClicked(SortingTube tube)
    {
        if (_isBusy) return;

        if (_selectedTube == null)
        {
            // Select
            if (!tube.IsEmpty)
            {
                _selectedTube = tube;
                _selectedTube.PeekItem().Select(true);
                Debug.Log($"[Select] {tube.name} selected.");
            }
            else
            {
                tube.Shake();
                Debug.Log("[Select] Clicked empty tube - nothing to select.");
            }
        }
        else
        {
            // Try Move
            if (_selectedTube == tube)
            {
                // Same tube: Deselect
                _selectedTube.PeekItem().Select(false);
                _selectedTube = null;
                Debug.Log("[Deselect] Same tube clicked.");
            }
            else if (tube.CanPush(_selectedTube.PeekItem()))
            {
                Debug.Log($"[Move] Valid move from {_selectedTube.name} to {tube.name}.");
                MoveItem(_selectedTube, tube);
                _selectedTube = null;
            }
            else
            {
                // Invalid move reason logging
                if (tube.IsFull) 
                    Debug.LogWarning($"[Move Invalid] {tube.name} is FULL.");
                else 
                    Debug.LogWarning($"[Move Invalid] Type mismatch ({_selectedTube.PeekItem().dessertType} vs {tube.PeekItem().dessertType}).");
                
                _selectedTube.PeekItem().Shake();
            }
        }
    }

    private void MoveItem(SortingTube from, SortingTube to)
    {
        _isBusy = true;
        SortingItem item = from.Pop();
        // item.Select(false); // MoveTo will handle the transition
        
        Vector3 targetPos = to.GetNextSlotPosition();
        item.MoveTo(targetPos, () => {
            to.Push(item);
            _isBusy = false;
            CheckWinCondition();
            
            // Hide tutorial on first move
            if (SortingHUDController.Instance != null) SortingHUDController.Instance.HideTutorial();
        });
    }

    public System.Action OnLevelComplete;

    private void CheckWinCondition()
    {
        foreach (var tube in _tubes)
        {
            if (!tube.IsComplete()) return;
        }
        
        Debug.Log("Level Complete!");
        OnLevelComplete?.Invoke();
    }

    private void NextLevel()
    {
        currentLevel++;
        LoadLevel(currentLevel);
    }

    [ContextMenu("Load Current Level")]
    public void LoadLevel(int level)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "levels.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            // Use DessertLevelList from SortingLevelData.cs instead of duplicated DessertGameData
            DessertLevelList gameData = JsonUtility.FromJson<DessertLevelList>(json);

            if (gameData != null && gameData.levels != null && level - 1 < gameData.levels.Count)
            {
                ClearLevel();
                SpawnTubes(gameData.levels[level - 1]);
                
                if (SortingHUDController.Instance != null)
                {
                    SortingHUDController.Instance.SetLevelText(level);
                    SortingHUDController.Instance.CheckTutorial();
                }
            }
        }
    }

    private void ClearLevel()
    {
        foreach (var tube in _tubes)
        {
            if (tube != null) Destroy(tube.gameObject);
        }
        _tubes.Clear();
        _selectedTube = null;
    }

    private void SpawnTubes(DessertLevelData data)
    {
        if (data == null || data.tubes == null)
        {
            Debug.LogError("DessertLevelData or tubes list is null!");
            return;
        }

        if (tubePrefab == null)
        {
            Debug.LogError("Tube Prefab is not assigned in SortingGameManager!");
            return;
        }

        // 1. UI 보드 영역 계산
        Vector3 boardWorldCenter = Vector3.zero;
        Vector2 boardWorldSize = new Vector2(5f, 8f); // 기본값 (폴백)

        if (SortingHUDController.Instance != null)
        {
            VisualElement board = SortingHUDController.Instance.GetGameBoard();
                // UI 좌표를 스크린 좌표로 변환
                Rect worldRect = board.worldBound;
                
                // 아직 레이아웃이 잡히지 않아 NaN인 경우 스킵 (기본값 사용)
                if (float.IsNaN(worldRect.x) || float.IsNaN(worldRect.width))
                {
                    Debug.LogWarning("[Board] UI Layout not ready, using defaults.");
                }
                else
                {
                    // 스크린 좌표 -> 월드 좌표 변환
                    Camera cam = Camera.main;
                    
                    // [정밀 보정] 단순히 center만 쓰지 않고 상/하단을 각각 계산하여 중간을 잡음
                    Vector3 screenTop = new Vector3(worldRect.center.x, Screen.height - worldRect.yMin, 10f);
                    Vector3 screenBottom = new Vector3(worldRect.center.x, Screen.height - worldRect.yMax, 10f);
                    
                    float worldTopY = cam.ScreenToWorldPoint(screenTop).y;
                    float worldBottomY = cam.ScreenToWorldPoint(screenBottom).y;
                    
                    boardWorldCenter.x = cam.ScreenToWorldPoint(new Vector3(worldRect.center.x, 0, 10f)).x;
                    boardWorldCenter.y = (worldTopY + worldBottomY) / 2f - 0.7f; // 하단으로 약간의 시각적 보정 추가
                    boardWorldCenter.z = 0;

                    Vector3 screenTopLeft = new Vector3(worldRect.xMin, Screen.height - worldRect.yMin, 10f);
                    Vector3 screenBottomRight = new Vector3(worldRect.xMax, Screen.height - worldRect.yMax, 10f);
                    
                    Vector3 worldTopLeft = cam.ScreenToWorldPoint(screenTopLeft);
                    Vector3 worldBottomRight = cam.ScreenToWorldPoint(screenBottomRight);
                    
                    boardWorldSize.x = Mathf.Abs(worldBottomRight.x - worldTopLeft.x);
                    boardWorldSize.y = Mathf.Abs(worldTopLeft.y - worldBottomRight.y);
                }
        }

        // 2. 3D 보드 비주얼 생성 (디저트 뒤에 배치)
        GameObject boardObj = GameObject.Find("3D_Board_Plate");
        if (boardObj == null) boardObj = new GameObject("3D_Board_Plate");
        
        // Z값을 5정도로 뒤로 뺍니다 (디저트는 Z=0)
        boardObj.transform.position = new Vector3(boardWorldCenter.x, boardWorldCenter.y, 5f);
        boardObj.transform.localScale = new Vector3(boardWorldSize.x, boardWorldSize.y, 1f);

        // 간단한 반투명 흰색 판 생성 (기존 Sprite가 없다면 흰색 기본 사용)
        SpriteRenderer sr = boardObj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = boardObj.AddComponent<SpriteRenderer>();
        
        // 흰색 기본 스프라이트 (또는 적절한 UI 패널 스프라이트 연결 가능)
        // 여기서는 은은한 흰색 판으로 설정
        sr.color = new Color(1f, 1f, 1f, 0.4f); 
        
        // 3. 튜브 배치 로직
        int count = data.tubes.Count;
        int maxPerRow = count >= 8 ? 4 : 3; // 튜브가 많으면 한 줄에 4개까지 배치 가능
        int rowCount = Mathf.CeilToInt((float)count / maxPerRow);
        
        // [중요] 줄 수에 따른 동적 스케일 조절 (안 겹치도록)
        float dynamicScale = tubeScale;
        if (rowCount >= 3) dynamicScale *= 0.6f;     // 3줄 이상이면 60% 크기로
        else if (rowCount == 2) dynamicScale *= 0.75f; // 2줄이면 75% 크기로
        
        // 튜브 실제 높이 추정 (스케일 반영)
        float worldTubeHeight = 6.5f * dynamicScale;
        float worldTubeWidth = 2.0f * dynamicScale;

        // 보드판 내 안쪽 여백 확보 (85% 영역 사용)
        float marginFactor = 0.85f;
        float usableWidth = boardWorldSize.x * marginFactor;
        float usableHeight = boardWorldSize.y * marginFactor;

        // 수평/수직 간격 계산 (튜브 크기를 고려하여 전체가 보드 안에 들어오도록)
        float currentTubeSpacing = maxPerRow > 1 ? (usableWidth - worldTubeWidth) / (maxPerRow - 1) : 0;
        
        // 수직 간격은 튜브 높이가 서로 겹치지 않게 '최소 튜브 높이'만큼을 확보
        float safeVerticalArea = usableHeight - worldTubeHeight;
        float verticalSpacing = rowCount > 1 ? safeVerticalArea / (rowCount - 1) : 0;

        // 너무 벌어지거나 좁아지지 않게 제한 (수직 간격은 튜브 높이보다 크게 하여 겹침 방지)
        currentTubeSpacing = Mathf.Min(currentTubeSpacing, 2.8f);
        verticalSpacing = Mathf.Max(verticalSpacing, worldTubeHeight + 0.3f); 

        // [중요] 수직 중앙 정렬 보정
        // 전체 높이는 (줄수-1)*간격 이지만, 실제 차지하는 시각적 높이는 여기에 튜브 높이 절반씩(위아래)이 더해진 것임
        float totalVisualHeight = (rowCount - 1) * verticalSpacing;
        float startY = boardWorldCenter.y + (totalVisualHeight / 2f); 

        for (int i = 0; i < count; i++)
        {
            int row = i / maxPerRow;
            int col = i % maxPerRow;

            int tubesInThisRow = Mathf.Min(maxPerRow, count - (row * maxPerRow));
            float rowWidth = (tubesInThisRow - 1) * currentTubeSpacing;

            Vector3 spawnPos = new Vector3(
                boardWorldCenter.x - (rowWidth / 2f) + (col * currentTubeSpacing),
                startY - (row * verticalSpacing),
                0
            );

            GameObject tubeObj = Instantiate(tubePrefab, spawnPos, Quaternion.identity, tubeParent);
            tubeObj.transform.localScale = Vector3.one * dynamicScale; 
            
            SortingTube tube = tubeObj.GetComponent<SortingTube>();
            if (tube != null)
            {
                tube.InitializeSlots();
                tube.capacity = data.tubeCapacity;
                _tubes.Add(tube);

                // Spawn items
                if (data.tubes[i].dessertIds != null)
                {
                    foreach (int typeId in data.tubes[i].dessertIds)
                    {
                        if (typeId - 1 >= dessertPrefabs.Length || dessertPrefabs[typeId - 1] == null) continue;
                        
                        GameObject itemObj = Instantiate(dessertPrefabs[typeId - 1], tube.GetNextSlotPosition(), Quaternion.identity);
                        SortingItem item = itemObj.GetComponent<SortingItem>();
                        if (item == null) item = itemObj.AddComponent<SortingItem>();
                        
                        item.dessertType = (DessertType)typeId; 
                        
                        // 현재 튜브 크기(dynamicScale)에 맞춰 디저트 크기 초기화
                        item.InitializeScale(itemTargetSize * dynamicScale); 
                        
                        // 등장 애니메이션: 0에서 톡 튀어나오는 연출
                        item.transform.localScale = Vector3.zero;
                        item.transform.DOScale(item.BaseScale, 0.4f).SetEase(Ease.OutBack).SetDelay((i * 0.05f) + (tube.GetCount() * 0.03f));

                        tube.Push(item);
                    }
                }
            }
        }
    }

    private void CreateDefaultLevel()
    {
        // Internal fallback for Level 1
        DessertLevelData data = new DessertLevelData();
        data.levelId = 1;
        data.tubeCapacity = 4;
        data.tubes = new List<TubeData> {
            new TubeData { dessertIds = new List<int> { 1, 2, 1, 2 } },
            new TubeData { dessertIds = new List<int> { 2, 1, 2, 1 } },
            new TubeData { dessertIds = new List<int> { } }
        };
        SpawnTubes(data);
    }
}
