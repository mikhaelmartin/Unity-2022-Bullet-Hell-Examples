using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MultiBullet : MonoBehaviour
{
    [SerializeField] Material material;

    [SerializeField] int numInstances = 0;
    [SerializeField] Mesh mesh;
    [SerializeField] LayerMask collisionLayer;
    [SerializeField] float collisionRadius = 0.08f;
    [SerializeField] bool useInterpolation = true;

    RenderParams rp;
    
    Matrix4x4[] instData = new Matrix4x4[100];
    float[] lifeTime = new float[100];

    // Target
    bool[] aimTarget = new bool[100];
    bool[] lockTarget = new bool[100];

    // Linear
    float[] linearInitialSpeed = new float[100];
    float[] linearFinalSpeed = new float[100];
    float[] linearAcceleration = new float[100];
    bool[] linearPingpong = new bool[100];

    // Angular
    float[] angularInitialSpeed = new float[100];
    float[] angularFinalSpeed = new float[100];
    float[] angularAcceleration = new float[100];
    bool[] angularPingpong = new bool[100];

    // Spin
    float[] spinInitialSpeed = new float[100];
    float[] spinFinalSpeed = new float[100];
    float[] spinAcceleration = new float[100];
    bool[] spinPingpong = new bool[100];

    GameObject[] target = new GameObject[100];

    float[] linearCurrentSpeed = new float[100];
    float[] angularCurrentSpeed = new float[100];
    float[] spinCurrentSpeed = new float[100];

    Vector2[] targetPosition = new Vector2[100];
    Vector2[] velocity = new Vector2[100];

    Vector2[] currentPosition = new Vector2[100];
    Vector2[] previousPosition = new Vector2[100];
    Vector2[] nextPosition = new Vector2[100];

    float[] currentRotation = new float[100];
    float[] previousRotation = new float[100];
    float[] nextRotation = new float[100];

    float[] currentSpin = new float[100];
    float[] previousSpin = new float[100];
    float[] nextSpin = new float[100];
    float deltaTime;

    private void Start() 
    {        
        rp = new RenderParams(material);    
    }

    private void Update()
    {     
        // we need to cache deltaTime in Update. value of Time.deltaTime in FixedUpted is
        // automatically changed to Time.fixedDeltaTime by the engine
        deltaTime = Time.deltaTime;
        
        if (useInterpolation && deltaTime < Time.fixedDeltaTime)
        {
            var weight = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

            for (int i = 0; i < numInstances; i++)
            {
                currentPosition[i] = Vector2.Lerp(
                    previousPosition[i],
                    nextPosition[i],
                    weight
                );
                currentRotation[i] = Mathf.LerpAngle(
                    previousRotation[i],
                    nextRotation[i],
                    weight
                );
                
                currentSpin[i] = Mathf.LerpAngle(
                    previousSpin[i],
                    nextSpin[i],
                    weight
                );
            }
        }

        if (numInstances > 0)
        {
            // Matrix4x4[] instData = new Matrix4x4[numInstances];
            for (int i = 0; i < numInstances; i++)
            {
                instData[i].SetTRS(
                    currentPosition[i],
                    Quaternion.Euler(0, 0, (currentRotation[i] + currentSpin[i]) * Mathf.Rad2Deg),
                    new Vector3(0.8f, 0.8f, 0.8f)
                );
            }

            // if (numInstances > 0 && numInstances <= instData.Length)
            Graphics.RenderMeshInstanced(rp, mesh, 0, instData, numInstances);
        }
    }
    

    void FixedUpdate() {
        for (int i = 0; i < numInstances; i++)
        {   
            if (Physics2D.OverlapCircle(currentPosition[i], collisionRadius, collisionLayer))
            {
                lifeTime[i] = 0;
            }        
        }


        for (int i = 0; i < numInstances; i++)
        {
            lifeTime[i] -= Time.deltaTime;
            if (lifeTime[i] <= 0)
            {
                RemoveBullet(i);
            }            
        }

        for (int i = 0; i < numInstances; i++)
        {
            // Linear
            if (linearAcceleration[i] !=0 && linearCurrentSpeed[i] != linearFinalSpeed[i])
            {
                linearCurrentSpeed[i] = Mathf.MoveTowards(
                    linearCurrentSpeed[i],
                    linearFinalSpeed[i],
                    linearAcceleration[i] * Time.fixedDeltaTime
                );

                if (linearPingpong[i] && Mathf.Approximately(linearCurrentSpeed[i], linearFinalSpeed[i]))
                {
                    linearFinalSpeed[i] = linearInitialSpeed[i];
                    linearInitialSpeed[i] = linearCurrentSpeed[i];
                }
            }

            var direction = new Vector2(Mathf.Cos(currentRotation[i]), Mathf.Sin(currentRotation[i])).normalized;
            velocity[i] = direction * linearCurrentSpeed[i];
            
            if (velocity[i] != Vector2.zero)
            {
                nextPosition[i] = currentPosition[i] + velocity[i] * Time.fixedDeltaTime;
            }

            // // Angular
            if (angularAcceleration[i] != 0 && angularCurrentSpeed[i] != angularFinalSpeed[i])
            {
                angularCurrentSpeed[i] = Mathf.MoveTowards(
                    angularCurrentSpeed[i],
                    angularFinalSpeed[i],
                    angularAcceleration[i] * Time.fixedDeltaTime
                );

                if (angularPingpong[i] && Mathf.Approximately(angularCurrentSpeed[i], angularFinalSpeed[i]))
                {
                    angularFinalSpeed[i] = angularInitialSpeed[i];
                    angularInitialSpeed[i] = angularCurrentSpeed[i];
                }
            
            }

            if (aimTarget[i] && target[i] != null)
            {
                if(lockTarget[i])
                {
                    targetPosition[i] = target[i].transform.position;
                }

                Vector2 desiredDirection = (targetPosition[i] - currentPosition[i]).normalized;
                float desiredRotation = Mathf.Atan2(desiredDirection.y,desiredDirection.x);
                
                if (currentRotation[i] != desiredRotation)
                {
                    nextRotation[i] = Mathf.MoveTowards(
                        currentRotation[i],
                        desiredRotation,
                        Mathf.Abs(angularCurrentSpeed[i]) * Time.fixedDeltaTime
                    );
                }

                if (!lockTarget[i] && Mathf.Approximately(currentRotation[i],desiredRotation))
                {
                    aimTarget[i] = false;
                    angularCurrentSpeed[i] = 0;
                    angularAcceleration[i] = 0;
                }
            }
            else if(angularCurrentSpeed[i] != 0)
            {
                nextRotation[i] = currentRotation[i] + angularCurrentSpeed[i] * Time.fixedDeltaTime;
            }       

            // Spin
            if (spinAcceleration[i] != 0 && spinCurrentSpeed[i] != spinFinalSpeed[i])
            {
                spinCurrentSpeed[i] = Mathf.MoveTowards(
                    spinCurrentSpeed[i], spinFinalSpeed[i], spinAcceleration[i] * Time.fixedDeltaTime);
                
                if (spinPingpong[i] && Mathf.Approximately(spinCurrentSpeed[i], spinFinalSpeed[i]))
                {
                    spinFinalSpeed[i] = spinInitialSpeed[i];
                    spinInitialSpeed[i] = spinCurrentSpeed[i];
                }
            }

            if (spinCurrentSpeed[i] != 0)
            {
                nextSpin[i] = currentSpin[i] + spinCurrentSpeed[i] * Time.fixedDeltaTime;
            }

            if (useInterpolation && deltaTime < Time.fixedDeltaTime)
            {
                previousPosition[i] = currentPosition[i];
                previousRotation[i] = currentRotation[i];
                previousSpin[i] = currentSpin[i];
            }
            else
            {
                currentPosition[i] = nextPosition[i];
                currentRotation[i] = nextRotation[i];
                currentSpin[i] = nextSpin[i];
            }
        }
    }


    public void AddBullet(Vector2 position, float rotation, BulletData data, GameObject target)
    {
        if (numInstances < 0)
        {
            numInstances = 0;
        }

        while (lifeTime.Length <= numInstances)
        {
            GrowBufferData(100);
        }


        SetAt(numInstances, position, rotation, data, target);
        numInstances += 1;
    }

    public void RemoveBullet(int idx)
    {
        if (numInstances == 0)
        {
            return;
        }

        numInstances -= 1;
        SwapIndex(idx, numInstances);

    }

    void GrowBufferData(int amount)
    {
        Array.Resize(ref instData, instData.Length + amount);

        Array.Resize(ref lifeTime,lifeTime.Length + amount);
	
        Array.Resize(ref aimTarget,aimTarget.Length + amount);
        Array.Resize(ref lockTarget,lockTarget.Length + amount);
        
        Array.Resize(ref linearInitialSpeed,linearInitialSpeed.Length + amount);
        Array.Resize(ref linearFinalSpeed,linearFinalSpeed.Length + amount);
        Array.Resize(ref linearAcceleration,linearAcceleration.Length + amount);
        Array.Resize(ref linearPingpong,linearPingpong.Length + amount);
        
        Array.Resize(ref angularInitialSpeed,angularInitialSpeed.Length + amount);
        Array.Resize(ref angularFinalSpeed,angularFinalSpeed.Length + amount);
        Array.Resize(ref angularAcceleration,angularAcceleration.Length + amount);
        Array.Resize(ref angularPingpong,angularPingpong.Length + amount);
        
        Array.Resize(ref spinInitialSpeed,spinInitialSpeed.Length + amount);
        Array.Resize(ref spinFinalSpeed,spinFinalSpeed.Length + amount);
        Array.Resize(ref spinAcceleration,spinAcceleration.Length + amount);
        Array.Resize(ref spinPingpong,spinPingpong.Length + amount);
        
        Array.Resize(ref target,target.Length + amount);
        
        Array.Resize(ref linearCurrentSpeed,linearCurrentSpeed.Length + amount);
        Array.Resize(ref angularCurrentSpeed,angularCurrentSpeed.Length + amount);
        Array.Resize(ref spinCurrentSpeed,spinCurrentSpeed.Length + amount);
        
        Array.Resize(ref targetPosition,targetPosition.Length + amount);
        
        Array.Resize(ref velocity,velocity.Length + amount);
        
        Array.Resize(ref currentPosition,currentPosition.Length + amount);
        Array.Resize(ref previousPosition,previousPosition.Length + amount);
        Array.Resize(ref nextPosition,nextPosition.Length + amount);
        
        Array.Resize(ref currentRotation,currentRotation.Length + amount);
        Array.Resize(ref previousRotation,previousRotation.Length + amount);
        Array.Resize(ref nextRotation,nextRotation.Length + amount);

        Array.Resize(ref currentSpin,currentSpin.Length + amount);
        Array.Resize(ref previousSpin,previousSpin.Length + amount);
        Array.Resize(ref nextSpin,nextSpin.Length + amount);
    }

    void SetAt(int idx,Vector2 position, float rotation, BulletData data, GameObject target)
    {
        lifeTime[idx] = data.lifeTime;
	
        aimTarget[idx] = data.aimTarget;
        lockTarget[idx] = data.lockTarget;
        
        linearInitialSpeed[idx] = data.linearInitialSpeed;
        linearFinalSpeed[idx] = data.linearFinalSpeed;
        linearAcceleration[idx] = data.linearAcceleration;
        linearPingpong[idx] = data.linearPingpong;
        
        angularInitialSpeed[idx] = data.angularInitialSpeed;
        angularFinalSpeed[idx] = data.angularFinalSpeed;
        angularAcceleration[idx] = data.angularAcceleration;
        angularPingpong[idx] = data.angularPingpong;
        
        spinInitialSpeed[idx] = data.spinInitialSpeed;
        spinFinalSpeed[idx] = data.spinFinalSpeed;
        spinAcceleration[idx] = data.spinAcceleration;
        spinPingpong[idx] = data.spinPingpong;
        
        this.target[idx] = target;
        
        linearCurrentSpeed[idx] = data.linearInitialSpeed;
        angularCurrentSpeed[idx] = data.angularInitialSpeed;
        spinCurrentSpeed[idx] = data.spinInitialSpeed;        
        
        targetPosition[idx] = target.transform.position;

        var direction = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
        velocity[idx] = direction * linearCurrentSpeed[idx] * deltaTime; 
        
        currentPosition[idx] = position;
        previousPosition[idx] = position;
        nextPosition[idx] = position;
        
        currentRotation[idx] = rotation;
        previousRotation[idx] = rotation;
        nextRotation[idx] = rotation;

        currentSpin[idx] = 0;
        previousSpin[idx] = 0;
        nextSpin[idx] = 0;
    }

    void SwapIndex(int a, int b)
    {
        lifeTime[a] = lifeTime[b];
	
        aimTarget[a] = aimTarget[b];
        lockTarget[a] = lockTarget[b];
        
        linearInitialSpeed[a] = linearInitialSpeed[b];
        linearFinalSpeed[a] = linearFinalSpeed[b];
        linearAcceleration[a] = linearAcceleration[b];
        linearPingpong[a] = linearPingpong[b];
        
        angularInitialSpeed[a] = angularInitialSpeed[b];
        angularFinalSpeed[a] = angularFinalSpeed[b];
        angularAcceleration[a] = angularAcceleration[b];
        angularPingpong[a] = angularPingpong[b];
        
        spinInitialSpeed[a] = spinInitialSpeed[b];
        spinFinalSpeed[a] = spinFinalSpeed[b];
        spinAcceleration[a] = spinAcceleration[b];
        spinPingpong[a] = spinPingpong[b];
        
        target[a] = target[b];
        
        linearCurrentSpeed[a] = linearCurrentSpeed[b];
        angularCurrentSpeed[a] = angularCurrentSpeed[b];
        spinCurrentSpeed[a] = spinCurrentSpeed[b];
        
        targetPosition[a] = targetPosition[b];
        
        velocity[a] = velocity[b];
        
        currentPosition[a] = currentPosition[b];
        previousPosition[a] = previousPosition[b];
        nextPosition[a] = nextPosition[b];
        
        currentRotation[a] = currentRotation[b];
        previousRotation[a] = previousRotation[b];
        nextRotation[a] = nextRotation[b];

        currentSpin[a] = currentSpin[b];
        previousSpin[a] = previousSpin[b];
        nextSpin[a] = nextSpin[b];
    }

}
