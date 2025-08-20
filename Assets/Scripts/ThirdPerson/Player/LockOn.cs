using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    public float lockOnRange = 10f;
    public Transform currentTarget;
    public LayerMask enemyLayer;
    public EnemyHealth health;
    public LockOnUI lockOnUI;

    private void Start()
    {
        health = GetComponent<EnemyHealth>();   
        if(lockOnUI == null)
        {
            lockOnUI = FindObjectOfType<LockOnUI>();
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // tecla para travar/destravar
        {
            if (currentTarget == null)
                LockOnTarget();
            else
            {
                if(lockOnUI != null)     
                lockOnUI.DisableTarget();
                currentTarget = null;
            }

        }
    }

    void LockOnTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);

        if (hits.Length > 0)
        {
            // pega o mais próximo
            Transform closest = hits[0].transform;
            float closestDist = Vector3.Distance(transform.position, closest.position);

            foreach (Collider c in hits)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < closestDist)
                {
                    closest = c.transform;
                    closestDist = dist;
                    
                }
            }

            currentTarget = closest;
            if(lockOnUI != null)
            lockOnUI.SetTarget(currentTarget);
        }
    }

    void LateUpdate()
    {
        if (currentTarget != null)
        {
            Vector3 lookDir = currentTarget.position - transform.position;
            lookDir.y = 0; // não inclina no eixo Y
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 10f // velocidade de rotação
            );
        }
    }
}
