using Fantasy.Entitas;

namespace Fantasy.Authentication;

public class AuthenticationAccountComponent : Entity
{
    //Key : accountName.HashCode , Value : Account
    public Dictionary<int , Account> AccountCache = new Dictionary<int , Account>();
}