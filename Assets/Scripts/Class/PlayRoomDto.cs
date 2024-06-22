using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PlayRoomDto
{
    public int roomId;
    public RoomUser[] roomUsers;
    public int remainCrimeCount;
    public string roomState;
    public string roomEndAt;
    public string copUsername;
    public string spyUsername;
    public string boomerUsername;
    public string assassinUsername;
    public rVector3[] recentItemUseInfo;
    public ReportInfo[] recentReportInfo;
}
