using UnityEngine;
using System.Collections.Generic;

public class Table : MonoBehaviour
{
    public bool isOccupied = false; // 누군가 앉아있는가?
    public Transform sitPoint;      // 손님이 앉을 위치
    public GameObject trashPrefab;  // 다 먹고 남길 쓰레기

    // 손님이 자리가 있는지 물어볼 때 사용
    public bool CanSit() => !isOccupied;

    public void Occupy() => isOccupied = true;
    public void Release()
    {
        isOccupied = false;
        // 다 먹었을 때 쓰레기 소환 로직 등을 여기에 추가
    }
}