using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public Camera mainCamera;                               //���� ī�޶� ���� (Raycast ���)
    public LineRenderer lineRenderer;                       //�׷��ø� ������ �׸��� ���� LineRenderer
    public DistanceJoint2D distanceJoint;                   //�׷��ø� ������ ���� DistanceJoint2D

    public LayerMask grappableLayerMask;                    //�׷��ø� ������ ���̾� ����ũ
    public LayerMask playerLayer;                           //�÷��̾� ���̾� ����ũ 

    public float maxDistance = 10.0f;                       //�ִ� �Ÿ�
    public float grappleSpeed = 5.0f;                       //�׷��� ����Ʈ�� ������� �ӵ�
    public float hookTravelSpeed = 15f;                     //���� ���ư��� �ӵ�

    private bool isGrappling = false;                       //���� �׷��ø� ������ ����
    private Vector2 grapplePoint;                           //�׷��ø� ����Ʈ ��ġ
    private Vector2 hookPosition;                           //�� ���� ��ġ
    private bool isHookTraveling = false;                   //���� ���ư��� ������ ����

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

        grappableLayerMask |= 1 << LayerMask.NameToLayer("Default");        //Default ���̾ grappableLayerMask�� �߰�
        grappableLayerMask &= ~playerLayer;                                 //�÷��̾� ���̾ ����

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))             //���콺 �Է� ó�� 
        {
            StartGrapple();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        if(isHookTraveling)                             //���� ���ư��� ���̸� ��ġ ������Ʈ
        {
            UpdateHookPosition();
        }

        if (isGrappling || isHookTraveling)             //�׷��ø� ���̰ų� ���� ���ư��� ���̸� LineRenderer ������Ʈ
        {
            UpdateLineRenderer();
        }
    }

    private void FixedUpdate()
    {
        if (isGrappling && !isHookTraveling)             //�׷��ø� ���̰� ���� ��ǥ�� ���������� �÷��̾� �̵� ó��
        {
            UpdateGrapple();
        }
    }

    void StartGrapple()
    {
        //���콺 ��ġ�� ����ĳ��Ʈ �߻�
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, grappableLayerMask);

        if(hit.collider != null)    //�����ɽ�Ʈ�� Hit ������
        {
            isHookTraveling = true;
            grapplePoint = hit.point;
            hookPosition = transform.position;
        }
        else
        {
            Debug.Log("��Ʈ �ȵ�");
        }
    }

    void UpdateHookPosition()
    {
        //���� �׷��� ����Ʈ �������� �̵�
        hookPosition = Vector2.MoveTowards(hookPosition, grapplePoint, hookTravelSpeed * Time.deltaTime);

        //���� �׷��� ����Ʈ�� ���� �ߴ��� üũ
        if(Vector2.Distance(hookPosition, grapplePoint) < 0.1f)
        {   //���� �Ϸ� �ߴٸ� ���� ������ �Է� ���ش�. 
            isHookTraveling = false;
            isGrappling = true;
            distanceJoint.connectedAnchor = grapplePoint;
            distanceJoint.distance = Vector2.Distance(transform.position, grapplePoint);
            distanceJoint.enabled = true;
        }
    }

    void StopGrapple()
    {
        //�׷��ø� ���� �� ���� ���� ����
        isGrappling = false;
        isHookTraveling = false;
        distanceJoint.enabled = false;
        lineRenderer.enabled = false;
    }

    void UpdateGrapple()
    {
        //�÷��̾ �׷��� ����Ʈ �������� �̵�
        Vector2 grappleDir = (grapplePoint - (Vector2)transform.position).normalized;
        GetComponent<Rigidbody2D>().AddForce(grappleDir * grappleSpeed);

        //�÷��̾ �׷��� ����Ʈ�� ���� �ߴ��� üũ
        if(Vector2.Distance(transform.position, grapplePoint) < 0.1f)
        {
            StopGrapple();
        }
    }

    void UpdateLineRenderer()
    {
        //LineRenderer�� ����ؼ� �׷��ø� ���� �׸���
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, isHookTraveling ? hookPosition : grapplePoint);
    }
}
