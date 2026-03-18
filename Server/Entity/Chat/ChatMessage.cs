using Fantasy;
using Fantasy.Entitas;

namespace Fantasy.Chat;

/// <summary>
/// 聊天消息实体
/// </summary>
public sealed class ChatMessage : Entity
{
    
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
    /// 频道ID（0表示私聊）
    /// </summary>
    public long ChannelId { get; set; }
    
   
}

