using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    [Header("LockOn Settings")]
    public float lockOnRange = 10f;
    public LayerMask enemyLayer; // selecione a layer dos inimigos no Inspector
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Refs (optional)")]
    public LockOnUI lockOnUI; // seu UI handler se tiver

    // runtime
    public Transform currentTarget { get; private set; }
    private Transform previousTarget;

    private void Start()
    {
        if (lockOnUI == null)
            lockOnUI = FindObjectOfType<LockOnUI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (currentTarget == null)
                TryLockOn();
            else
                UnlockCurrent();
        }

        // se o target for destruído/removido, limpa
        if (currentTarget != null && (currentTarget.gameObject == null || !currentTarget.gameObject.activeInHierarchy))
        {
            UnlockCurrent();
        }
    }

    private void TryLockOn()
    {
        Collider[] hits;
        if (enemyLayer.value == 0)
            hits = Physics.OverlapSphere(transform.position, lockOnRange);
        else
            hits = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);

        if (hits == null || hits.Length == 0) return;

        Transform closest = null;
        float closestDist2 = float.MaxValue;

        foreach (var c in hits)
        {
            // tenta encontrar o componente EnemyHealth no pai do collider
            EnemyHealth eh = c.GetComponentInParent<EnemyHealth>();
            if (eh == null) continue; // ignora colliders que não pertencem a inimigos

            Transform candidate = eh.transform;

            float d2 = (candidate.position - transform.position).sqrMagnitude;
            if (d2 < closestDist2)
            {
                closestDist2 = d2;
                closest = candidate;
            }
        }

        if (closest != null)
        {
            // garante que desliga o antigo target visual
            if (currentTarget != null)
                SetTargetLocked(currentTarget, false);

            previousTarget = currentTarget;
            currentTarget = closest;

            SetTargetLocked(currentTarget, true);

            if (lockOnUI != null)
                lockOnUI.SetTarget(currentTarget);
        }
    }

    private void UnlockCurrent()
    {
        if (currentTarget == null) return;

        SetTargetLocked(currentTarget, false);

        if (lockOnUI != null)
            lockOnUI.DisableTarget();

        previousTarget = currentTarget;
        currentTarget = null;
    }

    // helper: chama o método no EnemyLockVisual do inimigo (se existir)
    private void SetTargetLocked(Transform targetRoot, bool locked)
    {
        if (targetRoot == null) return;
        EnemyLockVisual vis = targetRoot.GetComponent<EnemyLockVisual>();
        if (vis != null)
        {
            vis.SetLocked(locked);
        }
        else
        {
            // fallback: tenta encontrar em children caso componente não esteja no root
            vis = targetRoot.GetComponentInChildren<EnemyLockVisual>();
            if (vis != null) vis.SetLocked(locked);
        }
    }

    private void LateUpdate()
    {
        if (currentTarget != null)
        {
            Vector3 lookDir = currentTarget.position - transform.position;
            lookDir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
        }
    }
}
