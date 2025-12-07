using Fantasy.Entitas;

namespace Fantasy.Dungeons;

public class BattleManagerComponent : Entity
{
    public Dictionary<long , Dungeon> AllBattles = new Dictionary<long, Dungeon>(); 
}