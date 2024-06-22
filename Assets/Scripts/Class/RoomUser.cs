using System;
public class RoomUser
{
    public string username;
    public string nickname;
    public rVector3 pos;
    public rVector3 rot;
    public float velocity;
    public Int32 anim;
    public Int32 color;
    public string roomUserType;
    public string userState;
    public string crimeType;
    public string lastReportAt;
}
public class rVector3
{
    public float x;
    public float y;
    public float z;

    public rVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}