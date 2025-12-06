using Fantasy;
using Fantasy.Entitas;

namespace Fantasy.Chat;

/// <summary>
/// 聊天频道实体
/// </summary>
public sealed class Channel : Entity
{
    /// <summary>
    /// 频道ID
    /// </summary>
    public long ChannelId { get; set; }
    
    /// <summary>
    /// 频道名称
    /// </summary>
    public string ChannelName { get; set; }
    
    /// <summary>
    /// 频道类型
    /// </summary>
    public ChannelType Type { get; set; }
    
    /// <summary>
    /// 频道内的玩家列表
    /// </summary>
    public Dictionary<long, ChatPlayer> Players { get; set; } = new Dictionary<long, ChatPlayer>();
    
    /// <summary>
    /// 频道创建者ID
    /// </summary>
    public long CreatorId { get; set; }
    
    /// <summary>
    /// 频道创建时间
    /// </summary>
    public long CreateTime { get; set; }
    
    /// <summary>
    /// 频道最大成员数（0表示无限制）
    /// </summary>
    public int MaxMembers { get; set; }
    
    /// <summary>
    /// 频道描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 历史消息（可选，用于新加入玩家查看历史）
    /// </summary>
    public List<ChatMessage> MessageHistory { get; set; } = new List<ChatMessage>();
    
    /// <summary>
    /// 最大历史消息数量
    /// </summary>
    public int MaxHistoryCount { get; set; } = 100;
}

/// <summary>
/// 频道类型
/// </summary>
public enum ChannelType
{
    World = 0,          // 世界频道（所有人）
    Guild = 1,          // 公会频道
    Team = 2,           // 队伍频道
    Custom = 3,         // 自定义频道
    Private = 4         // 私聊频道
}

