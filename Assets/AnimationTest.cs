using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    { 
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool spacePressed = Input.GetButtonDown("Jump");
        bool firePressed = Input.GetButtonDown("Fire1");

        if(spacePressed){
            animator.SetBool("Animate", true);
        }

        if(firePressed)
            animator.SetBool("Animate", false);
    }
}
