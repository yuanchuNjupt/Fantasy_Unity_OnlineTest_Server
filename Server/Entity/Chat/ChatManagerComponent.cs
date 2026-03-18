using Fantasy.Entitas;

namespace Fantasy.Chat;

public class ChatManagerComponent : Entity
{
    public Dictionary<long , Channel> AllChannels = new Dictionary<long, Channel>();
}