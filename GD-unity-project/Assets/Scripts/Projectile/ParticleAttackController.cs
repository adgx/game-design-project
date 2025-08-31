using System;
using UnityEngine;
using System.Collections.Generic;
using Audio;

public class ParticleAttackController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _attackPS;
    //acceleration
    [SerializeField] private float _a = 13f;
    private float _g = -9.81f;
    [SerializeField] private float _vyInit = 0.2f;
    //velocity max
    [SerializeField] private float _vMax = 15f;

    public Transform targetPos;
    private Vector3 _destPos;
    private float _currentVF;
    private float _currentVUp;
    private float _t = 0;
    public float initialPlayerBulletDamage = 50, enemyBulletDamage = 20;
    public float playerBulletDamage;
    public PlayerShoot.DamageTypes maynardDamageType = PlayerShoot.DamageTypes.MaynardDistanceAttack;

    void Start()
    {
        // This section of code only applies to projectiles that use a _currentVF and _currentVUp based motion system,
        // or Player's bullets and Incognito's bullets. Maynard's bullets, which use a Rigidbody for movement, do not
        // they should execute this logic.
        if (gameObject.CompareTag("PlayerProjectile") || gameObject.CompareTag("SpitEnemyAttack"))
        {
            _currentVF = 0f;
            _currentVUp = _vyInit;
            gameObject.SetActive(false); // Turn off for a moment to set LookAt correctly

            if (gameObject.CompareTag("PlayerProjectile"))
            {
                if (targetPos != null)
                {
                    transform.LookAt(targetPos.position + targetPos.forward);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: 'targetPos' not assigned for PlayerProjectile. The bullet may not be looking in the correct direction.");
                }
            }
            else if (gameObject.CompareTag("SpitEnemyAttack"))
            {
                if (targetPos != null)
                {
                    _destPos = targetPos.position;
                    //offset
                    _destPos.y += 1f;
                    transform.LookAt(_destPos);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: 'targetPos' not assigned for SpitEnemyAttack. The bullet may not be looking in the correct direction.");
                }
            } 
            
            gameObject.SetActive(true); // Reactivate GameObject

            // Checks if _attackPS was assigned before attempting to use it
            if (_attackPS != null)
            {
                _attackPS.Play();
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Unassigned ParticleSystem. If this bullet should have a ParticleSystem, check the Inspector.");
            }
        }
    }
    
    void Update()
    {
        if (gameObject.CompareTag("PlayerProjectile") || gameObject.CompareTag("SpitEnemyAttack"))
        {
            if (_currentVF < _vMax)
            {
                _t += Time.deltaTime * _a / _vMax;
                _currentVF = Mathf.Lerp(0f, _vMax, _t);
            }

            _currentVUp = _g * Time.deltaTime;
        
            Vector3 sDirF = transform.forward * _currentVF * Time.deltaTime;
            Vector3 sDirUp = transform.up * _currentVUp * Time.deltaTime;
            transform.position += sDirF + sDirUp;   
        }
    }
    
    void OnParticleCollision(GameObject other)
    {
        // Only projectiles with a ParticleSystem should handle this collision
        if (_attackPS != null)
        {
            List<ParticleCollisionEvent> ce = new();
            _attackPS.GetCollisionEvents(other, ce);

            if (gameObject.CompareTag("SpitEnemyAttack"))
            {
                if (other.CompareTag("Player"))
                {
                    PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();
                    if (playerShoot != null && ce.Count > 0)
                    {
                        playerShoot.TakeDamage(enemyBulletDamage, PlayerShoot.DamageTypes.Spit, Math.Sign(ce[0].normal.x), Math.Sign(ce[0].normal.z));
                    }
                    Destroy(gameObject);
                }
                else if (other.CompareTag("PlayerProjectile"))
                {
                    Destroy(gameObject); // Destroy Incognito's bullet
                    Destroy(other);      // Destroy Player's bullet
                }
                else if (!other.CompareTag("EnemyIncognito"))
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Audio management: avoid the destruction of the projectile if the other collider is the
        // box collider used for handling ambience sounds
        if (other.gameObject.layer == LayerMask.NameToLayer("Room") || other.gameObject.CompareTag("Sphere"))
        {
            return;
        }
        
        // To avoid projectile disappearing after colliding with door's collider  
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable") || other.gameObject.CompareTag("Sphere"))
        {
            return;
        }
        
        // Logic for Incognito's bullets
        if (gameObject.CompareTag("SpitEnemyAttack"))
        {
            if (other.gameObject.CompareTag("PlayerProjectile")) // Incognito's bullet hits Player's bullet
            {
                Destroy(gameObject); // Destroy Incognito's bullet
                Destroy(other.gameObject); // Destroy Player's bullet
            }
        }
        
        // Logic for Player's bullets
        if (gameObject.CompareTag("PlayerProjectile"))
        {
            if (other.gameObject.tag.Contains("Enemy") && !other.gameObject.CompareTag("SpitEnemyAttack") && !other.gameObject.CompareTag("MaynardEnemyAttack"))
            {
                other.gameObject.GetComponent<Enemy.EnemyManager.IEnemy>()?.TakeDamage(playerBulletDamage, "d", false);
                Destroy(gameObject);
            }
            else if (other.gameObject.CompareTag("SpitEnemyAttack") || other.gameObject.CompareTag("MaynardEnemyAttack")) // Player's bullet hits an enemy bullet
            {
                Destroy(gameObject); // Destroy enemy's bullet
                Destroy(other.gameObject); // Destroy Player's bullet
            }
            else
            {
                // Collision with something else that is not an enemy or an enemy bullet, destroy the player's bullet
                Destroy(gameObject);
            }
            
            // Audio management
            GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerDistanceAttackImpact, transform.position);
            
            return; // Exit after handling the player's bullet collision
        }

        // Logic for Maynard's bullets
        if (gameObject.CompareTag("MaynardEnemyAttack"))
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();
                if (playerShoot != null)
                {
                    // Determine the direction of the shot based on contact
                    Vector3 contactPoint = other.ClosestPoint(transform.position);
                    Vector3 normal = (transform.position - contactPoint).normalized;
                    playerShoot.TakeDamage(enemyBulletDamage, maynardDamageType, Math.Sign(normal.x), Math.Sign(normal.z));
                }
                Destroy(gameObject);
            }
            else if (other.gameObject.CompareTag("PlayerProjectile")) // Maynard's bullet hits Player's bullet
            {
                Destroy(gameObject); // Destroy Maynard's bullet
                Destroy(other.gameObject); // Destroy Player's bullet
            }
            else if (!other.gameObject.tag.Contains("Enemy") && !other.gameObject.CompareTag("SpitEnemyAttack"))
            {
                // Hit something other than the player, shield, player bullet or other enemy/enemy bullet
                Destroy(gameObject);
            }
        }
    }
}
