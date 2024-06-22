using System;
using UnityEngine;

[Serializable]
public class Movement : JsonData
{
    public int moveSpeed, runSpeed, rotateSpeed, FOV;
}
[Serializable]
public class EventScreen : JsonData
{
    public int screenXmin, screenYmin, screenZmin, screenXmax, screenYmax, screenZmax, screenMotionCycle;
}
[Serializable]
public class Spawn : JsonData
{
    public int posXmin, posZmin, posXmax, posZmax;
}
[Serializable]
public class Shot : JsonData
{
    public int AimRange, ReloadTime, InspectCoolTime, ShotCoolTime, MissShotPenalty;
}
[Serializable]
public class Map : JsonData
{
    public int mapXmin, mapYmin, mapZmin, mapXmax, mapYmax, mapZmax, mapCenterX, mapCenterZ;
}
[Serializable]
public class Motion : JsonData
{
    public int motionNo;
    public int motionTIme;
    public string motionTrigger;
    public bool screenMotion;
}
[Serializable]
public class Building : JsonData
{
    public int buildingCenterX;
    public int buildingCenterZ;
    public int buildingSizeRatio;
}
[Serializable]
public class criminalMinigame : JsonData
{
    public string interactionKey;
    public int barHeight, barLength;
    public int successBarLength, movingBarLength;
    public int firstMovingBarSpeed, secondMovingBarSpeed, thirdMovingBarSpeed;
}
[Serializable]
public class spyMinigame : JsonData
{
    public int pressTimeLimit, firstPressKeysNum, secondPressKeysNum, thirdPressKeysNum;
}
[Serializable]
public class robberyMinigame : JsonData
{
    public int givenWords, timeLimitToTyping;
}
[Serializable]
public class assassinMinigame : JsonData
{
    public int needToKill, undetectedDistanceFromNPC, timeOfScreamingIfDetected;
}
[Serializable]
public class boomerMinigame : JsonData
{
    public int timeToSetBomb, timeToExplosion, reducedTimePerMissionClear;
}
[Serializable]
public class report : JsonData
{
    public int ReportRange, ReportCoolTime, EffectActivatingTime, EffectContinuousTime;
}