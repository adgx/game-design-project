using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    [SerializeField] private float maxRotationSpeed = 200f;
    [SerializeField] private float sightRange = 20;
    [SerializeField] private LayerMask whatIsEnemy;
    private float playerPivotOffset;
    private float heightY;
    private bool isActiveLock = false;
    private bool enemyLocked = false;
    private PlayerInput input;
    private PlayerShoot playerShoot;
    private Transform mainCamTransform;
    private Collider closestEnemy = null;
    [SerializeField] private Animator camAnim;
    [SerializeField] private Transform lockOnCanvas;
    [SerializeField] private Transform enemyTargetLocator;
    [SerializeField] private float canvaScale = 1.0f;


    public void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerShoot = GetComponent<PlayerShoot>();

    }

    public void Start()
    {
        mainCamTransform = Camera.main.transform;
        lockOnCanvas.gameObject.SetActive(false);

        if (camAnim == null)
        {
            Debug.Log("Missing the animator for the cinemachine");
        }
    }

    void FixedUpdate()
    {
        //check if the player wants to active the lockon feature
        if (PlayerInput.Instance.LockPressed())
        {
            isActiveLock = !isActiveLock;
            if (!isActiveLock)
                ResetTarget();
        }
        if (isActiveLock && !enemyLocked)
        {
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, sightRange, whatIsEnemy);
            //bool enemiesPresent = enemiesInRange.Length > 0;
            if (enemiesInRange.Length > 0)
            {
                // We must inform PlayerShoot of the combat status
                //playerShoot.isInCombat = enemiesPresent;

                if (!playerShoot.cannotAttack)
                {
                    LookAtClosestEnemy(enemiesInRange);
                }
            }
            else isActiveLock = false;
        }
        else if (isActiveLock && enemyLocked)
        {
            if (!checkEnemyTarget())
            {
                isActiveLock = false;
                ResetTarget();
            }
        }
    }

    private void LookAtClosestEnemy(Collider[] enemies)
    {
        
        float minDistance = float.MaxValue;

        foreach (Collider enemyCollider in enemies)
        {
            //float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
            //if (distance < minDistance) {
            //    minDistance = distance;
            //    closestEnemy = enemyCollider.transform;
            //}
            float distance = Vector3.Angle(mainCamTransform.forward, enemyCollider.transform.position - mainCamTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemyCollider;

            }
        }

        if (closestEnemy != null)
        {

            Vector3 direction = closestEnemy.transform.position - transform.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * maxRotationSpeed);
            transform.rotation = rotation;
            if (checkEnemyTarget()) FoundTarget();
        }
    }

    bool checkEnemyTarget()
    {
        if (closestEnemy == null) return false;

        //check the distance
        Vector3 dist = closestEnemy.transform.position - mainCamTransform.position;

        if (dist.magnitude > sightRange) return false;

        //update the position of UI and check the occlusion
        float h1 = closestEnemy.bounds.size.y;
        float delta = h1 * 0.7f;
        Vector3 tarPos = closestEnemy.transform.position + new Vector3(0.0f, delta, 0.0f);
        enemyTargetLocator.position = tarPos;
        lockOnCanvas.position = tarPos;

        if (Blocked(tarPos)) return false;

        //fix the scale
        lockOnCanvas.localScale = Vector3.one * ((mainCamTransform.position - lockOnCanvas.position).magnitude * canvaScale);
        return true;
    }

    void FoundTarget()
    {
        lockOnCanvas.gameObject.SetActive(true);
        camAnim.Play("TargetCamera");
        enemyLocked = true;
    }

    void ResetTarget()
    {
        lockOnCanvas.gameObject.SetActive(false);
        enemyLocked = false;
        camAnim.Play("FollowCamera");

    }


    bool Blocked(Vector3 t)
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + Vector3.up * 1f, t, out hit)){
            if(hit.collider.gameObject.layer != (int)ORF.Utils.Layers.Enemy) return true;
        }
        return false;
    }    
}