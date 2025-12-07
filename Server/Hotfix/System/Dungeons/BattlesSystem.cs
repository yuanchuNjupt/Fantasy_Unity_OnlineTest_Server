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
        
        //逻辑帧更新
        self.TimerId = self.Scene.TimerComponent.Net.RepeatedTimer(CommonConfig.LogicFrameIntervalMs, self.LogicFrameUpdate);

        

    }

    
    public static void LogicFrameUpdate(this Dungeon self)
    {
        try
        {
            //逻辑帧自增
            self.LogicFrameId++;
            
            //给战斗中的所有玩家 推送逻辑帧更新通知
            var message = new FrameOperateEventMessage_G2C();
            message.battleId = self.Id;
            message.logicFrameId = self.LogicFrameId;
            message.frameOperateDataList = new List<FrameOperationData>();

            //收集所有客户端的当前帧操作推送给客户端
            lock (self.FrameOperationDataList)
            {
                foreach (var frameOperationData in self.FrameOperationDataList)
                {
                    message.frameOperateDataList.Add(frameOperationData);
                }
                //清理上一帧操作数据
                self.FrameOperationDataList.Clear();
            }
            
            

            foreach (var player in self.BattlePlayers)
            {
                if (player.Value.Session.IsDisposed)
                {
                    Log.Warning("玩家断线，无法推送逻辑帧 , ID : " + player.Value.PlayerId);
                    continue;
                }
                player.Value.Session.Send(message);
            }
            

        }
        catch (Exception e)
        {
            Log.Error("逻辑帧更新异常 : " + e.Message);            
        }
    }

    public static void SyncPlayerFrameData(this Dungeon self , long battleId , List<FrameOperationData> frameOperationDataList)
    {
        lock (self.FrameOperationDataList)
        {
            frameOperationDataList.ForEach(x =>
            {
                self.FrameOperationDataList.Add(x);
            });
        }
    }
    
    
    
    
    
}