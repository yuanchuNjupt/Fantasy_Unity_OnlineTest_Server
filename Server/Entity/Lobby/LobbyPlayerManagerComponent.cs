using Fantasy.Entitas;

namespace Fantasy.Lobby;

public class LobbyPlayerManagerComponent : Entity
{
    /// <summary>
    /// Lobby当前所有玩家
    /// </summary>
    public Dictionary<long , LobbyPlayer> LobbyPlayers = new Dictionary<long , LobbyPlayer>();
    
    public readonly float FixedDeltaTime = 0.02f;
}