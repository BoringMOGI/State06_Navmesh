using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomChatUI : ChatUI
{
    protected override void ConnectToChatServer()
    {
        // �뿡���� ���۽� �ڵ����� �������� �ʰ� �Ѵ�.
    }

    public void OnJoinedRoom(string userName, string roomName)
    {
        this.userName = userName;

        // ������ ���� �õ�!!
        server.ConnectToServer(userName, () => {

            // ������ ������ �Ϸ�Ǹ� Localä���� �߰��Ѵ�.
            OnAddChannel(roomName, true);
        });
    }
}
