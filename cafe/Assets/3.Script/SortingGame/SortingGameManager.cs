using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

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

    public void LoadLevel(int levelId)
    {
        // Clear existing
        foreach (var tube in _tubes) Destroy(tube.gameObject);
        _tubes.Clear();
        _selectedTube = null;

        // Load JSON (Simplified for prototype: using Resources.Load or hardcoded path)
        string path = Path.Combine(Application.streamingAssetsPath, levelDataFileName);
        if (!File.Exists(path))
        {
            // Default level if file not found
            CreateDefaultLevel();
            return;
        }

        string json = File.ReadAllText(path);
        DessertLevelList levelList = JsonUtility.FromJson<DessertLevelList>(json);
        DessertLevelData data = levelList.levels.Find(l => l.levelId == levelId);

        if (data != null)
        {
            SpawnTubes(data);
        }
        else
        {
            // 레벨이 없을 경우 다시 1레벨로 루프하거나 기본 레벨 생성
            Debug.LogWarning($"Level {levelId} not found, looping back to Level 1.");
            currentLevel = 1;
            data = levelList.levels.Find(l => l.levelId == 1);
            if (data != null) SpawnTubes(data);
            else CreateDefaultLevel();
        }
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

        int count = data.tubes.Count;
        int maxPerRow = 3; 
        int rowCount = Mathf.CeilToInt((float)count / maxPerRow);
        
        // 간격 조정: 더 오밀조밀하게 모이도록 축소
        float currentTubeSpacing = 2.1f; 
        float verticalSpacing = 4.8f; 

        // 전체 높이 계산 및 시작점 설정 (전체적으로 1.0f 만큼 위로 시프트)
        float totalHeight = (rowCount - 1) * verticalSpacing;
        float startY = 4.5f + (totalHeight / 2f); 

        for (int i = 0; i < count; i++)
        {
            int row = i / maxPerRow;
            int col = i % maxPerRow;

            int tubesInThisRow = Mathf.Min(maxPerRow, count - (row * maxPerRow));
            float rowWidth = (tubesInThisRow - 1) * currentTubeSpacing;

            Vector3 spawnPos = new Vector3(
                -rowWidth / 2f + (col * currentTubeSpacing),
                startY - (row * verticalSpacing),
                0
            );

            GameObject tubeObj = Instantiate(tubePrefab, spawnPos, Quaternion.identity, tubeParent);
            tubeObj.transform.localScale = Vector3.one * tubeScale; 
            
            SortingTube tube = tubeObj.GetComponent<SortingTube>();
            
            if (tube == null)
            {
                Debug.LogError($"Tube prefab at index {i} is missing the SortingTube component!");
                continue;
            }

            tube.InitializeSlots();
            tube.capacity = data.tubeCapacity;
            _tubes.Add(tube);

            // Spawn items
            if (data.tubes[i].dessertIds == null) continue;

            foreach (int typeId in data.tubes[i].dessertIds)
            {
                DessertType type = (DessertType)typeId;
                if (type == DessertType.None) continue;

                if (typeId - 1 >= dessertPrefabs.Length || dessertPrefabs[typeId - 1] == null)
                {
                    Debug.LogWarning($"Dessert prefab for type {type} (ID: {typeId}) is missing or index out of range!");
                    continue;
                }
                
                GameObject itemObj = Instantiate(dessertPrefabs[typeId - 1], tube.GetNextSlotPosition(), Quaternion.identity);
                SortingItem item = itemObj.GetComponent<SortingItem>();
                if (item == null) item = itemObj.AddComponent<SortingItem>();
                
                item.dessertType = type;
                item.InitializeScale(itemTargetSize); // 아이템 크기 정규화 호출
                tube.Push(item);
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
