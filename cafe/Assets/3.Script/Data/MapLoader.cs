using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MapLoader : MonoBehaviour
{
    [Header("Data Settings")]
    public string jsonFileName = "MapData_Stage1.json";

    [Header("Unlock Settings")]
    public GameObject unlockZonePrefab; // 에디터에서 원형 결제구역 프리팹을 할당하세요.

    void Start()
    {
        LoadAndSpawnMasterData();
    }

    public void LoadAndSpawnMasterData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);            
            MasterDataWrapper masterData = JsonUtility.FromJson<MasterDataWrapper>(jsonText);

            if (masterData == null) return;
                        
            RestorePlayerPosition(masterData);
                        
            if (masterData.furnitureData != null)
            {
                foreach (var entity in masterData.furnitureData)
                {                    
                    if (entity.position.x == 0 && entity.position.y == 0 && entity.position.z == 0)
                        continue;

                    SpawnFurniture(entity);
                }
            }
            Debug.Log("<color=green>통합 데이터 로드 완료!</color>");
            UnlockManager.Instance.InitUnlockSystem();
        }
    }

    private void RestorePlayerPosition(MasterDataWrapper data)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && data.playerPosition != null)
        {
            // CharacterController가 있다면 일시적으로 끄고 이동해야 씹히지 않습니다.
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = data.playerPosition.ToVector3();
            player.transform.eulerAngles = new Vector3(0, data.playerRotation, 0);

            if (cc != null) cc.enabled = true;
            Debug.Log("플레이어 위치 복구 완료.");
        }
    }

    private void SpawnFurniture(FurnitureEntity entity)
    {
        string fullPath = $"{entity.folderPath}/{entity.prefabName}";
        GameObject prefab = Resources.Load<GameObject>(fullPath);

        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab,
                entity.position.ToVector3(),
                Quaternion.Euler(0, entity.rotation, 0));

            instance.transform.localScale = entity.scale.ToVector3();
            instance.name = entity.id;

            // 데이터 보관을 위해 홀더 부착
            var holder = instance.AddComponent<FurnitureDataHolder>();
            holder.data = entity;

            // 1. 해금 상태에 따라 가구의 활성화 여부 결정
            instance.SetActive(entity.isUnlocked);

            // 2. 해금되지 않은 가구라면 UnlockZone(결제 구역) 생성
            if (!entity.isUnlocked && unlockZonePrefab != null)
            {
                CreateUnlockZone(instance, holder);
            }
        }
    }
    private void CreateUnlockZone(GameObject targetFurniture, FurnitureDataHolder holder)
    {
        // 가구의 위치와 동일한 곳(또는 약간 앞)에 결제 구역 생성
        GameObject zoneObj = Instantiate(unlockZonePrefab, targetFurniture.transform.position, Quaternion.identity);

        UnlockZone zone = zoneObj.GetComponent<UnlockZone>();
        if (zone != null)
        {
            // 결제 구역이 어떤 가구를 담당하는지 연결
            zone.targetFurniture = holder;
        }
    }
}