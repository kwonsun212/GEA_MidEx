using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyChaser : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;
    public float gravity = -20f;

    CharacterController cc;
    Vector3 vel;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (target == null && Camera.main)
        {
            target = Camera.main.transform; // 플레이어 카메라 쪽
        }
    }

    void Update()
    {
        if (!target) return;

        Vector3 to = target.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;

        if (dist > stopDistance)
        {
            Vector3 dir = to.normalized;
            cc.Move(dir * moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }

        // 중력
        if (cc.isGrounded && vel.y < 0) vel.y = -2f;
        vel.y += gravity * Time.deltaTime;
        cc.Move(vel * Time.deltaTime);
    }
}
