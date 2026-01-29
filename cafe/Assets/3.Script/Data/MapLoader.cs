using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MapLoader : MonoBehaviour
{
    public string jsonFileName = "ShopData.json";

    void Start()
    {
        LoadMap();
    }

    void LoadMap()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            ShopDataWrapper data = JsonUtility.FromJson<ShopDataWrapper>(jsonText);

            foreach (var entity in data.funitureData)
            {
                SpawnFurniture(entity);
            }

            // 모든 배치가 끝난 후 NavMesh 갱신 (손님 AI를 위해)
            // NavMeshSurface.BuildNavMesh(); 
        }
    }

    void SpawnFurniture(FurnitureEntity entity)
    {
        // 1. Resources 폴더에서 프리팹 로드
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{entity.prefabName}");

        if (prefab != null)
        {
            // 2. 지정된 위치와 회전으로 생성
            GameObject instance = Instantiate(prefab, entity.position.ToVector3(), Quaternion.Euler(0, entity.rotation, 0));
            instance.name = entity.id;

            // 3. 타입에 따른 컴포넌트 데이터 초기화 (필요시)
            if (entity.type == "Producer" && instance.TryGetComponent<Producer>(out var prod))
            {
                // 데이터 기반으로 용량 등 설정 가능
            }
        }
        else
        {
            Debug.LogWarning($"{entity.prefabName} 프리팹을 찾을 수 없습니다!");
        }
    }
}