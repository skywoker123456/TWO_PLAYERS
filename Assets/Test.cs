using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLSpace;

public class Test : MonoBehaviour
{
    RagdollManager m_Ragdoll;
    public float m_HitForce;
    void Start()
    {
        m_Ragdoll = GetComponent<RagdollManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            m_Ragdoll.StartRagdoll();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            m_Ragdoll.BlendToMecanim();
        }

//        if (Input.GetMouseButtonDown(0))
//        {
//            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//            int mask = 1 << LayerMask.NameToLayer("Enemy");
//            RaycastHit hit;
//            if(Physics.Raycast(ray,out hit, 120f, mask))
//            {
//                BodyColliderScript bcs = hit.collider.GetComponent<BodyColliderScript>();
//                BodyParts[] parts = new BodyParts[] { bcs.bodyPart };
//                m_Ragdoll.StartHitReaction(parts, ray.direction * m_HitForce);
//            }
//        }
//        if (Input.GetMouseButtonDown(1))
//        {
//            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//            int mask = 1 << LayerMask.NameToLayer("Enemy");
//            RaycastHit hit;
//            if (Physics.Raycast(ray, out hit, 120f, mask))
//            {
//                BodyColliderScript bcs = hit.collider.GetComponent<BodyColliderScript>();
//                BodyParts[] parts = new BodyParts[] { bcs.bodyPart };
//                m_Ragdoll.StartRagdoll(parts, ray.direction * m_HitForce);
//            }
       }
   }
//}
