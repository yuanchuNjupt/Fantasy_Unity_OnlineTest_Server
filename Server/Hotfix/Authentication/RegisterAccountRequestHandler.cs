using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Entitas;
using Fantasy.Helper;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System;

namespace Hotfix.Authentication;

public class RegisterAccountRequestHandler : MessageRPC<RegisterAccountRequest , RegisterAccountResponse>
{
    protected override async FTask Run(Session session, RegisterAccountRequest request, RegisterAccountResponse response, Action reply)
    {
        // //验证账号密码是否为空
        // if (String.IsNullOrEmpty(request.account) || String.IsNullOrEmpty(request.account))
        // {
        //     Log.Error("空账号或密码传输过来了！");
        //     return;
        // }

        var authenticationComponent = session.Scene.GetComponent<AuthenticationAccountComponent>();

        response.ErrorCode = await authenticationComponent.RegisterAccount(request.account, request.pass);
        response.account = request.account;
        response.pass = request.pass;



        await FTask.CompletedTask;
    }
}