using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New AISettings", menuName = "MySOs/AI Settings")]
public class AISettings : ScriptableObject
{
    [Range(-1,1)]public float DotCheck;
    public float IdleStayTime, IdleMoveTime;
    public float StayMoveXTime, StopMoveXTime;
    public float StayMoveZTime, StopMoveZTime;
    public float MaxPredictionTime, MinPredictDistance, MaxPredictDistance;
    public float DeviationSpeed, DeviationAmount;
    public float DistanceClearanceX, DistanceClearanceZ;
    public float VelocityClearence;
}
