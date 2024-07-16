using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterMovement controller;
    public Animator animator;

    public float runSpeed = 40.0f;                          //이동 속도 값 설정                 
    float horizontalMove = 0f;
    bool jump = false;
    bool dash = false;
    
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if(Input.GetKeyDown(KeyCode.Z))
        {
            jump = true;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            dash = true;
        }
    }

    public void OnFall()                    //떨어짐 처리 Event
    {
        animator.SetBool("IsJumping", true);
    }

    public void OnLanding()                 //바닥에 착지처리 Event
    {
        animator.SetBool("IsJumping", false);
    }

    void FixedUpdate()
    {
        //캐릭터 움직임 구현할 함수 
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
    }

}
