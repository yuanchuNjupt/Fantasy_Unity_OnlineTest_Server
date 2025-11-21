using Fantasy;
using Fantasy.Async;
using Fantasy.Entitas;
using Fantasy.Lobby;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.Helper;
using Hotfix.System;

namespace Hotfix.Lobby;

public class LoginRequestHandler : MessageRPC<LoginRequest , LoginResponse>
{
    protected override async FTask Run(Session session, LoginRequest request, LoginResponse response, Action reply)
    {
        //客户端发送登录请求
        //缓存数据
        Log.Info("收到登录请求");
        var lobbyPlayerManager = session.Scene.GetComponent<LobbyPlayerManagerComponent>();
        //缓存玩家数据并保持会话
        var res = lobbyPlayerManager.AddPlayer(session);
        Log.Info("登陆成功，分配玩家ID:" + res.id);
        var otherPlayers = lobbyPlayerManager.GetLobbyPlayers(res.id).ToList();
        
        List<PlayerData> otherPlayersData = new List<PlayerData>();
        
        for (int i = 0; i < otherPlayers.Count(); i++)
        {
            //注意引用类型问题
            PlayerData playerData = new PlayerData();
            playerData.playerId = otherPlayers[i].Id;
            playerData.position = otherPlayers[i].Position.ToCSVector3();
            playerData.renderDir = otherPlayers[i].RenderDir.ToCSVector3();
            otherPlayersData.Add(playerData);
            Log.Info("获取其他玩家数据，玩家ID:" + otherPlayers[i].Id);
        }

        response.otherPlayerData = otherPlayersData;

        

        response.ErrorCode = 0;
        response.playerId = res.id;

        //向其他玩家广播新玩家加入
        

        

        
        if (otherPlayers.Count() == 0)
        {
            Log.Debug("当前没有其他玩家在线，无需广播");
            return;
        }
        foreach (var otherPlayer in otherPlayers)
        {
            OtherPlayerLoginMessage message = OtherPlayerLoginMessage.Create(session.Scene);
            message.playerId = res.id;
            if (otherPlayer.Session.IsDisposed)
            {
                Log.Debug("连接已断开，无法发送消息，玩家ID:" + otherPlayer.Id);
                continue;
            }
            otherPlayer.Session.Send(message);
            Log.Debug("向ID:" + otherPlayer.Id + "发送其他玩家登录请求");
        }



        await FTask.CompletedTask;
    }
}