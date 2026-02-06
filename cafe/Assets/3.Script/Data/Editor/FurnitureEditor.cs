using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class FurnitureEditor : EditorWindow
{
    private StageData currentStageData = new StageData();
    private string fileName = "Stage_01.json";
    private string folderPath = "Assets/Resources/Data"; // 저장될 폴더 경로

    [MenuItem("Tools/Furniture Manager")]
    public static void ShowWindow()
    {
        GetWindow<FurnitureEditor>("Furniture Manager");
    }

    void OnGUI()
    {
        GUILayout.Label("Stage Data Editor", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField("파일 이름", fileName);

        if (GUILayout.Button("새 가구 데이터 추가"))
        {
            currentStageData.furnitureList.Add(new FurnitureSaveData());
        }

        EditorGUILayout.Space();

        // 가구 리스트 표시 및 수정
        for (int i = 0; i < currentStageData.furnitureList.Count; i++)
        {
            var item = currentStageData.furnitureList[i];
            EditorGUILayout.BeginVertical("box");
            item.furnitureName = EditorGUILayout.TextField("가구 이름", item.furnitureName);
            item.price = EditorGUILayout.IntField("가격", item.price);
            item.unlockOrder = EditorGUILayout.IntField("해금 순서", item.unlockOrder);
            item.prefabPath = EditorGUILayout.TextField("프리팹 경로", item.prefabPath);

            if (GUILayout.Button("삭제", GUILayout.Width(50)))
            {
                currentStageData.furnitureList.RemoveAt(i);
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("JSON으로 저장")) SaveData();
        if (GUILayout.Button("JSON 불러오기")) LoadData();
        if (GUILayout.Button("씬에 자동 배치")) AutoGenerate();
    }

    void SaveData()
    {
        // 1. 폴더가 없으면 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        string path = Path.Combine(Application.dataPath, "Resources/Data", fileName);
        string json = JsonUtility.ToJson(currentStageData, true);
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        Debug.Log("저장 완료!");
    }

    void LoadData()
    {
        string path = Path.Combine(Application.dataPath, "Resources/Data", fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentStageData = JsonUtility.FromJson<StageData>(json);
        }
    }

    void AutoGenerate()
    {
        // 언락존 프리팹 (Resources 폴더에 있어야 함)
        GameObject unlockZonePrefab = Resources.Load<GameObject>("Prefabs/UnlockZone");

        if (unlockZonePrefab == null)
        {
            Debug.LogError("Resources/Prefabs/UnlockZone 프리팹을 찾을 수 없습니다!");
            return;
        }

        // 순서대로 정렬하여 배치
        var sortedList = currentStageData.furnitureList.OrderBy(x => x.unlockOrder).ToList();

        foreach (var data in sortedList)
        {
            // 1. 가구 생성
            GameObject furniturePrefab = Resources.Load<GameObject>(data.prefabPath);
            if (furniturePrefab == null) continue;

            GameObject furnitureObj = (GameObject)PrefabUtility.InstantiatePrefab(furniturePrefab);
            furnitureObj.transform.position = data.position;
            furnitureObj.transform.eulerAngles = data.rotation;
            furnitureObj.name = data.furnitureName;

            // 2. 언락존 생성 (가구 위치와 동일하게 생성 후 필요시 조정)
            GameObject zoneObj = (GameObject)PrefabUtility.InstantiatePrefab(unlockZonePrefab);
            zoneObj.transform.position = data.position;
            zoneObj.name = $"UnlockZone_{data.furnitureName}";

            // 3. 자동 연결 (핵심 로직)
            UnlockZone zoneScript = zoneObj.GetComponent<UnlockZone>();
            FurnitureDataHolder holder = furnitureObj.GetComponent<FurnitureDataHolder>();

            if (zoneScript != null && holder != null)
            {
                zoneScript.targetFurniture = holder;
                holder.data.price = data.price; // JSON 데이터의 가격 반영

                // 가구 초기 비활성화 세팅
                furnitureObj.SetActive(false);
            }

            Undo.RegisterCreatedObjectUndo(furnitureObj, "Auto Generate Furniture");
            Undo.RegisterCreatedObjectUndo(zoneObj, "Auto Generate Zone");
        }

        Debug.Log("<color=green>모든 가구와 언락존이 자동 배치 및 연결되었습니다!</color>");
    }
}