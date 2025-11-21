using Fantasy;
using Fantasy.Entitas;
using Fantasy.Lobby;
using Fantasy.Network;
using Hotfix.Helper;

namespace Hotfix.System;

public static class LobbyPlayerManagerComponentSystem
{
    public static (long id , uint errorCode) AddPlayer(this LobbyPlayerManagerComponent self , Session session)
    {
        var player = Entity.Create<LobbyPlayer>(self.Scene , true , true);
        if (self.LobbyPlayers.ContainsKey(player.Id))
        {
            Log.Debug("玩家已存在，无法添加，玩家ID:" + player.Id);
            return (player.Id, ErrorCode.PLAYER_ADD_FAILED);
        }
        
        player.Session = session;
        self.LobbyPlayers.Add(player.Id , player);
        return (player.Id , ErrorCode.SUCCESS);
    }
    
    public static IEnumerable<LobbyPlayer?> GetLobbyPlayers(this LobbyPlayerManagerComponent self , long filterId = 0)
    {
        return self.LobbyPlayers.Values.Where(x => x.Id != filterId);
    }
    
    public static IEnumerable<long> GetLobbyPlayersId(this LobbyPlayerManagerComponent self , long filterId = 0)
    {
        return self.LobbyPlayers.Keys.Where(x => x != filterId);
    }
    
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
        long playerId, stateSyncData syncData)
    {
        //获取这个玩家
        if (!self.LobbyPlayers.TryGetValue(playerId, out var player))
        {
            Log.Debug("不存在玩家ID : " + playerId);
            return (null, ErrorCode.PLAYER_NOT_FOUND);
        }

        player.Position.x += syncData.inputDir.x * self.FixedDeltaTime * player.moveSpeed;
        player.Position.y += syncData.inputDir.y * self.FixedDeltaTime * player.moveSpeed;
        player.Position.z += syncData.inputDir.z * self.FixedDeltaTime * player.moveSpeed;

        if (syncData.inputDir.x != 0 || syncData.inputDir.y != 0 || syncData.inputDir.z != 0)
        {
            player.RenderDir = syncData.inputDir.ToVector3();
            Log.Info("玩家ID:" + playerId + " 移动方向: " + syncData.inputDir.x + " , " + syncData.inputDir.y + " , " + syncData.inputDir.z);
        }
        
        //更新状态数据
        syncData.position = player.Position.ToCSVector3();
        Log.Info("玩家ID:" + playerId + " 移动到新位置: " + syncData.position.x + " , " + syncData.position.y + " , " + syncData.position.z);
        
        return (syncData, ErrorCode.SUCCESS);
    }
}