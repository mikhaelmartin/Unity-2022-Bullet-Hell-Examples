using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [SerializeField] bool use_interpolation = true;
    Dictionary<MultiBullet,MultiBullet> multiBulletDict = new Dictionary<MultiBullet, MultiBullet>();
    // Dictionary<MultiBulletParallel,MultiBulletParallel> multiBulletParallelDict = new Dictionary<MultiBulletParallel, MultiBulletParallel>();

    public void Spawn(
        Vector2 position,
        float rotation,
        MonoBehaviour bulletPrefab,
        BulletData bulletData,
        GameObject target = null
    )
    {
        if (bulletPrefab is Bullet)
        {
            var bullet = Instantiate(bulletPrefab);
            bullet.transform.SetParent(this.transform);
            bullet.GetComponent<Bullet>().SetUp(position, rotation, bulletData, target);
        }

        else if (bulletPrefab is MultiBullet)
        {
            MultiBullet multiBulletPrefab = bulletPrefab as MultiBullet;

            if (!multiBulletDict.ContainsKey(multiBulletPrefab)) 
            {
                MultiBullet multiBullet = Instantiate(multiBulletPrefab);
                multiBullet.transform.SetParent(this.transform);
                multiBulletDict[multiBulletPrefab] = multiBullet;
            }

            multiBulletDict[multiBulletPrefab].AddBullet(position, rotation, bulletData, target);           
        }

        // else if (bulletPrefab is MultiBulletParallel)
        // {
        //     MultiBulletParallel multiBulletPrefab = bulletPrefab as MultiBulletParallel;

        //     if (!multiBulletParallelDict.ContainsKey(multiBulletPrefab)) 
        //     {
        //         MultiBulletParallel multiBullet = Instantiate(multiBulletPrefab);
        //         multiBullet.transform.SetParent(this.transform);
        //         multiBulletParallelDict[multiBulletPrefab] = multiBullet;
        //     }

        //     multiBulletParallelDict[multiBulletPrefab].AddBullet(position, rotation, bulletData, target);           
        // }
    }
}
