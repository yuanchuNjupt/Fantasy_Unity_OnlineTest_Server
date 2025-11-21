using Fantasy.Entitas;
using Fantasy.Network;

namespace Fantasy.Lobby;

public class LobbyPlayer : Entity
{
    //玩家会话
    public Session Session;
    
    //玩家位置数据
    public Vector3 Position = new Vector3(0,0,0);
    
    //玩家朝向数据
    public Vector3 RenderDir = new Vector3(0,0,0);
    
    //玩家移速
    public float moveSpeed = 10f;
    
    //玩家转速
    public float rotateSpeed = 10f;




}