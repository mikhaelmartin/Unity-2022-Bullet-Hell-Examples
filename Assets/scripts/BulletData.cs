using UnityEngine;

[CreateAssetMenu(fileName ="Bullet Data",menuName ="Custom/Bullet Data")]
public class BulletData : ScriptableObject
{
    public float lifeTime;

    [Header("target")]
    public bool aimTarget;
    public bool lockTarget;

    [Header("Linear")]
    public float linearInitialSpeed;
    public float linearFinalSpeed;
    public float linearAcceleration;
    public bool linearPingpong;

    [Header("Angular")]
    public float angularInitialSpeed;
    public float angularFinalSpeed;
    public float angularAcceleration;
    public bool angularPingpong;

    [Header("Spin")]
    public float spinInitialSpeed;
    public float spinFinalSpeed;
    public float spinAcceleration;
    public bool spinPingpong;

    [HideInInspector] public GameObject target;

    [HideInInspector] public float linearCurrentSpeed;
    [HideInInspector] public float angularCurrentSpeed;
    [HideInInspector] public float spinCurrentSpeed;

    [HideInInspector] public Vector2 targetPosition;
    [HideInInspector] public Vector2 velocity;

    [HideInInspector] public Vector2 currentPosition;
    [HideInInspector] public Vector2 previousPosition;
    [HideInInspector] public Vector2 nextPosition;

    [HideInInspector] public float currentRotation;
    [HideInInspector] public float previousRotation;
    [HideInInspector] public float nextRotation;

    [HideInInspector] public float currentSpin;
    [HideInInspector] public float previousSpin;
    [HideInInspector] public float nextSpin;

    public void SetUp(Vector3 position, float rotation, GameObject target)
    {
        this.target = target;
	
        if (target)
        {
            targetPosition = target.transform.position;
        }
        
        linearCurrentSpeed = linearInitialSpeed;
        angularCurrentSpeed  = angularInitialSpeed;
        spinCurrentSpeed = spinInitialSpeed;
        velocity = Quaternion.AngleAxis(rotation * Mathf.Rad2Deg, Vector3.forward) * Vector2.right * linearInitialSpeed;        
        
        currentPosition = position;
        previousPosition = position;
        nextPosition = position;
        
        currentRotation = rotation;
        previousRotation = rotation;
        nextRotation = rotation;

        currentSpin = 0;
        previousSpin = 0;
        nextSpin = 0;
    }
}
