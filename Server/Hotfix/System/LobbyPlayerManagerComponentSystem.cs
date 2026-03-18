using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Entitas;
using Fantasy.Lobby;
using Fantasy.Network;
using Hotfix.Helper;

namespace Hotfix.System;

public static class LobbyPlayerManagerComponentSystem
{
    
    /// <summary>
    /// 添加大厅中的玩家
    /// </summary>
    /// <param name="self"></param>
    /// <param name="session"></param>
    /// <param name="playerId"></param>
    /// <returns></returns>
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
        
        //从AuthenticationAccountComponent缓存中获取Account（包含Role）
        var authComponent = self.Scene.GetComponent<AuthenticationAccountComponent>();
        Account? account = authComponent.AccountCache.Values.FirstOrDefault(x => x.Id == playerId);
        
        if (account == null || account.role == null)
        {
            Log.Debug("缓存中不存在该账号或角色数据，玩家ID:" + playerId);
            return ErrorCode.PLAYER_ADD_FAILED;
        }

        player.role = account.role;
        
        //使用角色的上次下线位置和朝向初始化玩家位置
        player.Position = account.role.LastPosition;
        player.RenderDir = account.role.LastRenderDir;
        
        self.LobbyPlayers.Add(playerId , player);
        
        await FTask.CompletedTask;
        return ErrorCode.SUCCESS;
    }
    
    /// <summary>
    /// 得到大厅中的所有玩家列表， 可选过滤掉指定ID的玩家
    /// </summary>
    /// <param name="self"></param>
    /// <param name="filterId"></param>
    /// <returns></returns>
    public static IEnumerable<LobbyPlayer?> GetLobbyPlayers(this LobbyPlayerManagerComponent self , long filterId = 0)
    {
        return self.LobbyPlayers.Values.Where(x => x.AccountId != filterId);  // 使用 AccountId 进行过滤
    }

    public static List<LobbyPlayer> GetLobbyPlayersByIds(this LobbyPlayerManagerComponent self, List<long> playerIds)
    {
        var list = self.LobbyPlayers.Values.Where(x => playerIds.Contains(x.AccountId)).ToList();
        if (list.Count != playerIds.Count)
        {
            Log.Warning("有玩家ID未找到，可能不在大厅中");
        }
        return list;
    }
    
    
    /// <summary>
    /// 移除大厅中的玩家
    /// </summary>
    /// <param name="self"></param>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public static (uint errorCode , Account? account) RemovePlayer(this LobbyPlayerManagerComponent self , long playerId)
    {
        if (self.LobbyPlayers.ContainsKey(playerId))
        {
            var player = self.LobbyPlayers[playerId];
            
            //从AuthenticationAccountComponent缓存中获取Account
            var authComponent = self.Scene.GetComponent<AuthenticationAccountComponent>();
            Account? account = authComponent.AccountCache.Values.FirstOrDefault(x => x.Id == playerId);
            
            if (account != null && account.role != null)
            {
                //更新Role的位置和朝向数据
                account.role.LastPosition = player.Position;
                account.role.LastRenderDir = player.RenderDir;
            }
            
            player.Dispose();  // 释放player
            self.LobbyPlayers.Remove(playerId);
            return (ErrorCode.SUCCESS , account);
        }
        else
        {
            Log.Debug("不存在玩家ID : " + playerId);
            return (ErrorCode.PLAYER_REMOVE_FAILED , null);
        }
    }

    
    /// <summary>
    /// 同步玩家移动状态
    /// </summary>
    /// <param name="self"></param>
    /// <param name="syncData"></param>
    /// <returns></returns>
    public static (StateSyncData? syncData, uint errorCode) PlayerMove(this LobbyPlayerManagerComponent self,
        StateSyncData syncData)
    {
        //获取这个玩家
        if (!self.LobbyPlayers.TryGetValue(syncData.playerId, out var player))
        {
            Log.Debug("不存在玩家ID : " + syncData.playerId);
            return (null, ErrorCode.PLAYER_NOT_FOUND);
        }

        if (syncData.playerState == 1)
        {
            player.PlayerState = 1;
        }
        else if (syncData.playerState == 2)
        {
            //跑步
            player.Position.x += syncData.inputDir.x * self.FixedDeltaTime * player.role.moveSpeed;
            player.Position.y += syncData.inputDir.y * self.FixedDeltaTime * player.role.moveSpeed;
            player.Position.z += syncData.inputDir.z * self.FixedDeltaTime * player.role.moveSpeed;
            player.PlayerState = 2;
        }
        else if (syncData.playerState == 3)
        {
            //冲刺
            player.Position.x += syncData.inputDir.x * self.FixedDeltaTime * player.role.sprintSpeed;
            player.Position.y += syncData.inputDir.y * self.FixedDeltaTime * player.role.sprintSpeed;
            player.Position.z += syncData.inputDir.z * self.FixedDeltaTime * player.role.sprintSpeed;
            player.PlayerState = 3;
        }
        

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


    #region 组队相关

    
    /// <summary>
    /// 队长创建一个队伍
    /// </summary>
    /// <param name="self"></param>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public static (long teamId , uint errorCode) CreateTeam(this LobbyPlayerManagerComponent self, long playerId)
    {
        if (!self.LobbyPlayers.TryGetValue(playerId, out var player))
        {
            Log.Error("当前大厅不存在此玩家！");
            return(-1 , ErrorCode.PLAYER_NOT_FOUND);
        }
        
        //创建队伍
        Team team = Entity.Create<Team>(self.Scene, true, true);
        team.TeamOwner = new TeamMemberInfo()
        {
            memberAccountId = player.AccountId,
            memberName = player.role.accountName,
        };
        team.TeamId = self.TeamIdStart++;
        self.Teams.Add(team.TeamId , team);
        
        //设置玩家当前队伍ID
        player.TeamId = team.TeamId;
        return (team.TeamId, ErrorCode.SUCCESS);
    }

    /// <summary>
    /// 玩家加入一个队伍(包括消息广播)
    /// </summary>
    /// <param name="self"></param>
    /// <param name="playerId"></param>
    /// <param name="teamId"></param>
    /// <returns></returns>
    public static (Team? team, uint errorCode) JoinTeam(this LobbyPlayerManagerComponent self, long playerId,
        long teamId)
    {
        
        //查询成员
        if (!self.LobbyPlayers.TryGetValue(playerId, out var player))
        {
            Log.Error("当前大厅不存在此玩家，玩家ID:" + playerId);
            return (null, ErrorCode.PLAYER_NOT_FOUND);
        }
        
        
        //查看队伍是否存在 
        if (!self.Teams.TryGetValue(teamId, out var team))
        {
            Log.Error("当前不存在此队伍，队伍ID:" + teamId);
            return (null, ErrorCode.TEAM_NOT_FOUND);
        }

        if (team.TeamMembers.Count >= 3)
        {
            Log.Warning("队伍人数已满，无法加入，队伍ID:" + teamId);
            return (null, ErrorCode.TEAM_MAX);
        }
        
        //设置玩家当前队伍ID
        player.TeamId = teamId;
        
        
        //在这里向队伍中的其他成员广播新成员加入消息
        TeamStateChangeMessage message = new TeamStateChangeMessage();
        message.teamState = 1; //加入
        message.playerId = playerId;

        player = self.LobbyPlayers[team.TeamOwner.memberAccountId];
        player.Session.Send(message);
        
        team.TeamMembers.ForEach(x =>
        {
            player = self.LobbyPlayers[x.memberAccountId];
            player.Session.Send(message);
        });
        
        
        //在给当前队伍中的所有人发送完加入消息后 ， 再把新成员添加到队伍中
        
        team.TeamMembers.Add(new TeamMemberInfo()
        {
            memberAccountId = playerId,
            memberName = self.LobbyPlayers[playerId].role.accountName,
        });
        return (team, ErrorCode.SUCCESS);
    }

    
    /// <summary>
    /// 玩家离开队伍(包括消息广播)
    /// </summary>
    /// <param name="self"></param>
    /// <param name="playerId"></param>
    public static void LevelTeam(this LobbyPlayerManagerComponent self, long playerId)
    {
        //查看玩家存在的队伍是哪个
        if (!self.LobbyPlayers.TryGetValue(playerId, out var player))
        {
            Log.Error("当前大厅不存在此玩家，玩家ID:" + playerId);
            return;
        }

        if (!self.Teams.TryGetValue(player.TeamId, out var team))
        {
            Log.Error("当前不存在此队伍，队伍ID:" + player.TeamId);
            return;
        }
        
        //先把自己从队伍中移除
        team.TeamMembers.RemoveAll(x => x.memberAccountId == playerId);
        //在把自己的TeamId清空
        player.TeamId = 0;
        //在这里向队伍中的其他成员广播成员离开消息
        TeamStateChangeMessage message = new TeamStateChangeMessage();
        message.playerId = playerId;
        message.teamState = 2;

        //先是队长
        if (!self.LobbyPlayers.TryGetValue(team.TeamOwner.memberAccountId, out player))
        {
            Log.Error("当前大厅不存在此玩家，玩家ID:" + playerId);
            return;
        }
        player.Session.Send(message);

        //然后是队员
        //看看有没有必要发
        if (team.TeamMembers.Count == 0)
        {
            //没有队员，没必要发
            Log.Info("队伍ID:" + team.TeamId + " 已经没有成员，不需要发送离开消息");
            return;
        }
        
        //发消息
        team.TeamMembers.ForEach(member =>
        {
            if (!self.LobbyPlayers.TryGetValue(member.memberAccountId, out player))
            {
                Log.Error("当前大厅不存在此玩家，玩家ID:" + playerId);
                return;
            }
            player.Session.Send(message);
        });
        
    }

    /// <summary>
    /// 移除队伍(包括消息广播)
    /// </summary>
    /// <param name="self"></param>
    /// <param name="playerId"></param>
    public static void RemoveTeam(this LobbyPlayerManagerComponent self, long playerId)
    {
        //查看玩家存在的队伍是哪个
        if (!self.LobbyPlayers.TryGetValue(playerId, out var player))
        {
            Log.Error("当前大厅不存在此玩家，玩家ID:" + playerId);
            return;
        }
        
        if (!self.Teams.TryGetValue(player.TeamId, out var team))
        {
            Log.Error("当前不存在此队伍，队伍ID:" + player.TeamId);
            return;
        }
        
        //将每个LobbyPlayer的TeamId清空
        player.TeamId = 0;
        
        //接着是所有的成员
        //队长解散队伍 ， 所以直接给所有成员发送解散消息
        TeamStateChangeMessage message = new TeamStateChangeMessage();
        message.teamState = 3;
        team.TeamMembers.ForEach(member =>
        {
            if (!self.LobbyPlayers.TryGetValue(member.memberAccountId, out player))
            {
                Log.Error("当前大厅不存在此玩家，玩家ID:" + playerId);
                return;
            }

            player.TeamId = 0;
            player.Session.Send(message);
        });

        //最后删除队伍
        self.Teams.Remove(team.TeamId);
    }
    
    /// <summary>
    /// 得到队伍成员ID列表，包括队长
    /// </summary>
    /// <param name="self"></param>
    /// <param name="teamId"></param>
    /// <returns></returns>
    public static List<long>? GetTeamMemberIds(this LobbyPlayerManagerComponent self , long teamId)
    {
        if (!self.Teams.TryGetValue(teamId, out var team))
        {
            Log.Error("当前不存在此队伍，队伍ID:" + teamId);
            return null;
        }

        var list = team.TeamMembers.Select(x => x.memberAccountId).ToList();
        list.Add(team.TeamOwner.memberAccountId);
        return list;
    }
    
    public static bool SyncLoadingProgress(this LobbyPlayerManagerComponent self , long teamId , long playerId , float progress)
    {
       //获取队伍
       if(!self.Teams.TryGetValue(teamId , out var team))
       {
           Log.Error("当前不存在此队伍，队伍ID:" + teamId);
           return false;
       }

       if (!self.TeamLoadProgress.TryGetValue(teamId, out var loadProgress))
       {
           //还没有这个队伍的加载进度，创建一个
           loadProgress = new Dictionary<long, float>();
           self.TeamLoadProgress[teamId] = loadProgress;
       }


       if (!loadProgress.TryGetValue(playerId, out var loadingValue))
       {
           //还没有这个玩家的加载进度，添加一个
           loadProgress.Add(playerId , progress);
       }
       else
       {
           //存在，更新进度
           loadProgress[playerId] = progress;
       }
       
       //看一下是否所有人都加载完毕
       
       if(loadProgress.Count != team.TeamMembers.Count + 1)
       {
           //人数还不够，说明还有人没开始加载
           return false;
       }
       
       //看看是否都加载完毕
       return loadProgress.Values.All(value => !(value < 1.0f));
    }
    
    public static void ClearTeamLoadProgress(this LobbyPlayerManagerComponent self , long teamId)
    {
        self.TeamLoadProgress.Remove(teamId);
    }
    
    
    
    
    
    #endregion
}
