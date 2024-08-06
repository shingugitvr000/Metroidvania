using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float life = 10;
    private bool isPlat;
    private bool isObstacle;

    private Transform fallCheck;                        //üũ ��ġ�� Awake �����´�.
    private Transform wallCheck;

    public LayerMask turnLayerMask;                     //���� ���̾� ����ũ �����´�.
    private Rigidbody2D rigidbody2D;
    private bool facingRight = true;
    public float speed = 5f;
    public bool isInvincible = false;                   //���� üũ
    private bool isHitted = false;                      //Ÿ�� üũ

    private Timer hitTimer;
    private Timer destoryTimer;

    public float knockBackDruation = 0.2f;
    private float knockbackCounter;

    private Animator animator;

    public bool isDead = false;

    private void Awake()
    {
        fallCheck = transform.Find("FallCheck");                    //���� ���̷�Ű���� FallCheck(���ӿ�����Ʈ �̸�) ã�Ƽ� �Ҵ�
        wallCheck = transform.Find("WallCheck");                    //���� ���̷�Ű���� WallCheck(���ӿ�����Ʈ �̸�) ã�Ƽ� �Ҵ�
        rigidbody2D = GetComponent<Rigidbody2D>();
        hitTimer = new Timer(0.1f);                                 //�����ð�
        destoryTimer = new Timer(1.25f);                            //�״� ��� ���� ĳ���� ����
        animator = GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        hitTimer.Update(Time.deltaTime);                //����ϰ� �ִ� Ÿ�̸� ������Ʈ���� �ð� �޾Ƽ� ����
        destoryTimer.Update(Time.deltaTime);

        if(isInvincible)                                //�����̰�
        {
            if(hitTimer.GetRemainingTime() <= 0)        //Ÿ�̸� üũ �ؼ� ���� �ð��� ������ ��
            {
                EndInvincible();
            }
        }
        if(knockbackCounter > 0)                            //�˹� �ð� üũ
        {
            knockbackCounter -= Time.deltaTime;
            if(knockbackCounter <= 0)                       //�˹� �Ϸ�Ǹ�
            {
                rigidbody2D.velocity = Vector2.zero;        //�ð� �ʱ�ȭ
            }
        }

        if(destoryTimer.GetRemainingTime() <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if(life < 0)                                //����� üũ
        {
            if(isDead == false)
            {
                StartDestorySequence();
            }
            return;
        }

        if(knockbackCounter > 0)                    //�˹� ���̸� ����
        {
            return;
        }

        isPlat = Physics2D.OverlapCircle(fallCheck.position, 0.2f, 1 << LayerMask.NameToLayer("Default"));
        isObstacle = Physics2D.OverlapCircle(wallCheck.position, 0.2f, turnLayerMask);

        if(!isHitted && Mathf.Abs(rigidbody2D.velocity.y) < 0.5f)
        {
            if(isPlat && !isObstacle)
            {
                if(facingRight)
                {
                    rigidbody2D.velocity = new Vector2(-speed , rigidbody2D.velocity.y);
                }
                else
                {
                    rigidbody2D.velocity = new Vector2(speed, rigidbody2D.velocity.y);
                }
            }
            else
            {
                Flip();
            }
        }

    }

    void Flip()                                     //ĳ���� ���� ��ȯ
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && life > 0 && !isInvincible)
        {
            collision.gameObject.GetComponent<CharacterStatus>().ApplyDamage( 2, transform.position);
        }
    }

    void EndInvincible()            //������ �������� ���°��� �ʱ�ȭ ��Ų��.
    {
        isHitted = false;
        isInvincible = false;
        animator.SetBool("Hit", false);
    }

    void StartHitTimer()                //�ǰ� �޾����� Ÿ�̸� ����
    {
        isHitted = true;
        isInvincible = true;
        hitTimer.Start();
    }

    void StartDestorySequence()                             //���� �ǰ��� ������ �Լ�
    {
        animator.SetTrigger("IsDead");
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        rigidbody2D.velocity = Vector2.zero;
        destoryTimer.Start();
        isDead = true;
    }

    public void ApplyDamage(float damage)                                   //���� ������ ����
    {
        if (!isInvincible)
        {
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            animator.SetTrigger("Hit");
            life -= damage;
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.AddForce(new Vector2(direction * 500f, 100f));
            StartHitTimer();
            knockbackCounter = knockBackDruation;
        }
    }
}
