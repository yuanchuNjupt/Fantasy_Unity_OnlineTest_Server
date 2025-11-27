using Fantasy.Entitas;
using Fantasy.Entitas.Interface;

namespace Fantasy.Authentication;

public class Account : Entity , ISupportedDataBase
{
    //账号
    public string account;
    
    //密码
    public string password;
    
    //登陆时间
    public long loginTime = 0;
    
    //注册时间
    public long createTime;
    
    //玩家暂时和角色绑定
    public Role role;
    
    //是否是第一次登录 : 通过登陆时间可知
    
 

}