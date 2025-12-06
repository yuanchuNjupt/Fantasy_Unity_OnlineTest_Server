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
    
    /// <summary>
    /// 当前订阅的频道列表
    /// </summary>
    public HashSet<long> SubscribedChannels { get; set; } = new HashSet<long>();
    
    /// <summary>
    /// 玩家状态（在线/离线/忙碌等）
    /// </summary>
    public PlayerChatStatus Status { get; set; } = PlayerChatStatus.Online;
    
    /// <summary>
    /// 最后活跃时间
    /// </summary>
    public long LastActiveTime { get; set; }
}

/// <summary>
/// 玩家聊天状态枚举
/// </summary>
public enum PlayerChatStatus
{
    Online = 0,
    Offline = 1,
    Busy = 2,
    Away = 3
}

