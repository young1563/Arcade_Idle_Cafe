using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public VariableJoystick joystick; // 에셋 스토어의 Joystick Pack 사용 권장
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;

    private Rigidbody rb;
    private Animator anim;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>(); // 컴포넌트 가져오기

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 조이스틱 값을 받아옴 (마우스/터치 공용)
        float x = joystick.Horizontal;
        float z = joystick.Vertical;

        moveDirection = new Vector3(x, 0, z).normalized;

        // 조이스틱의 입력 세기(0~1)를 애니메이터의 Speed 파라미터에 전달
        // 0.1보다 커지면 스크린샷의 조건에 의해 Walk 애니메이션이 재생됩니다.
        if (anim != null)
        {
            float inputMagnitude = new Vector2(x, z).magnitude;
            anim.SetFloat("Speed", inputMagnitude);
        }
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