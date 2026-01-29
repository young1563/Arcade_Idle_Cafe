using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class MapBuilderEditor : EditorWindow
{
    private ShopDataWrapper shopData;
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

        if (shopData != null && shopData.furnitureData != null)
        {
            var folders = shopData.furnitureData.Select(e => e.folderPath.Split('/').Last()).Distinct().ToList();
            folders.Insert(0, "All");
            int selectedIndex = folders.IndexOf(selectedFolder);
            selectedIndex = EditorGUILayout.Popup("카테고리(폴더)", selectedIndex, folders.ToArray());
            selectedFolder = folders[selectedIndex];
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
            shopData = JsonUtility.FromJson<ShopDataWrapper>(File.ReadAllText(path));
    }

    private void SaveSceneData()
    {
        if (shopData == null) LoadJson();
        var holders = FindObjectsByType<FurnitureDataHolder>(FindObjectsSortMode.None);
        foreach (var holder in holders)
        {
            var entity = shopData.furnitureData.Find(e => e.id == holder.data.id);
            if (entity != null)
            {
                entity.position = new Vector3Data { x = holder.transform.position.x, y = holder.transform.position.y, z = holder.transform.position.z };
                entity.scale = new Vector3Data { x = holder.transform.localScale.x, y = holder.transform.localScale.y, z = holder.transform.localScale.z };
                entity.rotation = holder.transform.eulerAngles.y;
                entity.price = holder.data.price;
                entity.unlockOrder = holder.data.unlockOrder;
            }
        }
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "ShopData_AutoGenerated.json"), JsonUtility.ToJson(shopData, true));
        AssetDatabase.Refresh();
        Debug.Log("<color=cyan>배치 정보가 JSON에 저장되었습니다.</color>");
    }
}