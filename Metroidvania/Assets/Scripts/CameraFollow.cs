using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float FollowSpeed = 2.0f;            //카메라가 캐릭터를 따라가는 스피드
    public Transform Target;                    //타겟 트랜스폼

    private Transform camTransfrom;             //카메라의 트랜스폼

    public float shakeDruation = 0.0f;
    public float shakeAmount = 0.1f;
    public float decreaseFactor = 1.0f;

    Vector3 originalPos;                        //원본 위치 

    private void OnEnable()
    {
        //originalPos = camTransfrom.localPosition;
    }
    void Update()
    {
        Vector3 newPosition = Target.position;
        newPosition.z = -10;
        transform.position = Vector3.Slerp(transform.position, newPosition, FollowSpeed * Time.deltaTime);
    }
}
