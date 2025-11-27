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

    public static async FTask<uint> RegisterName(this AuthenticationAccountComponent self, string accountName, string name)
    {
        IDatabase dataBase = self.Scene.World.Database;
        //查询此名称是否重复
        if (await dataBase.Exist<Account>(x => x.role.accountName == name))
        {
            //已经有人注册了这个名字
            return ErrorCode.NAME_HAS_BE_REGISTER;
        }
        
        
        //不存在，允许注册
        
        if (!self.AccountCache.TryGetValue(accountName.GetHashCode(), out var account))
        {
            //从数据库中查询账户
            account = await dataBase.First<Account>(x => x.account == accountName);
        }

        if (account == null)
        {
            Log.Error("出现严重错误，不存在此账号！");
            return ErrorCode.PLAYER_NOT_FOUND;
        }
        account.role.accountName = name;
        
        //存回去
        await dataBase.Save(account);
        self.AccountCache[accountName.GetHashCode()] = account;
        Log.Info("账号名称注册成功 账号：" + accountName + " 名称：" + name);
        
        
        return ErrorCode.SUCCESS;
    }
        
    

   
}