using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Bullet Pattern", fileName ="BulletPattern")]
public class BulletPattern : ScriptableObject
{

    [Header("Rotation")]
    public float rotationInitialSpeed;
    public float rotationFinalSpeed;
    public float rotationAcceleration;
    public bool rotationSpeedPingpong;

    [Header("Spread")]
    public int spreadCount = 1;
    [Range(0, 360)] public float spreadAngle;
    

    [Header("Sub Spread")]
    public int subSpreadCount = 1;
    [Range(0, 360)] public float subSpreadAngle;
}
