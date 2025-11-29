using Fantasy;
using Fantasy.Async;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.Lobby.TeamHandler;

public class JoinTeamRequestHandler : MessageRPC<JoinTeamRequest , JoinTeamResponse>
{
    protected override async FTask Run(Session session, JoinTeamRequest request, JoinTeamResponse response, Action reply)
    {
        Log.Info("收到加入队伍请求 玩家ID：" + request.playerId + " 队伍ID：" + request.teamId);
        var lobbyPlayerManagerComponent = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        var res = lobbyPlayerManagerComponent.JoinTeam(request.playerId, request.teamId);

        response.ErrorCode = res.errorCode;
        if (response.ErrorCode != 0)
        {
            reply();
            return;
        }
        
        Log.Info("加入队伍成功 玩家ID：" + request.playerId + " 队伍ID：" + request.teamId);
        response.teamOwnerId = res.team.TeamOwner.memberAccountId;
        response.teamId = res.team.TeamId;
        
        //包括玩家自身
        response.teamMemberIds = res.team.TeamMembers.Select(x => x.memberAccountId).ToList();
        await FTask.CompletedTask;
    }
}