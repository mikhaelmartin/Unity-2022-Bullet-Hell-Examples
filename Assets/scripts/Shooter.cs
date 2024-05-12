using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] MonoBehaviour bulletPrefab;
    [SerializeField] BulletPattern bulletPattern;
    [SerializeField] BulletData bulletData;


    float rotationSpeed;
    BulletManager bulletManager;

    private void Start() {
        var go = GameObject.FindGameObjectWithTag("BulletManager");
        bulletManager = go.GetComponent<BulletManager>();

        rotationSpeed = bulletPattern.rotationInitialSpeed;
    }

    private void FixedUpdate()
    {

        if (bulletPattern.rotationAcceleration != 0)
        {
            rotationSpeed = Mathf.MoveTowards(
                rotationSpeed,
                bulletPattern.rotationFinalSpeed,
                bulletPattern.rotationAcceleration * Time.fixedDeltaTime
            );

            if (bulletPattern.rotationSpeedPingpong && Mathf.Approximately(rotationSpeed,bulletPattern.rotationFinalSpeed))
            {
                bulletPattern.rotationFinalSpeed = bulletPattern.rotationInitialSpeed;
                bulletPattern.rotationInitialSpeed = rotationSpeed;
            }
        }

        if (rotationSpeed != 0)
        {
            var z_rot = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            z_rot = z_rot + rotationSpeed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.Euler(0, 0, z_rot * Mathf.Rad2Deg);
        }
    }


    public void Shoot(GameObject target = null)
    {
        float minAngle = bulletPattern.spreadCount > 1 ? -bulletPattern.spreadAngle / 2 : 0;
        float deltaAngle = bulletPattern.spreadCount > 1 ? bulletPattern.spreadAngle / (bulletPattern.spreadCount - 1) : 0;

        List<float> angleArray = new List<float>(bulletPattern.spreadCount);

        for (int i = 0; i < bulletPattern.spreadCount; i++)
        {
            angleArray.Add(minAngle + i * deltaAngle);
        }


        float subMinAngle = bulletPattern.subSpreadCount > 1 ? -bulletPattern.subSpreadAngle / 2 : 0;
        float subDeltaAngle = bulletPattern.subSpreadCount > 1 ? bulletPattern.subSpreadAngle / (bulletPattern.subSpreadCount - 1) : 0;
        List<float> subAngleArray = new List<float>(bulletPattern.subSpreadCount);

        for (int i = 0; i < bulletPattern.subSpreadCount; i++)
        {
            subAngleArray.Add(subMinAngle + i * subDeltaAngle);
        }
    
        foreach (float angle in angleArray)
        {
            foreach (float subAngle in subAngleArray)
            {
                // Spawn Bullet
                bulletManager.Spawn(
                    transform.position,
                    (transform.rotation.eulerAngles.z + angle + subAngle) * Mathf.Deg2Rad ,
                    bulletPrefab,
                    bulletData,
                    target
                );
            }
        }    
    }
}