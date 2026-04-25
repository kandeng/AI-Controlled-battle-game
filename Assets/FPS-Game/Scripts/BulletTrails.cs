using System.Collections;
using UnityEngine;

public class BulletTrails : MonoBehaviour
{
    private float delayTime = 1f;
    private float counter = 0f;

    public bool Automatic;
    private float CurrentCoolDown;
    [SerializeField] private float FireCoolDown;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float fireRate;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;

    private Quaternion targetRotation;
    float targetAngleX;
    float targetAngleY;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        counter += Time.deltaTime * fireRate;
        if (counter <= delayTime) return;
        counter = 0f;
    }

    // Programmatic shoot — called externally instead of keyboard polling
    public void TriggerShoot()
    {
        if (CurrentCoolDown <= 0f)
        {
            CurrentCoolDown = FireCoolDown;
            Bullet bullet = BulletManager.Instance.GetBullet();
            bullet.transform.position = bulletSpawnPoint.position;
            Vector3 forceDirection = transform.forward * speed;
            bullet.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);
            bullet.StartCountingToDisappear();
        }
    }

    // Programmatic turret rotation — called externally
    public void RotateTurret(float deltaYaw, float deltaPitch)
    {
        targetAngleY += deltaYaw;
        targetAngleX += deltaPitch;
    }

    private void FixedUpdate()
    {
        targetRotation = Quaternion.Euler(targetAngleX, targetAngleY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        CurrentCoolDown -= Time.deltaTime;
    }
}
