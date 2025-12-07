using Fantasy;
using Fantasy.Async;
using Fantasy.Authentication;
using Fantasy.Dungeons;
using Fantasy.Event;
using Fantasy.Lobby;

namespace Hotfix;

public class OnSceneCreateConfig : AsyncEventSystem<OnCreateScene>
{
    protected override async FTask Handler(OnCreateScene self)
    {
        switch (self.Scene.SceneType)
        {
            case SceneType.Gate:
                self.Scene.AddComponent<LobbyPlayerManagerComponent>();
                self.Scene.AddComponent<AuthenticationAccountComponent>();
                self.Scene.AddComponent<BattleManagerComponent>();
                break;
        }



        await FTask.CompletedTask;
    }
}