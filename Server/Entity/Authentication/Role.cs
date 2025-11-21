using Fantasy.Entitas;
using Fantasy.Entitas.Interface;
using Fantasy.Lobby;

namespace Fantasy.Authentication;

public class Role : Entity , ISupportedDataBase 
{
    //归属于哪个账号
    public long AccountId;
    
    //角色移速
    public float moveSpeed = 10f;
    
    //角色上次下线的位置
    public Vector3 LastPosition;
    
    //角色上次下线的朝向
    public Vector3 LastRenderDir;
}