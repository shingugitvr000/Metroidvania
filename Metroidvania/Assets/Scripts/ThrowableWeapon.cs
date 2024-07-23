using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableWeapon : MonoBehaviour
{

    public Vector2 direction;                           //2D������ ������ ���� ���ư��� ����
    public bool hasHit = false;
    public float speed = 10.0f;                         //���ư��� �ӵ�   
    void FixedUpdate()
    {
        if(!hasHit)
            GetComponent<Rigidbody2D>().velocity = direction * speed;         //��Ʈ�� �˹�  
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.SendMessage("ApplyDamage", Mathf.Sign(direction.x) * 2f);  //������Ʈ�� �޼����� ������.
            Destroy(gameObject);
        }
        else if(collision.gameObject.tag != "Player")
        {
            Destroy(gameObject);
        }
    }
}
