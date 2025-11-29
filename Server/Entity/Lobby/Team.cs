using Fantasy.Authentication;
using Fantasy.Entitas;

namespace Fantasy.Lobby;

public class Team : Entity
{
    public long TeamId;
    
    public TeamMemberInfo TeamOwner;
    
    public List<TeamMemberInfo> TeamMembers = new();
}