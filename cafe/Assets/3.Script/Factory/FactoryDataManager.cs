using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MachineData
{
    public string id;
    public string name;
    public string description;
    public int price;
    public string prefabPath;
    public string type;
    public bool isUnlocked; // 해금 여부 (실시간 관리)
}

public class FactoryDataManager : MonoBehaviour
{
    public static FactoryDataManager Instance;
    public TextAsset csvFile; // 인스펙터에서 CSV 파일 연결

    public Dictionary<string, MachineData> machineTable = new Dictionary<string, MachineData>();

    void Awake()
    {
        Instance = this;
        LoadCSV();
    }

    void LoadCSV()
    {
        string[] data = csvFile.text.Split(new char[] { '\n' });

        for (int i = 1; i < data.Length; i++) // 0번은 헤더이므로 제외
        {
            string[] row = data[i].Split(new char[] { ',' });
            if (row.Length < 6) continue;

            MachineData m = new MachineData();
            m.id = row[0].Trim();
            m.name = row[1].Trim();
            m.description = row[2].Trim();
            m.price = int.Parse(row[3].Trim());
            m.prefabPath = row[4].Trim();
            m.type = row[5].Trim();
            m.isUnlocked = false; // 기본은 잠금 상태

            machineTable.Add(m.id, m);
        }
        Debug.Log("CSV 데이터 로드 완료!");
    }
}