using Fantasy.Entitas;

namespace Fantasy.Dungeons;

public class Dungeon : Entity
{
    
    //战斗ID使用的就是实体ID 
    
    public Dictionary<long , BattlePlayer> BattlePlayers = new Dictionary<long, BattlePlayer>();
    
    public BattleStateEnum BattleState;

    public long LogicFrameId;

    public long TimerId;
    
    // public List<FrameOperationData> FrameOperationDataList = new List<FrameOperationData>();
    
    public Dictionary<long , List<FrameOperationData>> PlayerFrameOperationDataDic = new Dictionary<long, List<FrameOperationData>>();
    
    
}



public enum BattleStateEnum
{
    None,
    Start,
    End,
}