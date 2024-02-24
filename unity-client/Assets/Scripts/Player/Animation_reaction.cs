using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_reaction : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("walkSpeed", Mathf.Abs(rigidbody.velocity.x));
        //if(transform.forward == Vector3)
    }

    public void Sit()
    {
        animator.SetBool("doSit", true);
    }
}
