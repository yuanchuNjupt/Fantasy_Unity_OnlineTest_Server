using Fantasy.Entitas;

namespace Fantasy.Authentication;

public class AuthenticationAccountComponent : Entity
{
    //Key : 账号的HashCode
    public Dictionary<int , Account> AccountCache = new Dictionary<int , Account>();
}