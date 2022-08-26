using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CameraController : MonoBehaviour
{
    private struct InsideRect
    {
        private Rect rect;

        // 마우스 포지션이 외곽에 있는가?
        public bool IsInside => rect.Contains(Input.mousePosition);

        public InsideRect(float ratio)
        {
            // 마우스가 가장자리로 갔을 때 화면을 움직이기 위해 안쪽 사각형을 계산함.
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

            // 왼쪽 가장자리.
            if (mousePos.x < rect.x)
                vector.x += -1;

            // 오른쪽 가장자리.
            if (mousePos.x > rect.x + rect.width)
                vector.x += 1;

            // 위쪽 가장자리.
            if (mousePos.y < rect.y)
                vector.z -= 1;

            // 아래쪽 가장자리.
            if (mousePos.y > rect.y + rect.height)
                vector.z += 1;

            return vector;            
        }
    }

    [SerializeField] Transform player;  // 대상 플레이어.
    [SerializeField] float moveSpeed;   // 카메라 이동 속도.
    [SerializeField] float zoomOffset;  // 줌을 했을때의 정도.
    [SerializeField] float minZoom;     // 최소 줌 거리.
    [SerializeField] float maxZoom;     // 최대 줌 거리.
    
    private Vector3 direction;          // 카메라가 목표점으로부터 멀어질 방향.
    private float distance;             // 카메라가 목표점으로부터 멀어질 거리.
    private InsideRect insideRect;      // 화면 안쪽 사각형. (마우스가 자유롭게 움직일 범위).
    private new Transform transform;    // 나의 트랜스폼.

    Vector3 camPivot;

    bool isFixPlayer;   // 플레이어를 화면 중앙으로 고정한다.
        
    private void Start()
    {
        transform = base.transform;    // 나의 트랜스폼을 캐싱.

        distance = Mathf.Cos(transform.rotation.x) * transform.position.y;        // 기준점에서 카메라까지의 방향.
        direction = transform.forward * -1f;                                      // 기준점 까지의 거리
        camPivot = transform.position + (direction * -1f * distance);             // 카메라의 기준점.

        // 마우스가 가장자리로 갔을 때 화면을 움직이기 위해 안쪽 사각형을 계산함.
        // 5% 비율로 설정.
        insideRect = new InsideRect(5f);

        // 윈도우에 마우스를 가둔다.
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    void Update()
    {
        // 게임을 일시정지 했을때 동작하지 않는다.
        if (GameManager.isPause)
            return;

        // 플레이어를 따라간다.
        // 플레이어를 따라가지 않는다면 Edge를 이용한다.
        if(!OnFocusPlayer())
            OnMouseEdge();

        // 마우스 휠을 이용한 확대,축소.
        OnZoomInOut();

        // 카메라의 최종 위치.
        transform.position = camPivot + (direction * distance);
    }

    private bool OnFocusPlayer()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.H))
            isFixPlayer = !isFixPlayer;

        if (Input.GetKey(KeyCode.Space) || isFixPlayer)
        {
            // 기준점의 x,z축 값을 플레이어의 값과 동일하게 맞춘다.
            camPivot.x = player.position.x;
            camPivot.z = player.position.z;
            return true;
        }

        return false;
    }
    private void OnZoomInOut()
    {
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        // 위로 올렸다. (Zoom In)
        if(wheel > 0f)
        {
            distance = Mathf.Clamp(distance - zoomOffset, minZoom, maxZoom);
        }
        // 아래로 내렸다. (Zoom out)
        else if(wheel < 0f)
        {
            distance = Mathf.Clamp(distance + zoomOffset, minZoom, maxZoom);
        }
    }
    private void OnMouseEdge()
    {
        // 만약 마우스 포인터가 화면 안에 있다면..
        if (insideRect.IsInside)
            return;

        // 가장자리를 기준으로 움직일 방향을 가져온다.
        Vector3 vector = insideRect.GetEdgeVector();

        // 카메라의 기준점을 vector방향으로 이동 시킨다.
        camPivot += (vector * moveSpeed * Time.deltaTime);
    }
}
