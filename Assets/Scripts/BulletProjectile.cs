using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitEffectDuration = 0.3f;

    [Header("Bullet Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float hitDistance = 0.2f;

    private Unit target;
    private int damage;

    private bool initialized;

    private Vector3 targetPosition;

    private BulletPool bulletPool;
    private BulletProjectile bulletPrefab;

    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void InitPool(BulletPool pool, BulletProjectile prefab)
    {
        bulletPool = pool;
        bulletPrefab = prefab;
    }

    public void Setup(Unit newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;

        if (target != null)
        {
            targetPosition = target.transform.position;
        }

        initialized = true;

        PlayParticleEffects();
    }

    public void ResetBullet()
    {
        target = null;
        damage = 0;
        targetPosition = Vector3.zero;
        initialized = false;

        StopParticleEffects();
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (target != null)
        {
            targetPosition = target.transform.position;
        }

        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;

        if (distance <= hitDistance)
        {
            HitTarget();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        transform.position += moveDirection * speed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void HitTarget()
    {
        if (hitEffect != null)
        {
            GameObject effect;

            if (target != null)
            {
                effect = Instantiate(hitEffect, target.transform.position, Quaternion.identity);
            }
            else
            {
                effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            Destroy(effect, hitEffectDuration);
        }

        if (target == null)
        {
            ReturnToPool();
            return;
        }

        UnitHealth targetHealth = target.Health;

        if (targetHealth != null && !targetHealth.IsDead())
        {
            targetHealth.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void PlayParticleEffects()
    {
        if (particleSystems == null)
        {
            return;
        }

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Play(true);
        }
    }

    private void StopParticleEffects()
    {
        if (particleSystems == null)
        {
            return;
        }

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void ReturnToPool()
    {
        if (bulletPool == null || bulletPrefab == null)
        {
            gameObject.SetActive(false);
            return;
        }

        ResetBullet();
        bulletPool.ReturnBullet(this, bulletPrefab);
    }
}