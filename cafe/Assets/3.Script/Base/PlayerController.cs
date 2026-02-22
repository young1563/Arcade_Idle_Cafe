using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float turnSpeed = 720f;
    
    private Rigidbody _rb;
    private Animator _anim;
    private Vector2 _moveInput;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.useGravity = true;
    }

    // New Input System: PlayerInput 컴포넌트에서 호출
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        
        // 애니메이터에 속도 전달 (보통 Blend Tree의 Speed 파라미터 사용)
        if (_anim != null)
        {
            _anim.SetFloat("Speed", _moveInput.magnitude);
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);

        if (moveDir.magnitude > 0.01f)
        {
            Vector3 vel = moveDir * moveSpeed;
            vel.y = _rb.linearVelocity.y;
            _rb.linearVelocity = vel;

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
        }
    }
}
