using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public VariableJoystick joystick; // 에셋 스토어의 Joystick Pack 사용 권장
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 하이퍼캐주얼은 물리 충돌로 캐릭터가 넘어지면 안되므로 회전 고정
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 조이스틱 값을 받아옴 (마우스/터치 공용)
        float x = joystick.Horizontal;
        float z = joystick.Vertical;

        moveDirection = new Vector3(x, 0, z).normalized;
    }

    void FixedUpdate()
    {
        if (moveDirection.magnitude >= 0.1f)
        {
            // 1. 이동
            rb.linearVelocity = moveDirection * moveSpeed;

            // 2. 부드러운 회전
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            rb.linearVelocity = Vector3.zero; // 멈췄을 때 미끄러짐 방지
        }
    }
}