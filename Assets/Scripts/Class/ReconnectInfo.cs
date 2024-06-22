using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ReconnectInfo
{
    public string currentDateAt;
    public int remainItemCount;
    public Dictionary<int, rVector3> missionInfo;
    public string[] targetInfo;
    public int currentMissionPhase;
    public PlayRoomDto playRoomDto;
    public LobbyRoomDto LobbyRoomDto;
}
