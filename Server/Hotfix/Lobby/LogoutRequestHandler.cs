using Fantasy;
using Fantasy.Async;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.Lobby;

public class LogoutRequestHandler : Message<LogoutMessage>
{
    protected override async FTask Run(Session session, LogoutMessage message)
    {
        var lobbyPlayerManager = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        uint errorCode = lobbyPlayerManager.RemovePlayer(message.playerId);
    
        if (errorCode != 0)
        {
            Log.Debug("玩家下线失败，玩家ID:" + message.playerId);
            return;
        }
        
        //已经把当前玩家从所有玩家列表中移除了，所以无需过滤
        var otherPlayers = lobbyPlayerManager.GetLobbyPlayers();

        if (otherPlayers.Count() == 0)
        {
            Log.Debug("当前服务器上没有其他玩家在线，无需广播下线消息");
            Log.Debug($"玩家ID:{message.playerId} 下线成功");
            return;
        }
    
        foreach (var otherPlayer in otherPlayers)
        {
            // 检查 Session 是否有效
            if (otherPlayer.Session == null || otherPlayer.Session.IsDisposed)
            {
                Log.Debug($"玩家ID:{otherPlayer.AccountId} 的Session已断开，跳过发送下线消息");
                continue;
            }
            
            //向其他玩家广播玩家下线消息
            OtherPlayerLogoutMessage logoutMsg = OtherPlayerLogoutMessage.Create(session.Scene);
            logoutMsg.playerId = message.playerId;
            otherPlayer.Session.Send(logoutMsg);
            Log.Debug($"向玩家ID:{otherPlayer.AccountId} 发送玩家ID:{message.playerId}下线消息");
        }
        
        
        
        //TODO:处理玩家下线逻辑 比如保存数据等
        
        
        
        
        
        
        
        Log.Debug($"玩家ID:{message.playerId} 下线成功");
    
        await FTask.CompletedTask;
    }

    
        
}