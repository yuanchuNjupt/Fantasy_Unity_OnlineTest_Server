using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Entitas.Interface;
using Fantasy.Lobby;

namespace Hotfix.System;


public class SessionDisposeCallBack : DestroySystem<SessionDisposeComponent>
{
    protected override async void Destroy(SessionDisposeComponent self)
    {
        var lobbyPlayerManager = self.Scene.GetComponent<LobbyPlayerManagerComponent>();
        var res = lobbyPlayerManager.RemovePlayer(self.AccountId);
    
        if (res.errorCode != 0)
        {
            Log.Debug("玩家下线失败，玩家ID:" + self.AccountId);
            return;
        }
        
        //已经把当前玩家从所有玩家列表中移除了，所以无需过滤
        var otherPlayers = lobbyPlayerManager.GetLobbyPlayers();

        if (otherPlayers.Count() == 0)
        {
            Log.Debug("当前服务器上没有其他玩家在线，无需广播下线消息");
            
            //保存Account数据到数据库（包含Role数据）
            if (res.account != null)
            {
                await SaveAccountData(res.account, self.Scene);
                Log.Debug($"玩家ID:{self.AccountId} 下线成功 , 下线位置:{res.account.role?.LastPosition}");
            }
            else
            {
                Log.Debug($"玩家ID:{self.AccountId} 下线成功");
            }
            
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
            OtherPlayerLogoutMessage logoutMsg = new OtherPlayerLogoutMessage();
            logoutMsg.playerId = self.AccountId;
            otherPlayer.Session.Send(logoutMsg);
            Log.Debug($"向玩家ID:{otherPlayer.AccountId} 发送玩家ID:{self.AccountId}下线消息");
        }
        
        //保存Account数据到数据库（包含Role数据）
        if (res.account != null)
        {
            await SaveAccountData(res.account, self.Scene);
            Log.Debug($"玩家ID:{self.AccountId} 下线成功 , 下线位置:{res.account.role?.LastPosition}");
        }
        else
        {
            Log.Debug($"玩家ID:{self.AccountId} 下线成功");
        }
    }
    
    
    private async FTask SaveAccountData(Account account, Scene scene)
    {
        var dataBase = scene.World.Database;
        await dataBase.Save(account);
        Log.Debug($"账号ID:{account.Id} 数据已保存到数据库");
    }
}

public static class SessionDisposeComponentSystem
{
    
    
    
    
}