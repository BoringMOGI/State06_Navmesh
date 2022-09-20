using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomChatUI : ChatUI
{
    protected override void ConnectToChatServer()
    {
        // 룸에서는 시작시 자동으로 접속하지 않게 한다.
    }

    public void OnJoinedRoom(string userName, string roomName)
    {
        this.userName = userName;

        // 서버에 접속 시도!!
        server.ConnectToServer(userName, () => {

            // 서버에 접속이 완료되면 Local채널을 추가한다.
            OnAddChannel(roomName, true);
        });
    }
}
