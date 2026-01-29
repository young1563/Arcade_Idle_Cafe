using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class MapBuilderEditor : EditorWindow
{
    private MasterDataWrapper shopData;
    private Vector2 scrollPos;
    private string selectedFolder = "All";
    private Dictionary<string, Texture2D> previewCache = new Dictionary<string, Texture2D>();

    // 배치 및 로드 상태 관리
    private bool isPlacing = false;
    private FurnitureEntity currentEntity;
    private GameObject previewObject;
    private bool isDataLoadedInEditor = false;

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

        // --- 에디터 로드 제어 섹션 ---
        EditorGUILayout.BeginVertical("helpbox");
        GUILayout.Label("에디터 뷰어 제어", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("데이터 로드 (On)", GUILayout.Height(30))) LoadDataToEditor();
        if (GUILayout.Button("전체 삭제 (Off)", GUILayout.Height(30))) ClearEditorMap();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // --- 데이터 관리 섹션 ---
        if (GUILayout.Button("JSON 데이터 및 프리뷰 새로고침")) { LoadJson(); previewCache.Clear(); }

        // --- OnGUI 내부의 카테고리 필터링 부분 ---
        if (shopData != null && shopData.furnitureData != null && shopData.furnitureData.Count > 0)
        {
            // 1. 모든 가구의 폴더 경로에서 마지막 이름만 추출하여 리스트화
            var folders = shopData.furnitureData
                .Select(e => e.folderPath.Split('/').Last())
                .Distinct()
                .ToList();

            folders.Insert(0, "All"); // 맨 앞에 'All' 추가

            // 2. 현재 선택된 폴더가 리스트에 있는지 확인 (없으면 "All"로 초기화)
            int currentIndex = folders.IndexOf(selectedFolder);
            if (currentIndex == -1) currentIndex = 0;

            // 3. 팝업 메뉴 출력 (안전하게 index 관리)
            int newIndex = EditorGUILayout.Popup("카테고리(폴더)", currentIndex, folders.ToArray());
            selectedFolder = folders[newIndex];
        }
        else
        {
            GUILayout.Label("표시할 가구 데이터가 없습니다. JSON을 확인하세요.");
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        DrawFurnitureList();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("현재 씬 배치 정보 JSON 저장", GUILayout.Height(40))) SaveSceneData();
    }

    private void DrawFurnitureList()
    {
        if (shopData == null || shopData.furnitureData == null) return;

        foreach (var entity in shopData.furnitureData)
        {
            string lastFolder = entity.folderPath.Split('/').Last();
            if (selectedFolder != "All" && lastFolder != selectedFolder) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            // 프리뷰 이미지
            Texture2D preview = GetAssetPreview(entity);
            GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"이름: {entity.prefabName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"가격: {entity.price} | 순서: {entity.unlockOrder}");
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("배치 시작", GUILayout.Width(70), GUILayout.Height(50))) StartPlacing(entity);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

    // --- 핵심 기능: 에디터에서 데이터 로드 (On) ---
    private void LoadDataToEditor()
    {
        ClearEditorMap(); // 중복 생성 방지
        LoadJson();

        if (shopData == null) return;

        foreach (var entity in shopData.furnitureData)
        {
            if (entity.position.x == 0 && entity.position.y == 0 && entity.position.z == 0) continue;

            GameObject prefab = Resources.Load<GameObject>($"{entity.folderPath}/{entity.prefabName}");
            if (prefab != null)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = entity.position.ToVector3();
                go.transform.eulerAngles = new Vector3(0, entity.rotation, 0);
                go.transform.localScale = entity.scale.ToVector3();
                go.name = entity.id;

                var holder = go.AddComponent<FurnitureDataHolder>();
                holder.data = entity;
            }
        }
        Debug.Log("<color=cyan>에디터에 배치 데이터를 불러왔습니다.</color>");
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
        string path = Path.Combine(Application.streamingAssetsPath, "ShopData_AutoGenerated.json");
        if (File.Exists(path))
            shopData = JsonUtility.FromJson<MasterDataWrapper>(File.ReadAllText(path));
    }

    private void SaveSceneData()
    {
        MasterDataWrapper masterData = new MasterDataWrapper();
        masterData.furnitureData = new List<FurnitureEntity>();

        // 1. 플레이어 정보 저장
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            masterData.playerPosition = new Vector3Data
            {
                x = player.transform.position.x,
                y = player.transform.position.y,
                z = player.transform.position.z
            };
            masterData.playerRotation = player.transform.eulerAngles.y;
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
            masterData.furnitureData.Add(entity);
        }

        string finalJson = JsonUtility.ToJson(masterData, true);
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "ShopData_AutoGenerated.json"), finalJson);

        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>통합 저장 성공! 플레이어 위치와 가구 {masterData.furnitureData.Count}개가 저장되었습니다.</color>");
    }
}