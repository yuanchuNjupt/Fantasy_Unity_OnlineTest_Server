using Fantasy;
using Fantasy.Async;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.Lobby;

public class StateSyncRequestHandler : MessageRPC<StateSyncRequest, StateSyncResponse>
{
    protected override async FTask Run(Session session, StateSyncRequest request, StateSyncResponse response,
        Action reply)
    {
        var lobbyPlayerManager = session.Scene.GetComponent<LobbyPlayerManagerComponent>();

        //hyw?
        var res = lobbyPlayerManager.PlayerMove(request.stateData);

        response.ErrorCode = res.errorCode;
        if (response.ErrorCode != 0)
        {
            Log.Debug("玩家状态同步失败，玩家ID:" + request.stateData.playerId);
            return;
        }

        response.stateData = res.syncData;

        //将玩家状态同步数据广播给其他玩家
        var otherPlayers = lobbyPlayerManager.GetLobbyPlayers(request.stateData.playerId);
        OtherPlayerStateSyncMessage message = new OtherPlayerStateSyncMessage();
        message.roleData = res.syncData;

        if (otherPlayers.Count() == 0)
        {
            Log.Debug("当前没有其他玩家在线，无需广播状态同步消息");
            return;
        }

        foreach (var otherPlayer in otherPlayers)
        {
            if (otherPlayer != null)
            {
                otherPlayer.Session.Send(message);
            }
        }


        await FTask.CompletedTask;
    }
}