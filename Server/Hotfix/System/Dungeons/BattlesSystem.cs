using Fantasy;
using Fantasy.Dungeons;
using Fantasy.Entitas.Interface;
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
    }
}

public static class BattlesSystem
{
    public static void BattleStart(this Dungeon self)
    {
        self.BattleState = BattleStateEnum.Start;

        // 逻辑帧更新
        self.TimerId = self.Scene.TimerComponent.Net.RepeatedTimer(
            CommonConfig.LogicFrameIntervalMs,
            self.LogicFrameUpdate
        );
    }

    public static void LogicFrameUpdate(this Dungeon self)
    {
        try
        {
            // 逻辑帧自增
            self.LogicFrameId++;

            // 本次广播的采样帧：上一帧
            var sampleFrameId = self.LogicFrameId - 1;
            List<FrameOperationData> frameDataToSend;

            lock (self.PlayerFrameOperationDataDic)
            {
                if (!self.PlayerFrameOperationDataDic.TryGetValue(sampleFrameId, out var lastFrameDataList))
                {
                    lastFrameDataList = new List<FrameOperationData>();
                }

                if (lastFrameDataList.Count < self.BattlePlayers.Count)
                {
                    Log.Warning(
                        $"采样帧{sampleFrameId}未收齐，已收{lastFrameDataList.Count}/应收{self.BattlePlayers.Count}，继续广播。"
                    );
                }

                // 复制发送，避免发送时并发修改
                frameDataToSend = new List<FrameOperationData>(lastFrameDataList);

                // 这里不清理过期帧数据了，等BattleEnd时统一清理，避免误删还未广播的帧数据
            }

            Log.Info(
                $"逻辑帧{self.LogicFrameId}，准备广播采样帧{sampleFrameId}的 {frameDataToSend.Count} 个操作数据"
            );

            foreach (var player in self.BattlePlayers)
            {
                if (player.Value.Session.IsDisposed)
                {
                    Log.Warning("玩家断线，无法推送逻辑帧 , ID : " + player.Value.PlayerId);
                    continue;
                }

                var message = new FrameOperateEventMessage_G2C
                {
                    battleId = self.Id,
                    logicFrameId = self.LogicFrameId,
                    frameOperateDataList = new List<FrameOperationData>(frameDataToSend)
                };

                player.Value.Session.Send(message);
            }
        }
        catch (Exception e)
        {
            Log.Error("逻辑帧更新异常 : " + e.Message);
        }
    }

    public static void SyncPlayerFrameData(this Dungeon self, long battleId, FrameOperationData frameOperationData)
    {
        var sampleFrameId = frameOperationData.sampleFrameId;
        var currentCollectFrame = self.LogicFrameId; // 当前逻辑帧期望收集的采样帧
    
        // 迟到帧：已经广播过（<= LogicFrameId - 1）的都丢弃，避免混入后续帧
        if (sampleFrameId <= self.LogicFrameId - 1)
        {
            Log.Warning(
                $"丢弃迟到帧操作：battleId={battleId}, sampleFrameId={sampleFrameId}, currentLogicFrame={self.LogicFrameId}"
            );
            return;
        }
    
        lock (self.PlayerFrameOperationDataDic)
        {
            if (!self.PlayerFrameOperationDataDic.TryGetValue(sampleFrameId, out var list))
            {
                list = new List<FrameOperationData>();
                self.PlayerFrameOperationDataDic[sampleFrameId] = list;
            }
    
            // 同一玩家在同一采样帧只保留一条，避免重复上报导致单帧操作数膨胀
            var duplicated = false;
            foreach (var item in list)
            {
                if (item.playerId == frameOperationData.playerId)
                {
                    duplicated = true;
                    break;
                }
            }
    
            if (duplicated)
            {
                Log.Warning(
                    $"丢弃重复帧操作：battleId={battleId}, sampleFrameId={sampleFrameId}, playerId={frameOperationData.playerId}"
                );
                return;
            }
    
            list.Add(frameOperationData);
    
            Log.Info(
                $"战斗{battleId}接收帧操作：sampleFrameId={sampleFrameId}, 当前帧桶数量={list.Count}, 当前收集目标帧={currentCollectFrame}"
            );
        }
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
    
    
    
}