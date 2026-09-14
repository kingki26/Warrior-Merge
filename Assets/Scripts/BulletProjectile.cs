using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
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

        // ========================================
        // TARGET CÒN TỒN TẠI
        // ========================================

        if (target != null)
        {
            // Liên tục cập nhật vị trí target
            targetPosition =
                target.transform.position;
        }

        // ========================================
        // BAY TỚI VỊ TRÍ TARGET
        // ========================================

        Vector3 direction =
            targetPosition -
            transform.position;

        float distance =
            direction.magnitude;

        // Đã tới vị trí target
        if (distance <= hitDistance)
        {
            HitTarget();
            return;
        }

        // ========================================
        // DI CHUYỂN
        // ========================================

        Vector3 moveDirection =
            direction.normalized;

        transform.position +=
            moveDirection *
            speed *
            Time.deltaTime;

        // ========================================
        // XOAY THEO HƯỚNG BAY
        // ========================================

        if (moveDirection != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    moveDirection
                );
        }
    }

    // ========================================
    // HIT TARGET
    // ========================================

    private void HitTarget()
    {
        // Target đã chết / bị Destroy
        if (target == null)
        {
            Debug.Log(
                gameObject.name +
                " reached target position, " +
                "but target no longer exists."
            );

            Destroy(gameObject);
            return;
        }

        UnitHealth targetHealth =
            target.GetComponent<UnitHealth>();

        // Target vẫn còn sống → gây damage
        if (targetHealth != null &&
            !targetHealth.IsDead())
        {
            Debug.Log(
                gameObject.name +
                " hit " +
                target.name +
                " | Damage: " +
                damage
            );

            targetHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}