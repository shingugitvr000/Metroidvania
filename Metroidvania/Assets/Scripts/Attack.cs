using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public float dmgValue = 4;                              //������ ��
    public GameObject throwableObject;                      //������ �ִ� ������Ʈ
    public Transform attackCheck;

    private Rigidbody2D rigidbody2D;
    public Animator animator;                               //�ִϸ����� �Ҵ�
    public bool canAttack = true;                           //���� ���� üũ
    public bool isTimeToCheck = false;                      

    public GameObject cam;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X) && canAttack)                //Ű��Ʈ X ���� ������ ������ �� ������
        {
            canAttack = true;
            animator.SetBool("IsAttacking", true);
            StartCoroutine(AttackCooldown());
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f),
                Quaternion.identity);

            Vector2 direction = new Vector2(transform.localScale.x, 0);
            throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction;          //�ν��Ͻ����� ���ܳ� ������Ʈ�� ���⼺ �Ҵ�
        }        
    }
    IEnumerator AttackCooldown() 
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }
}
