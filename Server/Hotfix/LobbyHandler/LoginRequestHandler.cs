using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.LobbyHandler;

public class LoginRequestHandler : MessageRPC<LoginRequest , LoginResponse>
{
    protected override async FTask Run(Session session, LoginRequest request, LoginResponse response, Action reply)
    {
        //客户端发送登录请求
        //缓存数据
        Log.Info("收到登录请求");
        
        var authenticationComponent = session.Scene.GetComponent<AuthenticationAccountComponent>();
        var res = await authenticationComponent.LoginAccount(request.account, request.pass);
        response.ErrorCode = res.errorCode;
        
        
        if (response.ErrorCode != 0)
        {
            Log.Debug("账号验证失败，错误码:" + response.ErrorCode);
            return;
        }

        response.accountId = res.accountData.Id;
        response.accountName = res.accountData.role.accountName;
        
        
        await FTask.CompletedTask;
    }
}