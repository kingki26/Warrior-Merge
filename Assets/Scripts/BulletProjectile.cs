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

    // Vị trí cuối cùng của target
    private Vector3 targetPosition;

    public void Setup(Unit newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;

        // Lưu vị trí ban đầu của target
        if (target != null)
        {
            targetPosition =
                target.transform.position;
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

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
                effect = Instantiate(hitEffect,target.transform.position,Quaternion.identity,target.transform);
            }
            else
            {
                effect = Instantiate(hitEffect,transform.position,Quaternion.identity);
            }

            Destroy(effect,hitEffectDuration);
        }
        if (target == null)
        {
            

            Destroy(gameObject);
            return;
        }

        UnitHealth targetHealth = target.GetComponent<UnitHealth>();

        if (targetHealth != null &&
            !targetHealth.IsDead())
        {

            targetHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}