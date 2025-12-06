using Fantasy;
using Fantasy.Entitas;

namespace Fantasy.Chat;

/// <summary>
/// Session销毁时的清理组件
/// </summary>
public sealed class ChatSessionDisposeComponent : Entity
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public long AccountId { get; set; }
}

