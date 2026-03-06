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
    public float itemTargetSize = 1.0f; 
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

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pointerPos);
        
        // Scene 뷰에서 레이저가 어디로 날아가는지 2초간 빨간 선으로 보여줍니다.
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
        Debug.Log($"Raycasting at {pointerPos}");

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Hit object: {hit.collider.name}");
            SortingTube tube = hit.collider.GetComponentInParent<SortingTube>();
            if (tube != null)
            {
                OnTubeClicked(tube);
            }
            else
            {
                Debug.LogWarning($"Hit object {hit.collider.name} has no SortingTube in parent!");
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing.");
        }
    }

    private void OnTubeClicked(SortingTube tube)
    {
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
        item.Select(false);
        
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

        float totalWidth = (data.tubes.Count - 1) * tubeSpacing;
        Vector3 startPos = new Vector3(-totalWidth / 2, 0, 0);

        for (int i = 0; i < data.tubes.Count; i++)
        {
            GameObject tubeObj = Instantiate(tubePrefab, startPos + Vector3.right * (i * tubeSpacing), Quaternion.identity, tubeParent);
            tubeObj.transform.localScale = Vector3.one * tubeScale; // 튜브 스케일 일관성 유지
            
            SortingTube tube = tubeObj.GetComponent<SortingTube>();
            
            if (tube == null)
            {
                Debug.LogError($"Tube prefab at index {i} is missing the SortingTube component!");
                continue;
            }

            tube.InitializeSlots(); // 명시적으로 초기화 호출
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
