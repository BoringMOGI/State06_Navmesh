using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PPlayer : MonoBehaviour, IPunObservable
{
    [SerializeField] float moveSpeed;

    PhotonView pv;
    SpriteRenderer spriteRenderer;
    Sprite[] sprites;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sprites = Resources.LoadAll<Sprite>("Room/Ghost");
    }

    void Update()
    {
        if (!pv.IsMine)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        transform.Translate(new Vector2(x, y) * moveSpeed * Time.deltaTime, Space.World);

        // 방향 전환.
        bool isbeforeFlipX = spriteRenderer.flipX;

        if (x < 0)
            spriteRenderer.flipX = false;
        else if (x > 0)
            spriteRenderer.flipX = true;

        if (isbeforeFlipX != spriteRenderer.flipX)
            pv.RPC(nameof(FlipX), RpcTarget.Others, spriteRenderer.flipX);
    }


    [PunRPC]
    private void FlipX(bool isFlip)
    {
        // 오브젝트의 원본이 그림자인 나에게 보내는 명령어.
        spriteRenderer.flipX = isFlip;
    }
}
