using Fantasy;
using Fantasy.Async;
using Fantasy.Dungeons;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;
using Hotfix.System.Dungeons;

namespace Hotfix.LobbyHandler.EnterDungeonHandler;

public class LoadDungeonProgressMessageHandler : Message<LoadDungeonProgressMessage>
{
    protected override async FTask Run(Session session, LoadDungeonProgressMessage message)
    {
        var lobbyPlayerComponent = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        
        Log.Info("收到加载进度消息 队伍："+ message.teamId + " 玩家ID：" + message.playerId + " 进度：" + message.progress);
        bool allLoadComplete =
            lobbyPlayerComponent.SyncLoadingProgress(message.teamId, message.playerId, message.progress);
        
        if (allLoadComplete)
        {
            Log.Info("队伍：" + message.teamId + " 所有玩家加载完成 可以进入战斗场景");
            //通知所有玩家加载完成 可以进入战斗场景

            var battleComponent = session.Scene.GetComponent<BattleManagerComponent>();
            
            //延迟1秒 保证完全加载
            session.Scene.TimerComponent.Net.OnceTimer(1000, () =>
            {
                var teamLobbyPlayers = lobbyPlayerComponent
                    .GetLobbyPlayersByIds(lobbyPlayerComponent.GetTeamMemberIds(message.teamId));
                //移除加载进度缓存
                lobbyPlayerComponent.ClearTeamLoadProgress(message.teamId);
                
                //通知开始战斗
                battleComponent.StartBattle(teamLobbyPlayers);
                
            });
        }


        await FTask.CompletedTask;
    }
}