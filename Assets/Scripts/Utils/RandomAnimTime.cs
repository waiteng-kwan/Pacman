using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimTime : MonoBehaviour
{
    public Animator animator;
    [Range(0f, 1f)]
    public float randomMin, randomMax;
    public bool startRandomState = false;

    

    // Start is called before the first frame update
    void Start()
    {
        if(!animator)
            animator = GetComponent<Animator>();
        
        animator.speed = Random.Range(randomMin, randomMax);

        var stateMachine = animator.GetCurrentAnimatorStateInfo(0);
        //print(stateMachine.length);
    }
}
