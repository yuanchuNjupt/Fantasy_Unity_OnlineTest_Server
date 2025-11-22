using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Database;
using Fantasy.Entitas;
using Fantasy.Helper;
using Fantasy.Lobby;

namespace Hotfix.System;

public static class AuthenticationAccountComponentSystem
{
    public static async FTask<uint> RegisterAccount(this AuthenticationAccountComponent self, string account , string password)
    {
        
        //验证账号密码是否为空
        if (String.IsNullOrEmpty(account) || String.IsNullOrEmpty(account))
        {
            Log.Error("空账号或密码传输过来了！");
            return ErrorCode.ACCOUNT_OR_PASSWORD_IS_EMPTY;
        }
        
        if (self.AccountCache.ContainsKey(account.GetHashCode()))
        {
            Log.Info("账号已存在:" + account);
            return ErrorCode.ACCOUNT_ALREADY_EXISTS;
        }
        
        //不存在，向数据库查询该账号是否存在
        IDatabase dataBase = self.Scene.World.Database;
        bool res = await dataBase.Exist<Account>(x => x.account == account);
        if (res)
        {
            Log.Info("账号已存在:" + account);
            return ErrorCode.ACCOUNT_ALREADY_EXISTS;
        }
        
        //确实不存在
        Log.Info("账号不存在，可以注册:" + account);
        
        //创建账号
        Account newAccount = Entity.Create<Account>(self.Scene , true , true);
        newAccount.account = account;
        newAccount.createTime = TimeHelper.Now;
        newAccount.password = password;
        
        //创建角色，作为Account的一部分
        Role role = Entity.Create<Role>(self.Scene , true , true);
        role.AccountId = newAccount.Id;
        role.moveSpeed = 10f;
        role.LastPosition = new Vector3(0, 0, 0);
        role.LastRenderDir = new Vector3(0, 0, 1);
        newAccount.role = role;
        
        //缓存Account（包含Role）
        self.AccountCache.Add(account.GetHashCode() , newAccount);
        
        //只保存Account到数据库，Role作为Account的一部分会自动保存
        await dataBase.Save(newAccount);

        return ErrorCode.SUCCESS;
    }

    public static async FTask<(uint errorCode , Account accountData)> LoginAccount(this AuthenticationAccountComponent self, string account, string password)
    {
        //向数据库查询该账号是否存在
        IDatabase database = self.Scene.World.Database;
        Account res = await database.First<Account>(x => x.account == account);

        if (res == null || res.password != password)
        {
            return (ErrorCode.ACCOUNT_OR_PASSWORD_ERROR , null);
        }
        
        //验证通过
        //缓存
        if (!self.AccountCache.ContainsKey(account.GetHashCode()))
        {
            self.AccountCache.Add(account.GetHashCode(), res);
        }

        return (ErrorCode.SUCCESS , res);


    }
    
    
    //玩家上下线处理也在这里
    
    

   
}