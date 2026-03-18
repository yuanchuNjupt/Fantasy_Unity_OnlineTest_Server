using Fantasy;
using Fantasy.Dungeons;
using Fantasy.Entitas;
using Fantasy.Lobby;

namespace Hotfix.System.Dungeons;

public static class BattleManagerComponentSystem
{
    public static void StartBattle(this BattleManagerComponent self, List<LobbyPlayer> roleList)
    {
        Dungeon dungeon = Entity.Create<Dungeon>(self.Scene , true , true);
        self.AllBattles.Add(dungeon.Id , dungeon);

        dungeon.BattlePlayers = new Dictionary<long, BattlePlayer>();
        
        roleList.ForEach(role =>
        {
            BattlePlayer bp = Entity.Create<BattlePlayer>(self.Scene , true , true);
            bp.PlayerId = role.AccountId;
            bp.Session = role.Session;
            bp.BattleRole = role.role;
            dungeon.BattlePlayers.Add(bp.PlayerId, bp);
        });

        //开始战斗，推送逻辑帧更新。
        dungeon.BattleStart();
    }

    public static void OnPlayerOperateFrameInput(this BattleManagerComponent self, long battleId , FrameOperationData frameDataList)
    {
        //当前战斗是否存在
        self.AllBattles.TryGetValue(battleId, out var battle);
        if (battle == null)
        {
            Log.Error("操作帧同步无效，战斗不存在 , ID :" + battleId);
            return;
        }
            
        //缓存玩家操作
        battle.SyncPlayerFrameData(battleId, frameDataList);
    }


}