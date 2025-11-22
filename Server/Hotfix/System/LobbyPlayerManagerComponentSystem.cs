using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Database;
using Fantasy.Entitas;
using Fantasy.Lobby;
using Fantasy.Network;
using Hotfix.Helper;

namespace Hotfix.System;

public static class LobbyPlayerManagerComponentSystem
{
    public static async FTask<uint> AddPlayer(this LobbyPlayerManagerComponent self , Session session , long playerId)
    {
        var player = Entity.Create<LobbyPlayer>(self.Scene , true , true);
        if (self.LobbyPlayers.ContainsKey(playerId))
        {
            Log.Debug("玩家已存在，无法添加，玩家ID:" + playerId);
            return ErrorCode.PLAYER_ADD_FAILED;
        }
        
        player.AccountId = playerId;  // 设置账号ID
        player.Session = session;
        //向数据库中获取玩家角色数据
        IDatabase dataBase = self.Scene.World.Database;

        Role role = await dataBase.First<Role>(x => x.AccountId == playerId);
        if (role == null)
        {
            
            //一般来说不太可能出现这种情况
            Log.Debug("数据库中不存在该玩家角色数据，玩家ID:" + playerId);
            return ErrorCode.PLAYER_ADD_FAILED;
        }

        player.role = role;
        self.LobbyPlayers.Add(playerId , player);
        
        return ErrorCode.SUCCESS;
    }
    
    public static IEnumerable<LobbyPlayer?> GetLobbyPlayers(this LobbyPlayerManagerComponent self , long filterId = 0)
    {
        return self.LobbyPlayers.Values.Where(x => x.AccountId != filterId);  // 使用 AccountId 进行过滤
    }
    
    // public static IEnumerable<long> GetLobbyPlayersId(this LobbyPlayerManagerComponent self , long filterId = 0)
    // {
    //     return self.LobbyPlayers.Keys.Where(x => x != filterId);
    // }
    
    public static uint RemovePlayer(this LobbyPlayerManagerComponent self , long playerId)
    {
        if (self.LobbyPlayers.ContainsKey(playerId))
        {
            var player = self.LobbyPlayers[playerId];
            player.Dispose();
            self.LobbyPlayers.Remove(playerId);
            return ErrorCode.SUCCESS;
        }
        else
        {
            Log.Debug("不存在玩家ID : " + playerId);
            return ErrorCode.PLAYER_REMOVE_FAILED;
        }
    }

    public static (stateSyncData? syncData, uint errorCode) PlayerMove(this LobbyPlayerManagerComponent self,
        stateSyncData syncData)
    {
        //获取这个玩家
        if (!self.LobbyPlayers.TryGetValue(syncData.playerId, out var player))
        {
            Log.Debug("不存在玩家ID : " + syncData.playerId);
            return (null, ErrorCode.PLAYER_NOT_FOUND);
        }

        player.Position.x += syncData.inputDir.x * self.FixedDeltaTime * player.role.moveSpeed;
        player.Position.y += syncData.inputDir.y * self.FixedDeltaTime * player.role.moveSpeed;
        player.Position.z += syncData.inputDir.z * self.FixedDeltaTime * player.role.moveSpeed;

        if (syncData.inputDir.x != 0 || syncData.inputDir.y != 0 || syncData.inputDir.z != 0)
        {
            player.RenderDir = syncData.inputDir.ToVector3();
            Log.Info("玩家ID:" + syncData.playerId + " 移动方向: " + syncData.inputDir.x + " , " + syncData.inputDir.y + " , " + syncData.inputDir.z);
        }
        
        //更新状态数据
        syncData.position = player.Position.ToCSVector3();
        Log.Info("玩家ID:" + syncData.playerId + " 移动到新位置: " + syncData.position.x + " , " + syncData.position.y + " , " + syncData.position.z);
        
        return (syncData, ErrorCode.SUCCESS);
    }
}