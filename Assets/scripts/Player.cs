using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] float max_speed;
    [SerializeField] float acceleration;
    [SerializeField] float deceleration;
    [SerializeField] bool autoShoot;
    [SerializeField] float shootCoolDown;


    float speed;
    Vector2 previousDirection;
    Vector2 targetDirection;
    Vector2 direction;
    Rigidbody2D rb;
    Shooter shooter;
    float shootCoolDownTimer;
    bool shoot;
 
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shooter = GetComponentInChildren<Shooter>();
        shootCoolDownTimer = shootCoolDown;
    }

    // Update is called once per frame
    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        targetDirection = new Vector2(horizontal, vertical).normalized;

        shoot = Input.GetKey(KeyCode.Z);
    }

    void FixedUpdate() {
        if (targetDirection != Vector2.zero)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetDirection * max_speed, acceleration*Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, deceleration*Time.fixedDeltaTime);
        }

        if (autoShoot || shoot)
        {
            if (shootCoolDownTimer <= 0)
            {
                shooter.Shoot(target);
                shootCoolDownTimer = shootCoolDown;
            }
        }

        if (shootCoolDownTimer > 0)
            shootCoolDownTimer -= Time.fixedDeltaTime;
    }
}
