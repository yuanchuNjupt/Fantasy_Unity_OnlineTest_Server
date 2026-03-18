using Fantasy;
using Fantasy.Async;
using Fantasy.Dungeons;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Hotfix.System.Dungeons;

namespace Hotfix.DungeonsHandler;

public class FrameOperateEventMessageHandler : Message<FrameOperateEventMessage_C2G>
{
    protected override async FTask Run(Session session, FrameOperateEventMessage_C2G message)
    {
        //1.收集并缓存所有客户端发送过来的帧操作请求，在下一个逻辑帧广播给所有的客户端，实现操作同步
        //即相同的时机 + 相同的操作 = 相同的结果
        var battleComponent = session.Scene.GetComponent<BattleManagerComponent>();
        
        battleComponent.OnPlayerOperateFrameInput(message.battleId , message.frameOperateDataList);
        
        await FTask.CompletedTask;
        
    }
}