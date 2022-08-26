using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CameraController : MonoBehaviour
{
    private struct InsideRect
    {
        private Rect rect;

        // ���콺 �������� �ܰ��� �ִ°�?
        public bool IsInside => rect.Contains(Input.mousePosition);

        public InsideRect(float ratio)
        {
            // ���콺�� �����ڸ��� ���� �� ȭ���� �����̱� ���� ���� �簢���� �����.
            rect = new Rect();
            rect.x = Screen.width * ratio * 0.01f;
            rect.y = Screen.height * ratio * 0.01f;
            rect.width = Screen.width - rect.x * 2f;
            rect.height = Screen.height - rect.y * 2f;
        }
        public Vector3 GetEdgeVector()
        {
            Vector3 vector = Vector2.zero;
            Vector3 mousePos = Input.mousePosition;

            // ���� �����ڸ�.
            if (mousePos.x < rect.x)
                vector.x += -1;

            // ������ �����ڸ�.
            if (mousePos.x > rect.x + rect.width)
                vector.x += 1;

            // ���� �����ڸ�.
            if (mousePos.y < rect.y)
                vector.z -= 1;

            // �Ʒ��� �����ڸ�.
            if (mousePos.y > rect.y + rect.height)
                vector.z += 1;

            return vector;            
        }
    }

    [SerializeField] Transform player;  // ��� �÷��̾�.
    [SerializeField] float moveSpeed;   // ī�޶� �̵� �ӵ�.
    [SerializeField] float zoomOffset;  // ���� �������� ����.
    [SerializeField] float minZoom;     // �ּ� �� �Ÿ�.
    [SerializeField] float maxZoom;     // �ִ� �� �Ÿ�.
    
    private Vector3 direction;          // ī�޶� ��ǥ�����κ��� �־��� ����.
    private float distance;             // ī�޶� ��ǥ�����κ��� �־��� �Ÿ�.
    private InsideRect insideRect;      // ȭ�� ���� �簢��. (���콺�� �����Ӱ� ������ ����).
    private new Transform transform;    // ���� Ʈ������.

    Vector3 camPivot;

    bool isFixPlayer;   // �÷��̾ ȭ�� �߾����� �����Ѵ�.
        
    private void Start()
    {
        transform = base.transform;    // ���� Ʈ�������� ĳ��.

        distance = Mathf.Cos(transform.rotation.x) * transform.position.y;        // ���������� ī�޶������ ����.
        direction = transform.forward * -1f;                                      // ������ ������ �Ÿ�
        camPivot = transform.position + (direction * -1f * distance);             // ī�޶��� ������.

        // ���콺�� �����ڸ��� ���� �� ȭ���� �����̱� ���� ���� �簢���� �����.
        // 5% ������ ����.
        insideRect = new InsideRect(5f);

        // �����쿡 ���콺�� ���д�.
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    void Update()
    {
        // ������ �Ͻ����� ������ �������� �ʴ´�.
        if (GameManager.isPause)
            return;

        // �÷��̾ ���󰣴�.
        // �÷��̾ ������ �ʴ´ٸ� Edge�� �̿��Ѵ�.
        if(!OnFocusPlayer())
            OnMouseEdge();

        // ���콺 ���� �̿��� Ȯ��,���.
        OnZoomInOut();

        // ī�޶��� ���� ��ġ.
        transform.position = camPivot + (direction * distance);
    }

    private bool OnFocusPlayer()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.H))
            isFixPlayer = !isFixPlayer;

        if (Input.GetKey(KeyCode.Space) || isFixPlayer)
        {
            // �������� x,z�� ���� �÷��̾��� ���� �����ϰ� �����.
            camPivot.x = player.position.x;
            camPivot.z = player.position.z;
            return true;
        }

        return false;
    }
    private void OnZoomInOut()
    {
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        // ���� �÷ȴ�. (Zoom In)
        if(wheel > 0f)
        {
            distance = Mathf.Clamp(distance - zoomOffset, minZoom, maxZoom);
        }
        // �Ʒ��� ���ȴ�. (Zoom out)
        else if(wheel < 0f)
        {
            distance = Mathf.Clamp(distance + zoomOffset, minZoom, maxZoom);
        }
    }
    private void OnMouseEdge()
    {
        // ���� ���콺 �����Ͱ� ȭ�� �ȿ� �ִٸ�..
        if (insideRect.IsInside)
            return;

        // �����ڸ��� �������� ������ ������ �����´�.
        Vector3 vector = insideRect.GetEdgeVector();

        // ī�޶��� �������� vector�������� �̵� ��Ų��.
        camPivot += (vector * moveSpeed * Time.deltaTime);
    }
}
