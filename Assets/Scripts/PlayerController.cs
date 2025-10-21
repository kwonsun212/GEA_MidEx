using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("이동설정")]
    public float walkSpeed = 3;
    public float runSpeed = 6f;
    public float rotationSpeed = 10;

    [Header("점프 설정")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;              //중력 속도 추가
    public float landingDuration = 0.3f;        //착지 후 착지 지속 시간

    [Header("공격설정")]
    public float attackDuration = 0.8f;
    public bool canmoveWhileAttacking = false;

    [Header("컴포넌트")]
    public Animator animator;

    private CharacterController controller;
    private Camera playerCamera;

    //현재상태
    private float currentSpeed;
    private bool isAttacking = false;           //공격중인지 체크
    private bool isLanding = false;             //착지 중인지 체크
    private float landingTimer;                 //착지 타이머

    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;                   //이전 프레임에 땅 이었는지
    private float attackTimer;

    private bool isUIMode = false;              //UI 모드 설정


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCursorLock();
        }

        if (!isUIMode)
        {
            CheckGrounded();
            HandleLanding();
            HandleMovement();
            HandleJump();
            HandleAttack();
            UpdateAnimator();
        }

    }

    void CheckGrounded()
    {
        //이전 상태 저장
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;             //캐릭터 컨트롤러에서 받아온다.

        if (!isGrounded && wasGrounded)                  //땅에서 떨어졌을때(지금 프레임은 땅이 아니고 이전 프레임은 땅)
        {
            Debug.Log("떨어지기 시작");
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;

            //착지 모션 트리거 및 착지 상태 시작
            if (!wasGrounded && animator != null)
            {
                isLanding = true;
                landingTimer = landingDuration;
                Debug.Log("착지");
            }
        }
    }


    void HandleLanding()
    {
        if (isLanding)
        {
            landingTimer -= Time.deltaTime;                 //랜딩 타이머 시간 만큼 못 움직임.
            if (landingTimer <= 0)
            {
                isLanding = false;              //착지 완료
            }
        }
    }

    void HandleAttack()
    {
        if (isAttacking)                         //공격 중일때
        {
            attackTimer -= Time.deltaTime;      //타이머를 감소 시킨다.
            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !isAttacking)            //공격중이 아닐때 키를 누르면 공격
        {
            Debug.Log("공격중");
            isAttacking = true;                 //공격중 표시
            attackTimer = attackDuration;          //타이머 리필

            if (animator != null)
            {
                animator.SetTrigger("attackTrigger");
            }
        }

    }
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
            {
                animator.SetTrigger("jumpTrigger");
            }
        }
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()                           //이동 함수 제작
    {

        if ((isAttacking && !canmoveWhileAttacking) || isLanding)                //공격 중이거나 착지 중일 때 움직임 제한
        {
            currentSpeed = 0;
            return;
        }
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0 || vertical != 0)        //둘중 하나라도 있을때
        {
            //카메라가 보는 방향이 앞쪽으로 되게 설정
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = cameraForward * vertical + cameraRight * horizontal;    //이동 방향 설정

            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = runSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);     //캐릭터 컨트롤러의 이동 입력

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            currentSpeed = 0;       //이동이 아닐 경우 0
        }


    }


    void UpdateAnimator()
    {
        float animatorSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        animator.SetFloat("speed", animatorSpeed);
        animator.SetBool("isGrounded", isGrounded);

        bool isFalling = !isGrounded && velocity.y < -0.1f;              //캐릭터의 Y 축 속도가 음수로 넘어가면 떨어지고 있다고 판단
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isLanding", isLanding);


    }

    public void SetCursorLock(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isUIMode = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isUIMode = true;

        }
    }

    public void ToggleCursorLock()
    {
        bool shouldLock = Cursor.lockState != CursorLockMode.Locked;
        SetCursorLock(shouldLock);
    }

    public void SetUIMode(bool uiMode)
    {
        SetCursorLock(!uiMode);
    }
}
