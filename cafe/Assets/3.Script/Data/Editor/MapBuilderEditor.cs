using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class MapBuilderEditor : EditorWindow
{    
    private Vector2 scrollPos;
    private string selectedFolder = "All";
    private string searchString = "";
    private int tabIndex = 0; // 0: 전체 목록(추가), 1: 배치된 목록(관리)

    private Dictionary<string, Texture2D> previewCache = new Dictionary<string, Texture2D>();
    private bool isPlacing = false;
    private FurnitureEntity currentEntity;
    private GameObject previewObject;

    private MasterDataWrapper libraryData; // 전체 가구 도감 (읽기 전용)
    private MasterDataWrapper mapSaveData; // 실제 배치 데이터 (읽기/쓰기)
    private const string PARENT_NAME = "Cafe"; // 부모가 될 오브젝트 이름

    [MenuItem("Tools/Duzzonku Map Builder Pro")]
    public static void ShowWindow() => GetWindow<MapBuilderEditor>("Duzzonku Builder Pro");
    private void OnEnable()
    {
        LoadJson();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        if (previewObject != null) DestroyImmediate(previewObject);
    }

    private void OnGUI()
    {
        GUILayout.Label("두쫀쿠 매장 빌더 Pro", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- 1. 상단 제어 바 (핵심 액션) ---
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("데이터 로드 (On)")) LoadDataToEditor();
        if (GUILayout.Button("전체 삭제 (Off)")) ClearEditorMap();
        if (GUILayout.Button("JSON 저장 (Save)", GUILayout.Height(20))) SaveSceneData();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // --- 2. 검색 및 탭 메뉴 ---
        searchString = EditorGUILayout.TextField("이름 검색", searchString);
        tabIndex = GUILayout.Toolbar(tabIndex, new string[] { "가구 도감 (추가)", "배치된 목록 (관리)" });

        EditorGUILayout.Space();

        // --- 3. 필터링 및 새로고침 ---
        // 기존에 중복되던 필터 UI 로직을 DrawFilterUI 하나로 통합했습니다.
        DrawFilterUI();

        EditorGUILayout.Space();

        // --- 4. 리스트 영역 ---
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // shopData 대신 현재 탭에 맞는 데이터를 체크합니다.
        MasterDataWrapper currentData = (tabIndex == 0) ? libraryData : mapSaveData;

        if (currentData != null && currentData.furnitureData != null && currentData.furnitureData.Count > 0)
        {
            if (tabIndex == 0)
                DrawFullAssetList(); // libraryData를 사용함
            else
                DrawPlacedObjectList(); // 씬의 하이어라키를 직접 스캔함
        }
        else
        {
            EditorGUILayout.HelpBox(tabIndex == 0 ?
                "도감 데이터(FurnitureLibrary.json)가 없습니다." :
                "배치 데이터(MapData_Stage1.json)가 없거나 씬이 비어있습니다.", MessageType.Warning);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // --- 5. 하단 데이터 관리 ---
        if (GUILayout.Button("JSON 데이터 및 프리뷰 전체 새로고침", GUILayout.Height(30)))
        {
            LoadJson();
            previewCache.Clear();
            Debug.Log("데이터 및 캐시가 초기화되었습니다.");
        }
    }
    // 부모 오브젝트를 찾거나 없으면 생성하는 헬퍼 함수
    private Transform GetParentGroup()
    {
        GameObject parent = GameObject.Find(PARENT_NAME);
        if (parent == null)
        {
            parent = new GameObject(PARENT_NAME);
            Undo.RegisterCreatedObjectUndo(parent, "Create Cafe Parent");
        }
        return parent.transform;
    }

    private void DrawFilterUI()
    {
        // 도감 데이터를 기준으로 필터 목록을 생성합니다.
        if (libraryData == null || libraryData.furnitureData == null) return;

        var folders = libraryData.furnitureData
            .Select(e => e.folderPath.Split('/').Last())
            .Distinct().ToList();
        folders.Insert(0, "All");

        int currentIndex = folders.IndexOf(selectedFolder);
        if (currentIndex == -1) currentIndex = 0;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("카테고리 필터:", GUILayout.Width(80));
        int newIndex = EditorGUILayout.Popup(currentIndex, folders.ToArray());
        selectedFolder = folders[newIndex];

        if (GUILayout.Button("새로고침", GUILayout.Width(60))) { LoadJson(); previewCache.Clear(); }
        EditorGUILayout.EndHorizontal();
    }

    // [탭 0] 전체 가구 도감 (libraryData)
    private void DrawFullAssetList()
    {
        if (libraryData == null) return;        

        foreach (var entity in libraryData.furnitureData)
        {
            if (!FilterMatch(entity)) continue;

            EditorGUILayout.BeginHorizontal("box");
            Texture2D preview = GetAssetPreview(entity);
            GUILayout.Label(preview, GUILayout.Width(40), GUILayout.Height(40));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(entity.prefabName, EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"{entity.type} | {entity.price}G", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("배치", GUILayout.Width(50), GUILayout.Height(35))) StartPlacing(entity);
            EditorGUILayout.EndHorizontal();
        }
    }

    // [탭 1] 현재 씬에 배치된 목록 (관리용)
    private void DrawPlacedObjectList()
    {
        var holders = FindObjectsByType<FurnitureDataHolder>(FindObjectsSortMode.None);
        if (holders.Length == 0) { GUILayout.Label("배치된 가구가 없습니다."); return; }

        foreach (var holder in holders)
        {
            if (holder.data == null) continue;
            if (!FilterMatch(holder.data)) continue;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(holder.gameObject.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"위치: {holder.transform.position.ToString("F1")}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("선택", GUILayout.Width(45))) Selection.activeGameObject = holder.gameObject;
            if (GUILayout.Button("삭제", GUILayout.Width(45))) DestroyImmediate(holder.gameObject);
            EditorGUILayout.EndHorizontal();
        }
    }

    private bool FilterMatch(FurnitureEntity entity)
    {
        // 폴더 필터
        string lastFolder = entity.folderPath.Split('/').Last();
        if (selectedFolder != "All" && lastFolder != selectedFolder) return false;

        // 검색 필터
        if (!string.IsNullOrEmpty(searchString) && !entity.prefabName.ToLower().Contains(searchString.ToLower())) return false;

        return true;
    }

    // --- 핵심 기능: 에디터에서 데이터 로드 (On) ---
    private void LoadDataToEditor()
    {
        ClearEditorMap(); // 1. 기존 씬의 가구들 청소
        LoadJson();       // 2. 파일에서 최신 데이터(mapSaveData) 읽어오기

        // [중요] shopData가 아니라 mapSaveData를 참조해야 합니다.
        if (mapSaveData == null || mapSaveData.furnitureData == null)
        {
            Debug.LogWarning("불러올 배치 데이터(MapData_Stage1.json)가 없습니다.");
            return;
        }

        Transform parentTransform = GetParentGroup(); // 부모 가져오기

        foreach (var entity in mapSaveData.furnitureData)
        {
            // 3. 리소스 로드
            GameObject prefab = Resources.Load<GameObject>($"{entity.folderPath}/{entity.prefabName}");

            if (prefab != null)
            {
                // 4. 에디터 씬에 프리팹 소환
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                go.transform.SetParent(parentTransform);
                // 5. 위치, 회전, 스케일 복구
                go.transform.position = entity.position.ToVector3();
                go.transform.eulerAngles = new Vector3(0, entity.rotation, 0);
                go.transform.localScale = entity.scale.ToVector3();
                go.name = entity.id;

                // 6. 데이터 홀더 연결 및 데이터 주입
                var holder = go.AddComponent<FurnitureDataHolder>();
                holder.data = entity;
            }
            else
            {
                Debug.LogError($"프리팹을 찾을 수 없습니다: {entity.folderPath}/{entity.prefabName}");
            }
        }        
        Debug.Log("<color=cyan>MapData_Stage1.json으로부터 배치를 성공적으로 로드했습니다!</color>");
        Debug.Log($"<color=cyan>{PARENT_NAME} 오브젝트 하위로 배치를 완료했습니다.</color>");
    }

    // --- 핵심 기능: 에디터 맵 비우기 (Off) ---
    private void ClearEditorMap()
    {
        var objects = FindObjectsByType<FurnitureDataHolder>(FindObjectsSortMode.None);
        foreach (var obj in objects)
        {
            DestroyImmediate(obj.gameObject);
        }
        Debug.Log("<color=yellow>에디터의 모든 배치 오브젝트를 삭제했습니다.</color>");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (isPlacing && previewObject != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                float snap = 0.5f;
                Vector3 pos = new Vector3(
                    Mathf.Round(hit.point.x / snap) * snap,
                    hit.point.y,
                    Mathf.Round(hit.point.z / snap) * snap
                );
                previewObject.transform.position = pos;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    FinalizePlacement(pos);
                    Event.current.Use();
                }
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) StopPlacing();
            sceneView.Repaint();
        }
    }

    private void StartPlacing(FurnitureEntity entity)
    {
        isPlacing = true;
        currentEntity = entity;
        if (previewObject != null) DestroyImmediate(previewObject);

        GameObject prefab = Resources.Load<GameObject>($"{entity.folderPath}/{entity.prefabName}");
        previewObject = Instantiate(prefab);
        previewObject.name = "[Preview] " + entity.prefabName;
        previewObject.hideFlags = HideFlags.HideAndDontSave;
    }

    private void FinalizePlacement(Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>($"{currentEntity.folderPath}/{currentEntity.prefabName}");
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        // 부모 설정
        go.transform.SetParent(GetParentGroup());

        go.transform.position = pos;
        go.name = currentEntity.id;

        var holder = go.AddComponent<FurnitureDataHolder>();
        holder.data = currentEntity;

        Undo.RegisterCreatedObjectUndo(go, "Place Furniture");
        StopPlacing();
    }

    private void StopPlacing()
    {
        isPlacing = false;
        if (previewObject != null) DestroyImmediate(previewObject);
    }

    private Texture2D GetAssetPreview(FurnitureEntity entity)
    {
        string path = $"{entity.folderPath}/{entity.prefabName}";
        if (previewCache.ContainsKey(path) && previewCache[path] != null) return previewCache[path];

        GameObject prefab = Resources.Load<GameObject>(path);
        Texture2D tex = AssetPreview.GetAssetPreview(prefab);
        if (tex != null) previewCache[path] = tex;
        return tex ?? Texture2D.blackTexture;
    }

    private void LoadJson()
    {
        // 1. 가구 도감 로드
        string libPath = Path.Combine(Application.streamingAssetsPath, "FurnitureLibrary.json");
        if (File.Exists(libPath))
            libraryData = JsonUtility.FromJson<MasterDataWrapper>(File.ReadAllText(libPath));

        // 2. 맵 배치 데이터 로드
        string mapPath = Path.Combine(Application.streamingAssetsPath, "MapData_Stage1.json");
        if (File.Exists(mapPath))
            mapSaveData = JsonUtility.FromJson<MasterDataWrapper>(File.ReadAllText(mapPath));
        else
            mapSaveData = new MasterDataWrapper { furnitureData = new List<FurnitureEntity>() };
    }

    // [저장 기능] 이제 MapData_Stage1.json에만 저장함
    private void SaveSceneData()
    {
        MasterDataWrapper newMapData = new MasterDataWrapper();
        newMapData.furnitureData = new List<FurnitureEntity>();

        // 1. 플레이어 정보 저장
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            newMapData.playerPosition = new Vector3Data
            {
                x = player.transform.position.x,
                y = player.transform.position.y,
                z = player.transform.position.z
            };
            newMapData.playerRotation = player.transform.eulerAngles.y;
        }

        // 2. 씬의 모든 가구 정보 저장
        var holders = FindObjectsByType<FurnitureDataHolder>(FindObjectsSortMode.None);
        foreach (var holder in holders)
        {
            // 씬에서 수정한 실시간 위치/회전을 데이터에 동기화 (holder에 추가한 함수 호출)
            holder.SyncTransformToData();

            FurnitureEntity entity = new FurnitureEntity
            {
                id = holder.gameObject.name, // 씬의 이름을 ID로 사용하여 복사본 구분
                prefabName = holder.data.prefabName,
                folderPath = holder.data.folderPath,
                type = holder.data.type,
                position = holder.data.position,
                rotation = holder.data.rotation,
                scale = holder.data.scale,
                price = holder.data.price,
                unlockOrder = holder.data.unlockOrder,
                isUnlocked = holder.data.isUnlocked // 이제 에러가 나지 않습니다!
            };
            newMapData.furnitureData.Add(entity);
        }

        string json = JsonUtility.ToJson(newMapData, true);
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "MapData_Stage1.json"), json);
        AssetDatabase.Refresh();
        Debug.Log("<color=cyan>배치 데이터가 MapData_Stage1.json에 저장되었습니다.</color>");

        /*        string finalJson = JsonUtility.ToJson(masterData, true);
                File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "ShopData_AutoGenerated.json"), finalJson);

                AssetDatabase.Refresh();
                Debug.Log($"<color=cyan>통합 저장 성공! 플레이어 위치와 가구 {masterData.furnitureData.Count}개가 저장되었습니다.</color>");*/
    }
}