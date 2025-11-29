using Fantasy;
using Fantasy.Async;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.Lobby.TeamHandler;

public class TeamStateChangeMessageHandler : Message<TeamStateChangeMessage>
{
    protected override async FTask Run(Session session, TeamStateChangeMessage message)
    {

        var lobbyPlayerManager = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        
        
        switch (message.teamState)
        {
            case 2:
                //退出队伍
                lobbyPlayerManager.LevelTeam(message.playerId);
                break;
            case 3:
                //解散队伍
                lobbyPlayerManager.RemoveTeam(message.playerId);
                break;
        }




        await FTask.CompletedTask;
    }
}