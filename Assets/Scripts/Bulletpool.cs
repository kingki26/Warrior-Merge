using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [Header("Bullet Prefabs")]
    [SerializeField] private BulletProjectile[] bulletPrefabs;

    [Header("Initial Size")]
    [SerializeField] private int[] initialSizes;

    private Dictionary<BulletProjectile, Queue<BulletProjectile>> bulletPools = new Dictionary<BulletProjectile, Queue<BulletProjectile>>();

    private void Awake()
    {
        CreateInitialBullets();
    }

    private void CreateInitialBullets()
    {
        if (bulletPrefabs == null || initialSizes == null)
        {
            return;
        }

        if (bulletPrefabs.Length != initialSizes.Length)
        {
            return;
        }

        for (int i = 0; i < bulletPrefabs.Length; i++)
        {
            BulletProjectile prefab = bulletPrefabs[i];

            if (prefab == null)
            {
                continue;
            }

            Queue<BulletProjectile> pool = new Queue<BulletProjectile>();
            bulletPools.Add(prefab, pool);

            for (int j = 0; j < initialSizes[i]; j++)
            {
                BulletProjectile bullet = CreateBullet(prefab);
                pool.Enqueue(bullet);
            }
        }
    }

    private BulletProjectile CreateBullet(BulletProjectile prefab)
    {
        BulletProjectile bullet = Instantiate(prefab, transform);
        bullet.InitPool(this, prefab);
        bullet.ResetBullet();
        bullet.gameObject.SetActive(false);
        return bullet;
    }

    public BulletProjectile GetBullet(BulletProjectile prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        if (!bulletPools.TryGetValue(prefab, out Queue<BulletProjectile> pool))
        {
            return null;
        }

        BulletProjectile bullet;

        if (pool.Count > 0)
        {
            bullet = pool.Dequeue();
        }
        else
        {
            bullet = CreateBullet(prefab);
        }

        bullet.gameObject.SetActive(true);

        return bullet;
    }

    public void ReturnBullet(BulletProjectile bullet, BulletProjectile prefab)
    {
        if (bullet == null || prefab == null)
        {
            return;
        }

        if (!bulletPools.TryGetValue(prefab, out Queue<BulletProjectile> pool))
        {
            bullet.gameObject.SetActive(false);
            return;
        }

        bullet.ResetBullet();
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }
}