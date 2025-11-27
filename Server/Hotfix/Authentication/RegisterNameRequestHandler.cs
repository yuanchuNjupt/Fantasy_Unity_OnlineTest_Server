using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.Authentication;

public class RegisterNameRequestHandler : MessageRPC<RegisterNameRequest , RegisterNameResponse>
{
    protected override async FTask Run(Session session, RegisterNameRequest request, RegisterNameResponse response, Action reply)
    {
        
        Log.Info("收到注册名称请求，账号：" + request.accountName + "，名称：" + request.name);
        var authenticationAccountComponent = session.Scene.GetComponent<AuthenticationAccountComponent>();

        response.ErrorCode = await authenticationAccountComponent.RegisterName(request.accountName, request.name);
        if (response.ErrorCode != 0)
        {
            Log.Debug("注册名称失败！");
            return;
        }
        
        response.accountName = request.accountName;
        response.name = request.name;
        await FTask.CompletedTask;
    }
}