using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400.0f;                                //점프 시 추가되는 힘의 양
    [Range(0, 0.3f)][SerializeField] private float m_MovementSmoothing = 0.05f;         //이동을 부드럽게 하기 위해 평탄화 
    [SerializeField] private bool m_AirControl = false;                                 //점프 중에도 조작 할 수 있는지 여부
    [SerializeField] private LayerMask m_WhatIsGround;                                  //캐릭터에게 땅이 무엇인지 결정하는 마스크
    [SerializeField] private Transform m_GroundCheck;                                   //플레이어가 땅에 닿았는지 확인하는 위치
    [SerializeField] private Transform m_WallCheck;                                     //플레이어가 벽에 닿았는지 확인하는 위치

    const float k_GroundedRadius = 0.2f;                            //땅에 닿았는지 확인하는 겹침 원의 반경 
    private bool m_IsGrounded;                                      //플레이어가 땅에 닿았는지 여부
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;                              //플레이어가 현재 어느 방향을 바라보고 있는 지 결정
    private Vector3 velocity = Vector3.zero;                        //속도
    private float limitFallSpeed = 25f;                              //낙하 속도 제한

    public bool canDoubleJump = true;                               //플레이어가 더블 점프를 할 수 있는지 여부
    private bool m_IsWall = false;                                  //플레이어 앞에 벽이 있는지 여부
    private bool m_IsWallSlidding = false;                          //플레이어가 벽을 타고 있는지 여부
    private bool oldWallSlidding = false;                           //이전 프레임에서 플레이어가 벽을 타고 있었는지 여부
    private float prevVelocityX = 0f;
    private bool canCheck = false;                                  //플레이어가 벽을 타고 있는지 확인하기 위한 여부

    private Animator animator;

    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0;                    //플레이어와 벽 사이의 거리
    private bool limitVelOnWallJump = false;            //낮은 FPS에서 벽 점프 거리를 제한하기 위한 변수

    [Header("Events")]
    [Space]
    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;
    private CharacterStatus status;
    private void Awake()                //컴포넌트들을 가져와서 셋팅한다.
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        status = GetComponent<CharacterStatus>();

        if (OnFallEvent == null)                //이벤트가 추가 안되어 있을 때 이벤트를 할당한다. 
            OnFallEvent = new UnityEvent();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }

    private void FixedUpdate()                      //FiexedUpdate에서 이동 구현을 한다. 
    {
        bool wasGrouned = m_IsGrounded;             //땅 체크 값을 이전 변수에 넣어서 매 프레임마다 갱신 
        m_IsGrounded = false;
        //GroundCheck 위치에서 땅으로 지정된 것과 겹치는지 확인 
        Collider2D[] groundColiiders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for(int i = 0; i < groundColiiders.Length; i++)
        {
            if (groundColiiders[i].gameObject != gameObject)
            {
                m_IsGrounded = true;
                if(!wasGrouned)
                {
                    OnLandEvent.Invoke();
                    animator.SetBool("IsJumping", false);
                    animator.SetBool("IsDoubleJumping", false);
                    animator.SetBool("JumpUp", false);
                    if (!m_IsWall && !status.isDashing) canDoubleJump = true;
                    if (m_Rigidbody2D.velocity.y < 0f) limitVelOnWallJump = false;                   
                }
            }
        }
        if (!m_IsGrounded) OnFallEvent.Invoke();                    //땅이 아닐래 떨어지고 있다는 이벤트를 호출

        bool wasWallSlidding = m_IsWallSlidding;
        m_IsWallSlidding = false;
        m_IsWall = false;

        //WallCheck 위치에서 벽으로 지정된 것과 겹치는지 확인
        Collider2D[] WallColiiders = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
        for(int i = 0; i < WallColiiders.Length ; i++)
        {
            if (WallColiiders[i].gameObject != gameObject)
            {
                m_IsWall = true;
                if (!m_IsGrounded && !status.isDashing && m_Rigidbody2D.velocity.y < 0)
                {
                    m_IsWallSlidding = true;
                    if(!wasWallSlidding)                //이전 프레임이 벽이 아니였다면 체크하여 돌려서 벽을 슬라이딩 하고 검출 하게 한다. 
                    {
                        m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                        Flip();
                    }
                }
            }
        }

        prevVelocityX = m_Rigidbody2D.velocity.x;           //이전 속도를 할당한다.

        if(limitVelOnWallJump)
        {
            if (m_Rigidbody2D.velocity.y < -0.5f) limitVelOnWallJump = false;
            jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;       //벽과의 거리를 계산한다.
            if(jumpWallDistX < -0.5f && jumpWallDistX > -1f)
            {
                status.canMove = true;
            }
            else if(jumpWallDistX < -1f && jumpWallDistX >= -2f)
            {
                status.canMove = true;
                m_Rigidbody2D.velocity = new Vector2(10.0f * transform.localScale.x, m_Rigidbody2D.velocity.y);
            }
            else if(jumpWallDistX < -2)
            {
                limitVelOnWallJump = false;
                m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            }
            else if(jumpWallDistX > 0)
            {
                limitVelOnWallJump = false;
                m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            }
        }

        if(!m_IsWallSlidding && wasWallSlidding)
        {
            m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
            animator.SetBool("IsWallSliding", false);
        }

        animator.SetFloat("Speed", Mathf.Abs(m_Rigidbody2D.velocity.x));        //애니메이터에 속도 전달 
    }

    public void Move(float move, bool jump , bool dash)
    {
        if(status.canMove)          //클래스에서 이동이 가능하다고 하면
        {
            if(dash && status.canDash && !m_IsWallSlidding) //대시가 가능한 조건을 검사하여 대시 실행
            {
                status.StartDash(0.1f);
            }
            if(status.isDashing)        //대시 중일 때 처리
            {
                m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * status.mDashForce, 0);
            }
            else if(m_IsGrounded || m_AirControl)   //플레이어가 땅에 닿았거나 공중 제어가 켜져있을때 
            {
                if (m_Rigidbody2D.velocity.y < -limitFallSpeed) m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -limitFallSpeed); //속도 제한

                Vector3 targetVelocity = new Vector2(move * 10.0f, m_Rigidbody2D.velocity.y);   //목표 속도를 계산
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing); //속도를 부드럽게 변경

                if(move > 0 && !m_FacingRight && !m_IsWallSlidding) //입력이 오른쪽, 플레이어가 왼쪽 보고 있다면
                {
                    Flip();                                         //플레이어를 뒤집는다.
                }
                else if(move < 0 && m_FacingRight && !m_IsWallSlidding)
                {
                    Flip();
                }               
            }

            if(m_IsGrounded && jump)        //플레이어 점프
            {
                animator.SetBool("IsJumping", true);                    //애니메이터에 점프 상태 전달
                animator.SetBool("JumpUp", true);
                m_IsGrounded = false;                                   //점프하면서 땅에 있지 않으므로 BOOL 값 변경
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));   //플레이어에게 수직 힘을 추가 
                canDoubleJump = true;
            }
            else if(!m_IsGrounded && jump && canDoubleJump && !m_IsWallSlidding)
            {
                canDoubleJump = false;
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));   //플레이어에게 수직 힘을 추가 
                animator.SetBool("IsDoubleJumping", true);              //애니메이터에 더블 점프 상태 전달
            }
            else if(m_IsWall && !m_IsGrounded)
            {
                if(!oldWallSlidding && m_Rigidbody2D.velocity.y < 0 || status.isDashing)    //벽에 붙어서 슬라이딩 되는 순간
                {
                    m_IsWallSlidding = true;
                    m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                    Flip();
                    status.checkTimer.Start();
                    canDoubleJump = true;
                    animator.SetBool("IsWallSliding", true);
                }
                status.isDashing = false;

                if(m_IsWallSlidding)                                            //벽 슬라이딩 중
                {
                    if(move * transform.localScale.x > 0.1f)
                    {
                        status.endSlidingTimer.Start();
                    }
                    else
                    {
                        oldWallSlidding = true;
                        m_Rigidbody2D.velocity = new Vector2(-transform.localScale.x * 2, -5);
                    }
                }

                if(jump && m_IsWallSlidding)                                    //벽 슬라이딩 중 점프
                {
                    animator.SetBool("IsJumping", true);                        //애니메이터에 점프 상태 전달
                    animator.SetBool("JumpUp", true);
                    m_Rigidbody2D.velocity = new Vector2(0.0f, 0.0f);           //속도값 보정
                    m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce * 1.2f, m_JumpForce));  //점프 힘들 전달해준다. 
                    jumpWallStartX = transform.position.x;
                    limitVelOnWallJump = true;
                    canDoubleJump = true;
                    m_IsWallSlidding = false;
                    animator.SetBool("IsWallSliding", false);
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    status.moveTimer.Start();
                }
                else if(dash && status.canDash)                                     //벽 대시 설정 
                {
                    m_IsWallSlidding = false;
                    animator.SetBool("IsWallSliding", false);
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    canDoubleJump = true;
                    status.StartDash(0.1f);
                }
            }
            else if(m_IsWallSlidding && !m_IsWall && status.checkTimer.GetRemainingTime() <= 0) //벽 슬라이딩 중이고 벽이 끝났을 때
            {
                m_IsWallSlidding = false;
                animator.SetBool("IsWallSliding", false);
                oldWallSlidding = false;
                m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                canDoubleJump = true;
            }
        }
    }

    private void Flip()                             //플립 함수를 호출 하여 방향 전환
    {
        m_FacingRight = !m_FacingRight;             //플레이어가 바라보는 방향을 전환
        Vector3 theScale = transform.localScale;    //플레이어의 x 로컬 스케일을 -1로 곱해서 뒤집어 준다.
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
