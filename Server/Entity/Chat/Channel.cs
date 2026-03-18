using Fantasy;
using Fantasy.Entitas;

namespace Fantasy.Chat;

/// <summary>
/// 聊天频道实体
/// </summary>
public sealed class Channel : Entity
{

    /// <summary>
    /// 频道内的玩家列表
    /// </summary>
    public Dictionary<long, ChatPlayer> Players { get; set; } = new Dictionary<long, ChatPlayer>();
    
    /// <summary>
    /// 频道最大成员数（0表示无限制）
    /// </summary>
    public int MaxMembers { get; set; }
    
    /// <summary>
    /// 历史消息队列（可选，用于新加入玩家查看历史）
    /// </summary>
    public Queue<ChatMessage> MessageHistory { get; set; } = new Queue<ChatMessage>();
    
    /// <summary>
    /// 最大历史消息数量
    /// </summary>
    public int MaxHistoryCount { get; set; } = 100;
}

