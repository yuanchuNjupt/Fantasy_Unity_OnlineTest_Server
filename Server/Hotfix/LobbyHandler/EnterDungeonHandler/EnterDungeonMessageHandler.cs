using Fantasy;
using Fantasy.Async;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.LobbyHandler.EnterDungeonHandler;

public class EnterDungeonMessageHandler : Message<EnterDungeonMessage>
{
    protected override async FTask Run(Session session, EnterDungeonMessage message)
    {
        //收到了进入副本的消息
        
        var lobbyPlayerComponent = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        //得到玩家的副本信息
        var teamMembers = lobbyPlayerComponent.GetTeamMemberIds(message.teamId);
        if(teamMembers == null || teamMembers.Count == 0)
        {
            Log.Error("EnterDungeonMessageHandler: No team members found for teamId " + message.teamId);
            return;
        }
        
        //寻找到这个队伍中所有玩家的Session，发送进入副本的消息
        var teamSessions = lobbyPlayerComponent.GetLobbyPlayersByIds(teamMembers)?.Select(x => x.Session).ToList();
        
        
        teamSessions?.ForEach(x => x.Send(message));
        
        //至于各个玩家的加载进度，在LoadDungeonProgressMessageHandler中处理
        Log.Info($"收到进入副本消息，队伍ID：{message.teamId}。队伍人数：{message.teamMemberIds.Count}，已通知队伍中的玩家进入副本。");
        await FTask.CompletedTask;
    }
}