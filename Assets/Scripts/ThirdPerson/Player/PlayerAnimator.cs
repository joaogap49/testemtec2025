using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    [SerializeField] PlayerThird player;
    private int damageIndex;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        damageIndex = animator.GetLayerIndex("DamageLayer");
    }
    private void Update()
    {
        animator.SetBool("IsRunning", player.IsSprinting());
        animator.SetBool("IsWalking", player.IsWalking());
        if(player.IsDamaged())
        {
            animator.SetTrigger("damaged");
            
        }
    }
    public IEnumerator SmoothLayerTransition(float targetWeight, float duration)
    {
        float startWeight = animator.GetLayerWeight(damageIndex);
        float currentWeight = startWeight;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            currentWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime / duration);
            animator.SetLayerWeight(damageIndex, currentWeight);
            yield return null;
        }
        animator.SetLayerWeight(damageIndex, targetWeight);
    }
}
