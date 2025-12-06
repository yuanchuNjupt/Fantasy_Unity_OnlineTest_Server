using Fantasy;
using Fantasy.Async;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

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
            var enterDungeonCompleteMessage = new StartDungeonBattleMessage();
            //发送给队伍中的所有玩家
            lobbyPlayerComponent
                .GetLobbyPlayersByIds(lobbyPlayerComponent.GetTeamMemberIds(message.teamId)).Select(x => x.Session)
                .ToList().ForEach(x => x.Send(enterDungeonCompleteMessage));
            
            //移除加载进度缓存
            lobbyPlayerComponent.ClearTeamLoadProgress(message.teamId);
            
            
            
            
        }


        await FTask.CompletedTask;
    }
}