using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine; // 시네머신 네임스페이스 추가

public class CompassNavigator : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera mainCamera;
    public CinemachineBrain brain; // 시네머신 브레인 직접 참조
    public Transform player;
    public float viewDuration = 2.0f;
    public float moveSpeed = 1.0f;

    [Header("UI 설정")]
    public Button compassButton;

    private Vector3 _originalOffset;
    private bool _isViewing = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // 시네머신 브레인 자동 할당
        if (brain == null) brain = mainCamera.GetComponent<CinemachineBrain>();

        // 초기 오프셋 계산 (시네머신 사용 시에도 현재 카메라 위치 기반으로 저장)
        _originalOffset = mainCamera.transform.position - player.position;

        if (compassButton != null)
            compassButton.onClick.AddListener(ShowNextTarget);
    }

    public void ShowNextTarget()
    {
        if (_isViewing) return;

        UnlockZone nextZone = Object.FindObjectsByType<UnlockZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .Length > 0 ? Object.FindObjectsByType<UnlockZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)[0] : null;

        if (nextZone == null) return;

        StartCoroutine(ViewRoutine(nextZone.transform.position));
    }

    IEnumerator ViewRoutine(Vector3 targetPos)
    {
        _isViewing = true;
        compassButton.interactable = false;

        // [중요] 1. 시네머신 브레인을 비활성화하여 DOTween이 카메라를 제어하게 함
        if (brain != null) brain.enabled = false;

        Vector3 targetCameraPos = targetPos + _originalOffset;

        // 2. 카메라 이동
        yield return mainCamera.transform.DOMove(targetCameraPos, moveSpeed).SetEase(Ease.InOutSine).WaitForCompletion();

        // 3. 관찰 시간
        yield return new WaitForSeconds(viewDuration);

        // 4. 복귀 (플레이어 위치 + 오프셋으로 이동)
        yield return mainCamera.transform.DOMove(player.position + _originalOffset, moveSpeed).SetEase(Ease.InOutSine).WaitForCompletion();

        // [중요] 5. 이동이 완전히 끝난 후 시네머신 브레인을 다시 활성화
        if (brain != null) brain.enabled = true;

        _isViewing = false;
        compassButton.interactable = true;
    }
}