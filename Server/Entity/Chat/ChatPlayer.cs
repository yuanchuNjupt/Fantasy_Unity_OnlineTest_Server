using Fantasy;
using Fantasy.Entitas;
using Fantasy.Network;

namespace Fantasy.Chat;

/// <summary>
/// 聊天玩家实体
/// </summary>
public sealed class ChatPlayer : Entity
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public long AccountId { get; set; }
    
    /// <summary>
    /// 玩家昵称
    /// </summary>
    public string PlayerName { get; set; }
    
    /// <summary>
    /// 玩家Session
    /// </summary>
    public Session Session { get; set; }
    
    
}



