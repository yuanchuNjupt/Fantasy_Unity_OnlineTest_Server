namespace Hotfix.Helper;

public class CommonConfig
{
    //逻辑帧间隔时间，单位毫秒
    public static readonly int LogicFrameIntervalMs = 66;
    
    //最大允许的预测帧数 
    public static readonly int MaxPredictFrames = 5;
    
    //内存中保留的最大历史帧数
    public static readonly int MaxHistoryFrames = 100;
    
    //追帧的单批次最大帧数
    public static readonly int MaxChaseFramesPerBatch = 20;
    
}