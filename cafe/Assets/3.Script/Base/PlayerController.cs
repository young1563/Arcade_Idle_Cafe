using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float turnSpeed = 720f;

    private Rigidbody _rb;
    private Vector3 _moveDir;

    void Start() => _rb = GetComponent<Rigidbody>();

    void Update()
    {
        // 입력 받기
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveDir = new Vector3(h, 0, v).normalized;
    }

    void FixedUpdate()
    {
        if (_moveDir.magnitude > 0.1f)
        {
            // 이동
            _rb.linearVelocity = _moveDir * moveSpeed + Vector3.up * _rb.linearVelocity.y;

            // 회전
            Quaternion targetRot = Quaternion.LookRotation(_moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
        }
    }
}