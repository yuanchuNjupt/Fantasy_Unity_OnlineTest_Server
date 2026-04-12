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

        var battleComponent = session.Scene.GetComponent<BattleManagerComponent>();
        
        battleComponent.OnPlayerOperateFrameInput(message.battleId , message);
        
        await FTask.CompletedTask;
        
    }
}