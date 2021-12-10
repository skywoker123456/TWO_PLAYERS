using MLSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    private RagdollManagerHum m_Ragdoll;
    public float m_HitForce = 16.0f;

    // Use this for initialization
    void Start()
    {
        m_Ragdoll = GetComponent<RagdollManagerHum>();

        m_Ragdoll.RagdollEventTime = 3.0f;
        m_Ragdoll.OnTimeEnd = () =>
        {
            m_Ragdoll.BlendToMecanim();
        };
    }

    // Update is called once per frame
    void Update()
    {
        
            doHitReaction();
        
        if (Input.GetMouseButtonDown(1))
        {
            doRagdoll();
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            BodyColliderScript bcs = collision.collider.GetComponent<BodyColliderScript>();
            if (bcs.ParentObject == this.gameObject)
            {
                int[] parts = new int[] { bcs.index };
                m_Ragdoll.StartHitReaction(parts, collision.relativeVelocity * m_HitForce);
            }
        }
    }


    private void doHitReaction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mask = LayerMask.GetMask("Enemy");
        RaycastHit rhit;
        if (Physics.Raycast(ray, out rhit, 3.0f, mask))
        {
            Debug.DrawLine(transform.position, Vector3.up * 3, Color.red);
            BodyColliderScript bcs = rhit.collider.GetComponent<BodyColliderScript>();
            if (bcs.ParentObject == this.gameObject)
            {
                int[] parts = new int[] { bcs.index };
                m_Ragdoll.StartHitReaction(parts, ray.direction * m_HitForce);
            }
        }
    }

    private void doRagdoll()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mask = LayerMask.GetMask("Enemy");
        RaycastHit rhit;
        if (Physics.Raycast(ray, out rhit, 3.0f, mask))
        {
            BodyColliderScript bcs = rhit.collider.GetComponent<BodyColliderScript>();
            if (bcs.ParentObject == this.gameObject)
            {
                int[] parts = new int[] { bcs.index };
                m_Ragdoll.StartRagdoll(parts, ray.direction * m_HitForce);
            }
        }
    }
}
