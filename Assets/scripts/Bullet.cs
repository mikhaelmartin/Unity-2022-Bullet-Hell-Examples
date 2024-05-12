using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.UI;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] BulletData data;

    [SerializeField] GameObject spriteGO;

    [SerializeField] bool useInterpolation = true;

    float deltaTime;

    void Update()
    {
        // we need to cache deltaTime in Update. value of Time.deltaTime in FixedUpted is
        // automatically changed to Time.fixedDeltaTime by the engine
        deltaTime = Time.deltaTime;
        
        if (useInterpolation && deltaTime < Time.fixedDeltaTime)
        {
            var weight = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

            data.currentPosition = Vector2.Lerp(
                data.previousPosition,
                data.nextPosition,
                weight
            );

            data.currentRotation = Mathf.LerpAngle(
                data.previousRotation,
                data.nextRotation,
                weight
            );

            data.currentSpin = Mathf.LerpAngle(
                data.previousSpin,
                data.nextSpin,
                weight
            );
        }

        transform.position = data.currentPosition;
        transform.rotation = Quaternion.Euler(0, 0, data.currentRotation * Mathf.Rad2Deg);
        spriteGO.transform.rotation = Quaternion.Euler(
            0, 0, (data.currentRotation + data.currentSpin) * Mathf.Rad2Deg);        
    }


    void FixedUpdate() {
        data.lifeTime -= Time.fixedDeltaTime;
        if (data.lifeTime <= 0)
        {
            Destroy(gameObject);
        }


        // Linear
        if (data.linearAcceleration !=0 && data.linearCurrentSpeed != data.linearFinalSpeed)
        {
            data.linearCurrentSpeed = Mathf.MoveTowards(
                data.linearCurrentSpeed,
                data.linearFinalSpeed,
                data.linearAcceleration * Time.fixedDeltaTime
            );

            if (data.linearPingpong && Mathf.Approximately(data.linearCurrentSpeed, data.linearFinalSpeed))
            {
                data.linearFinalSpeed = data.linearInitialSpeed;
                data.linearInitialSpeed = data.linearCurrentSpeed;
            }
        }

        var direction = new Vector2(Mathf.Cos(data.currentRotation), Mathf.Sin(data.currentRotation)).normalized;
        data.velocity = direction * data.linearCurrentSpeed;
        if (data.velocity != Vector2.zero)
        {
            data.nextPosition = data.currentPosition + data.velocity * Time.fixedDeltaTime;
        }

        // // Angular
        if (data.angularAcceleration != 0 && data.angularCurrentSpeed != data.angularFinalSpeed)
        {
            data.angularCurrentSpeed = Mathf.MoveTowards(
                data.angularCurrentSpeed,
                data.angularFinalSpeed,
                data.angularAcceleration * Time.fixedDeltaTime
            );

            if (data.angularPingpong && Mathf.Approximately(data.angularCurrentSpeed, data.angularFinalSpeed))
            {
                data.angularFinalSpeed = data.angularInitialSpeed;
                data.angularInitialSpeed = data.angularCurrentSpeed;
            }
           
        }
        
        if (data.aimTarget && data.target)
        {
            if( data.lockTarget)
            {
                data.targetPosition = data.target.transform.position;
            }

            Vector2 desiredDirection = (data.targetPosition - data.currentPosition).normalized;
            float desiredRotation = Mathf.Atan2(desiredDirection.y,desiredDirection.x);

            if (data.currentRotation != desiredRotation)
            {                
                data.nextRotation = Mathf.MoveTowards(
                    data.currentRotation,
                    desiredRotation,
                    data.angularCurrentSpeed * Time.fixedDeltaTime
                );
            }

            if (!data.lockTarget && Mathf.Approximately(data.currentRotation, desiredRotation))
            {
                data.aimTarget = false;
                data.angularCurrentSpeed = 0;
                data.angularAcceleration = 0;
            }
        }
        else if(data.angularCurrentSpeed != 0)
        {
            data.nextRotation = data.currentRotation + data.angularCurrentSpeed * Time.fixedDeltaTime;
        }       

        // Spin
        if (data.spinAcceleration != 0 && data.spinCurrentSpeed != data.spinFinalSpeed)
        {
            data.spinCurrentSpeed = Mathf.MoveTowards(
                data.spinCurrentSpeed, data.spinFinalSpeed, data.spinAcceleration * Time.fixedDeltaTime);
            
            if (data.spinPingpong && Mathf.Approximately(data.spinCurrentSpeed, data.spinFinalSpeed))
            {
                data.spinFinalSpeed = data.spinInitialSpeed;
                data.spinInitialSpeed = data.spinCurrentSpeed;
            }
        }

        if (data.spinCurrentSpeed != 0)
        {
            data.nextSpin = data.currentSpin + data.spinCurrentSpeed * Time.fixedDeltaTime;
        }

        if (useInterpolation && deltaTime < Time.fixedDeltaTime)
        {
            data.previousPosition = data.currentPosition;
            data.previousRotation = data.currentRotation;
            data.previousSpin = data.currentSpin;
        }
        else
        {
            data.currentPosition = data.nextPosition;
            data.currentRotation = data.nextRotation;
            data.currentSpin = data.nextSpin;
        }
    }


    public void SetUp(Vector2 position, float rotation, BulletData _data, GameObject target)
    {
        this.transform.position = position;
        this.transform.rotation = Quaternion.Euler(0,0,rotation * Mathf.Rad2Deg);

        this.data = Instantiate(_data);
        this.data.SetUp(position, rotation, target);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Destroy(this.gameObject);
    }
}
