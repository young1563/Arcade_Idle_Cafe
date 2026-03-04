using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SortingGameManager : MonoBehaviour
{
    public static SortingGameManager Instance;

    [Header("Settings")]
    public string levelDataFileName = "levels.json";
    public GameObject tubePrefab;
    public GameObject[] dessertPrefabs; // Index matches DessertType enum
    public Transform tubeParent;
    public float tubeSpacing = 2.5f;

    [Header("Runtime")]
    public int currentLevel = 1;
    private List<SortingTube> _tubes = new List<SortingTube>();
    private SortingTube _selectedTube;
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

        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            SortingTube tube = hit.collider.GetComponentInParent<SortingTube>();
            if (tube != null)
            {
                OnTubeClicked(tube);
            }
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
            }
        }
        else
        {
            // Try Move
            if (_selectedTube == tube)
            {
                // Deselect
                _selectedTube.PeekItem().Select(false);
                _selectedTube = null;
            }
            else if (tube.CanPush(_selectedTube.PeekItem()))
            {
                MoveItem(_selectedTube, tube);
                _selectedTube = null;
            }
            else
            {
                // Invalid move, shake or feedback?
                _selectedTube.PeekItem().Select(false);
                _selectedTube = null;
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
        });
    }

    private void CheckWinCondition()
    {
        foreach (var tube in _tubes)
        {
            if (!tube.IsComplete()) return;
        }
        
        Debug.Log("Level Complete!");
        // Load next level after delay
        Invoke(nameof(NextLevel), 1.5f);
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
        float totalWidth = (data.tubes.Count - 1) * tubeSpacing;
        Vector3 startPos = new Vector3(-totalWidth / 2, 0, 0);

        for (int i = 0; i < data.tubes.Count; i++)
        {
            GameObject tubeObj = Instantiate(tubePrefab, startPos + Vector3.right * (i * tubeSpacing), Quaternion.identity, tubeParent);
            SortingTube tube = tubeObj.GetComponent<SortingTube>();
            tube.capacity = data.tubeCapacity;
            _tubes.Add(tube);

            // Spawn items
            foreach (int typeId in data.tubes[i].dessertIds)
            {
                DessertType type = (DessertType)typeId;
                if (type == DessertType.None) continue;
                
                GameObject itemObj = Instantiate(dessertPrefabs[typeId - 1], tube.GetNextSlotPosition(), Quaternion.identity);
                SortingItem item = itemObj.AddComponent<SortingItem>(); // or handle via prefab
                item.dessertType = type;
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
