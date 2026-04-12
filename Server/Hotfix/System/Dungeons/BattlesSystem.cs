using Fantasy;
using Fantasy.Dungeons;
using Fantasy.Entitas.Interface;
using Fantasy.Helper;
using Hotfix.Helper;

namespace Hotfix.System.Dungeons;

public class BattleDestroySystem : DestroySystem<Dungeon>
{
    protected override void Destroy(Dungeon self)
    {
        if (self.TimerId != 0)
        {
            self.Scene.TimerComponent.Net.Remove(ref self.TimerId);
        }

        foreach (var player in self.BattlePlayers)
        {
            player.Value.Dispose();
        }

        self.BattlePlayers.Clear();
        self.TimerId = 0;
        self.LogicFrameId = 0;
        self.BattleState = BattleStateEnum.None;
        self.PlayerFrameOperationDataDic.Clear();
    }
}

public static class BattlesSystem
{


    #region 生命周期

    public static void BattleStart(this Dungeon self)
    {
        self.BattleState = BattleStateEnum.Start;
        self.LogicFrameId = 0;

        // 逻辑帧更新
        self.TimerId = self.Scene.TimerComponent.Net.RepeatedTimer(
            CommonConfig.LogicFrameIntervalMs,
            self.LogicFrameUpdate
        );
    }
    
    public static void BattleEnd(this Dungeon self)
    {
        self.BattleState = BattleStateEnum.End;
        
        //这里的操作数据可以保存到数据库做战斗回放，但此项目暂不实现回放功能，所以直接清除
        self.PlayerFrameOperationDataDic.Clear();
        
        if (self.TimerId != 0)
        {
            self.Scene.TimerComponent.Net.Remove(ref self.TimerId);
            self.TimerId = 0;
        }
    }

    #endregion
    
    

    #region 逻辑帧更新核心

    public static void LogicFrameUpdate(this Dungeon self)
    {
        
        // 逻辑帧自增
        self.LogicFrameId++;
        foreach (var player in self.BattlePlayers)
        {
            if (player.Value.Session.IsDisposed)
            {
                Log.Warning("玩家断线，无法推送逻辑帧 , ID : " + player.Value.PlayerId);
                continue;
            }
            
            self.SendFrameData(player.Value);

        }

    }

    #endregion




    #region 追帧机制

    
    
    public static void SendFrameData(this Dungeon self , BattlePlayer bp)
    {
        var lastFrameId = bp.CurrentFrameId;
        var currentFrameId = self.LogicFrameId;
        
        SendFrameData(self, bp, lastFrameId + 1, currentFrameId);
    }
    
    private static void SendFrameData(this Dungeon self , BattlePlayer bp , long startFrameId , long endFrameId)
    {
        if (startFrameId > endFrameId)
        {
            Log.Warning($"[补帧失败] 无效的帧数范围 : {startFrameId} - {endFrameId}");
            return;
        }
        
        var serverCurrentFrameId = self.LogicFrameId;
        var availableFrameId = serverCurrentFrameId - CommonConfig.MaxHistoryFrames;
        
        if(startFrameId < availableFrameId)
        {
            Log.Warning($"[补帧失败] 请求的起始帧ID {startFrameId} 已经过期，当前可用的最早帧ID是 {availableFrameId}");
            return;
        }
        
        //分批次处理
        var totalFramesToSend = endFrameId - startFrameId + 1;
        var maxFramesPerBatch = CommonConfig.MaxChaseFramesPerBatch;
        if (totalFramesToSend > maxFramesPerBatch)
        {
            for(long batchStart = startFrameId; batchStart <= endFrameId; batchStart += maxFramesPerBatch)
            {
                long batchEnd = Math.Min(batchStart + maxFramesPerBatch - 1, endFrameId);
                SendFrameDataCore(self, bp, batchStart, batchEnd);
            }
        }
        else
        {
            SendFrameDataCore(self, bp, startFrameId, endFrameId);
        }
    }
    
    
    //追帧，补发从startFrameId到endFrameId之间的帧数据
    private static void SendFrameDataCore(this Dungeon self , BattlePlayer bp , long startFrameId , long endFrameId)
    {
        var message = new FrameOperateEventMessage_G2C
        {
            battleId = self.Id,
            startLogicFrameId = startFrameId,
            endLogicFrameId =  endFrameId,
            serverTick = TimeHelper.Now,
        };
        message.oneFrameCommandList = new List<OneFrameCommand>();
        
        for (long frameId = startFrameId; frameId <= endFrameId; frameId++)
        {
            var frameList = self.GetOneFrameOperationData(frameId);

            var oneFrameCommand = new OneFrameCommand
            {
                frameId =  frameId,
                frameOperateDataList = new List<FrameOperationData>(frameList)
            };
            message.oneFrameCommandList.Add(oneFrameCommand);
        }
        
        bp.Session.Send(message);
        
    }

    #endregion


    #region 操作帧

    public static List<FrameOperationData> GetOneFrameOperationData(this Dungeon self , long frameId)
    {
        lock (self.PlayerFrameOperationDataDic)
        {
            if (!self.PlayerFrameOperationDataDic.TryGetValue(frameId, out var list))
            {
                list = new List<FrameOperationData>();
                self.PlayerFrameOperationDataDic[frameId] = list;
            }
        }
        return self.PlayerFrameOperationDataDic[frameId];
    }
    
    public static void AddPlayerFrameOperationData(this Dungeon self , long frameId, List<FrameOperationData> frameOperationData)
    {
        lock (self.PlayerFrameOperationDataDic)
        {
            if (!self.PlayerFrameOperationDataDic.TryGetValue(frameId, out var list))
            {
                list = new List<FrameOperationData>();
                self.PlayerFrameOperationDataDic[frameId] = list;
            }

            
            //按照权重插入排序
            foreach (var op in frameOperationData)
            {
                int insertIndex = list.Count;
                for (int i = 0; i < list.Count; i++)
                {
                    var current = list[i];
                    if (op.operateType > current.operateType ||
                        (op.operateType == current.operateType && op.playerId < current.playerId))
                    {
                        insertIndex = i;
                        break;
                    }
                }

                list.Insert(insertIndex, op);
            }
        }
    }
    
    public static void SyncPlayerFrameData(this Dungeon self, long battleId, FrameOperateEventMessage_C2G message)
    {
        
        if(message.predictLogicFrameId < self.LogicFrameId)
        {
            Log.Warning(
                $"丢弃预测帧操作：battleId={battleId}, predictLogicFrameId={message.predictLogicFrameId}, currentLogicFrame={self.LogicFrameId}"
            );
            return;
        }

        if (message.predictLogicFrameId > self.LogicFrameId + CommonConfig.MaxPredictFrames)
        {
            Log.Warning(
                $"丢弃超前帧操作：battleId={battleId}, predictLogicFrameId={message.predictLogicFrameId}, currentLogicFrame={self.LogicFrameId}"
            );
            return;
        }
        
        //同步玩家当前执行到的逻辑帧
        var player = GetBattlePlayer(self, message.frameOperateDataList[0].playerId);
        if (player == null)
        {
            Log.Info("未找到玩家，无法同步操作数据，battleId : " + battleId + " playerId : " + message.frameOperateDataList[0].playerId);
            return;
        }
        player.CurrentFrameId = Math.Max(player.CurrentFrameId, message.lastLogicFrameId);
        
        Log.Info("接收玩家操作数据，battleId : " + battleId + "预测逻辑帧数 : " + message.predictLogicFrameId + " 操作数据数量 : " + message.frameOperateDataList.Count);
        
        AddPlayerFrameOperationData(self, message.predictLogicFrameId, message.frameOperateDataList);
    }

    #endregion

    #region 辅助方法

    public static BattlePlayer? GetBattlePlayer(this Dungeon self , long playerId)
    {
        if (self.BattlePlayers.TryGetValue(playerId, out var bp))
        {
            return bp;
        }

        return null;
    }

    #endregion
    
}