using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Database;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.Helper;
using Hotfix.System;

namespace Hotfix.LobbyHandler;

public class EntryLobbyRequestHandler : MessageRPC<EntryLobbyRequest,EntryLobbyResponse>
{
    protected override async FTask Run(Session session, EntryLobbyRequest request, EntryLobbyResponse response, Action reply)
    {
        var lobbyPlayerManager = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        //缓存玩家数据并保持会话
        response.ErrorCode = await lobbyPlayerManager.AddPlayer(session , request.accountId);

        if (response.ErrorCode != 0)
        {
            Log.Debug("添加玩家到大厅失败，错误码:" + response.ErrorCode);
            return;
        }
        
        //监听会话
        var sessionDispose = session.AddComponent<SessionDisposeComponent>();
        sessionDispose.AccountId = request.accountId;
        
        
        Log.Info("进入大厅成功，玩家ID:" + request.accountId);
        var otherPlayers = lobbyPlayerManager.GetLobbyPlayers(request.accountId).ToList();
        
        
        
        List<StateSyncData> otherPlayersData = new ();
        
        for (int i = 0; i < otherPlayers.Count(); i++)
        {
            //注意引用类型问题
            StateSyncData playerData = new ();
            playerData.playerId = otherPlayers[i].AccountId;
            playerData.position = otherPlayers[i].Position.ToCSVector3();
            playerData.inputDir = otherPlayers[i].RenderDir.ToCSVector3();
            playerData.PlayerName = otherPlayers[i].role.accountName;
            otherPlayersData.Add(playerData);
            Log.Info("获取其他玩家数据，玩家ID:" + otherPlayers[i].AccountId);
        }

        response.otherPlayerData = otherPlayersData;

        

        response.ErrorCode = ErrorCode.SUCCESS;
        
        //获取自身玩家数据
        IDatabase dataBase = session.Scene.World.Database;
        var selfAccount = await dataBase.First<Account>(x => x.Id == request.accountId);
        
        
        
        
        
        Log.Debug("玩家上次上线位置：" + selfAccount.role.LastPosition);

        response.selfData = new StateSyncData()
        {
            playerId = request.accountId,
            position = selfAccount.role.LastPosition.ToCSVector3(),
            inputDir = selfAccount.role.LastRenderDir.ToCSVector3(),
            PlayerName = selfAccount.role.accountName
            
        };
        //向其他玩家广播新玩家加入
        

        

        
        if (otherPlayers.Count() == 0)
        {
            Log.Debug("当前没有其他玩家在线，无需广播");
            return;
        }
        foreach (var otherPlayer in otherPlayers)
        {
            OtherPlayerLoginMessage message = OtherPlayerLoginMessage.Create(session.Scene);
            message.playerData = response.selfData;
            if (otherPlayer.Session.IsDisposed)
            {
                Log.Debug("连接已断开，无法发送消息，玩家ID:" + otherPlayer.Id);
                continue;
            }
            otherPlayer.Session.Send(message);
            Log.Debug("向ID:" + otherPlayer.Id + "发送其他玩家登录请求");
        }

    }
}