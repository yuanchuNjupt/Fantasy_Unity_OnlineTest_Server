using Fantasy.Entitas;

namespace Fantasy.Lobby;

public class LobbyPlayerManagerComponent : Entity
{
    /// <summary>
    /// Lobby当前所有玩家
    /// </summary>
    public Dictionary<long , LobbyPlayer> LobbyPlayers = new Dictionary<long , LobbyPlayer>();
    
    /// <summary>
    /// Lobby当前所有队伍
    /// </summary>
    public Dictionary<long , Team> Teams = new Dictionary<long , Team>();
    
    
    /// <summary>
    /// Key: TeamId
    /// Value: Key: PlayerId , Value: LoadProgress
    /// </summary>
    public Dictionary<long , Dictionary<long , float>> TeamLoadProgress = new Dictionary<long , Dictionary<long , float>>();
    
    //从1000开始 
    public long TeamIdStart = 1000;
    
    public readonly float FixedDeltaTime = 0.02f;
}