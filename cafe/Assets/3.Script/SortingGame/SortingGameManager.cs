using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SortingGameManager : MonoBehaviour
{
    public static SortingGameManager Instance;

    [Header("Settings")]
    public string levelDataFileName = "levels.json";
    public GameObject tubePrefab;
    public GameObject[] dessertPrefabs; // Index matches DessertType enum
    public Transform tubeParent;
    public float tubeSpacing = 2.2f;   // 세로 화면에 맞춰 간격 축소
    public float itemTargetSize = 1.4f; 
    public float tubeScale = 0.85f;    // 세로 화면에 맞춰 튜브 크기 약간 축소

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
            if (board != null)
            {
                // UI 좌표를 스크린 좌표로 변환
                Rect worldRect = board.worldBound;
                
                // 스크린 좌표 -> 월드 좌표 변환 (Z축은 0으로 가정)
                Camera cam = Camera.main;
                Vector3 screenCenter = new Vector3(worldRect.center.x, Screen.height - worldRect.center.y, 10f);
                boardWorldCenter = cam.ScreenToWorldPoint(screenCenter);
                boardWorldCenter.z = 0;

                Vector3 screenTopLeft = new Vector3(worldRect.xMin, Screen.height - worldRect.yMin, 10f);
                Vector3 screenBottomRight = new Vector3(worldRect.xMax, Screen.height - worldRect.yMax, 10f);
                
                Vector3 worldTopLeft = cam.ScreenToWorldPoint(screenTopLeft);
                Vector3 worldBottomRight = cam.ScreenToWorldPoint(screenBottomRight);
                
                boardWorldSize.x = Mathf.Abs(worldBottomRight.x - worldTopLeft.x);
                boardWorldSize.y = Mathf.Abs(worldTopLeft.y - worldBottomRight.y);
                
                Debug.Log($"[Board] Calculated World Center: {boardWorldCenter}, Size: {boardWorldSize}");
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
        int maxPerRow = 3; 
        int rowCount = Mathf.CeilToInt((float)count / maxPerRow);
        
        // 보드판 영역의 80%만 사용하여 여유를 줌
        float usableWidth = boardWorldSize.x * 0.85f;
        float usableHeight = boardWorldSize.y * 0.85f;

        float currentTubeSpacing = count > 1 ? usableWidth / (maxPerRow - 1) : 0;
        float verticalSpacing = rowCount > 1 ? usableHeight / (rowCount - 1) : 0;

        // 간격이 너무 벌어지지 않도록 최댓값 제한
        currentTubeSpacing = Mathf.Min(currentTubeSpacing, 2.8f);
        verticalSpacing = Mathf.Min(verticalSpacing, 5.5f);

        // 시작점 설정 (보드판 중심으로부터 상대적 배치)
        float totalHeight = (rowCount - 1) * verticalSpacing;
        float startY = boardWorldCenter.y + (totalHeight / 2f); 

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
            tubeObj.transform.localScale = Vector3.one * tubeScale; 
            
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
                        item.InitializeScale(itemTargetSize);
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
