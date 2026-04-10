using Fantasy.Entitas;

namespace Fantasy.Dungeons;

public class Dungeon : Entity
{
    
    //战斗ID使用的就是实体ID 
    public Dictionary<long , BattlePlayer> BattlePlayers = new Dictionary<long, BattlePlayer>();
    
    public BattleStateEnum BattleState;

    //当前服务器权威的逻辑帧ID
    public long LogicFrameId;

    public long TimerId;
    
    public readonly Dictionary<long , List<FrameOperationData>> PlayerFrameOperationDataDic = new Dictionary<long, List<FrameOperationData>>();
    
    //最大允许的预测帧数 
    public long MaxPredictFrames = 5;
    
    
    
}



public enum BattleStateEnum
{
    None,
    Start,
    End,
}