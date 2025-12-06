using Fantasy;
using Fantasy.Entitas;

namespace Fantasy.Chat;

/// <summary>
/// 聊天消息实体
/// </summary>
public sealed class ChatMessage : Entity
{
    /// <summary>
    /// 消息ID（唯一标识）
    /// </summary>
    public long MessageId { get; set; }
    
    /// <summary>
    /// 发送者账号ID
    /// </summary>
    public long SenderId { get; set; }
    
    /// <summary>
    /// 发送者昵称
    /// </summary>
    public string SenderName { get; set; }
    
    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// 消息类型
    /// </summary>
    public ChatMessageType MessageType { get; set; }
    
    /// <summary>
    /// 频道ID（0表示私聊）
    /// </summary>
    public long ChannelId { get; set; }
    
    /// <summary>
    /// 接收者ID（私聊时使用）
    /// </summary>
    public long ReceiverId { get; set; }
    
    /// <summary>
    /// 发送时间戳
    /// </summary>
    public long Timestamp { get; set; }
    
    /// <summary>
    /// 消息状态
    /// </summary>
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
}

/// <summary>
/// 聊天消息类型
/// </summary>
public enum ChatMessageType
{
    Text = 0,           // 文本消息
    Private = 1,        // 私聊
    World = 2,          // 世界频道
    Guild = 3,          // 公会频道
    Team = 4,           // 队伍频道
    System = 5,         // 系统消息
    Custom = 99         // 自定义频道
}

/// <summary>
/// 消息状态
/// </summary>
public enum MessageStatus
{
    Sending = 0,        // 发送中
    Sent = 1,           // 已发送
    Received = 2,       // 已接收
    Read = 3,           // 已读
    Failed = 4          // 发送失败
}

