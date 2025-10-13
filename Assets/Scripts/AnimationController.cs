using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public RuntimeAnimatorController animatorController;
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
    }
}