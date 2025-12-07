using Fantasy.Authentication;
using Fantasy.Entitas;
using Fantasy.Network;

namespace Fantasy.Dungeons;

public class BattlePlayer : Entity
{
    public long PlayerId;
    
    public Role BattleRole;
    
    public Session Session;

    public bool IsBattleEnd = false;


    public override void Dispose()
    {
        base.Dispose();
        PlayerId = 0;
        BattleRole = null;
        Session = null;
        IsBattleEnd = false;
    }
}