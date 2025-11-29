using Fantasy;
using Fantasy.Async;
using Fantasy.Entitas;
using Fantasy.IdFactory;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.Lobby.TeamHandler;

public class CreateTeamRequestHandler : MessageRPC<CreateTeamRequest , CreateTeamResponse>
{
    protected override async FTask Run(Session session, CreateTeamRequest request, CreateTeamResponse response, Action reply)
    {
        //此时玩家一定在大厅里且在线，否则不会有此请求
        //获取此前玩家的数据
        var lobbyPlayerManagerComponent = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        var res = lobbyPlayerManagerComponent.CreateTeam(request.playerId);

        response.ErrorCode = res.errorCode;
        if (res.errorCode != 0)
        {
            Log.Error("创建队伍失败 错误码：" + res.errorCode);
            reply();
            return;
        }
        
        //创建队伍成功
        response.teamId = res.teamId;
        response.playerId = request.playerId;
        Log.Info("创建队伍成功 队伍ID：" + res.teamId + " 玩家ID：" + request.playerId);
        await FTask.CompletedTask;
    }
}