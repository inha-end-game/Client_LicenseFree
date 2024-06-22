using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LobbyRoomDto
{
    public int roomId;
    public RoomUser[] roomUsers;
    public RoomUser[] roomNpcs;
    public string roomState;
}
