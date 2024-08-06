using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public Camera mainCamera;                               //메인 카메라 참조 (Raycast 사용)
    public LineRenderer lineRenderer;                       //그래플링 로프를 그리기 위한 LineRenderer
    public DistanceJoint2D distanceJoint;                   //그래플링 동작을 위한 DistanceJoint2D

    public LayerMask grappableLayerMask;                    //그래플링 가능한 레이어 마스크
    public LayerMask playerLayer;                           //플레이어 레이어 마스크 

    public float maxDistance = 10.0f;                       //최대 거리
    public float grappleSpeed = 5.0f;                       //그래플 포인트로 당겨지는 속도
    public float hookTravelSpeed = 15f;                     //훅이 날아가는 속도

    private bool isGrappling = false;                       //현재 그래플링 중인지 여부
    private Vector2 grapplePoint;                           //그래플링 포인트 위치
    private Vector2 hookPosition;                           //훅 현재 위치
    private bool isHookTraveling = false;                   //훅이 날아가는 중인지 여부

    // Start is called before the first frame update
    void Start()
    {
        if(mainCamera == null)
            mainCamera = Camera.main;

        if(distanceJoint == null)
            distanceJoint = GetComponent<DistanceJoint2D>();

        distanceJoint.autoConfigureDistance = false; ;
        distanceJoint.maxDistanceOnly = true;
        distanceJoint.enabled = false;

        if(lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;

        grappableLayerMask |= 1 << LayerMask.NameToLayer("Default");        //Default 레이어를 grappableLayerMask에 추가
        grappableLayerMask &= ~playerLayer;                                 //플레이어 레이어를 제외

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))             //마우스 입력 처리 
        {
            StartGrapple();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        if(isHookTraveling)                             //훅이 날아가는 중이면 위치 업데이트
        {
            UpdateHookPosition();
        }

        if (isGrappling || isHookTraveling)             //그래플링 중이거나 훅이 날아가는 중이면 LineRenderer 업데이트
        {
            UpdateLineRenderer();
        }
    }

    private void FixedUpdate()
    {
        if (isGrappling && !isHookTraveling)             //그래플링 중이고 훅이 목표에 도달했으면 플레이어 이동 처리
        {
            UpdateGrapple();
        }
    }

    void StartGrapple()
    {
        //마우스 위치로 레이캐스트 발사
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, grappableLayerMask);

        if(hit.collider != null)    //레이케스트가 Hit 했으면
        {
            isHookTraveling = true;
            grapplePoint = hit.point;
            hookPosition = transform.position;
        }
        else
        {
            Debug.Log("히트 안됨");
        }
    }

    void UpdateHookPosition()
    {
        //훅을 그래플 포인트 방향으로 이동
        hookPosition = Vector2.MoveTowards(hookPosition, grapplePoint, hookTravelSpeed * Time.deltaTime);

        //훅이 그래플 포인트에 도달 했는지 체크
        if(Vector2.Distance(hookPosition, grapplePoint) < 0.1f)
        {   //도달 완료 했다면 설정 값들을 입력 해준다. 
            isHookTraveling = false;
            isGrappling = true;
            distanceJoint.connectedAnchor = grapplePoint;
            distanceJoint.distance = Vector2.Distance(transform.position, grapplePoint);
            distanceJoint.enabled = true;
        }
    }

    void StopGrapple()
    {
        //그래플링 정지 및 관련 변수 리셋
        isGrappling = false;
        isHookTraveling = false;
        distanceJoint.enabled = false;
        lineRenderer.enabled = false;
    }

    void UpdateGrapple()
    {
        //플레이어를 그래플 포인트 방향으로 이동
        Vector2 grappleDir = (grapplePoint - (Vector2)transform.position).normalized;
        GetComponent<Rigidbody2D>().AddForce(grappleDir * grappleSpeed);

        //플레이어가 그래플 포인트에 도달 했는지 체크
        if(Vector2.Distance(transform.position, grapplePoint) < 0.1f)
        {
            StopGrapple();
        }
    }

    void UpdateLineRenderer()
    {
        //LineRenderer를 사용해서 그래플링 로프 그리기
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, isHookTraveling ? hookPosition : grapplePoint);
    }
}
