// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(RagdollManager))]
    public class RagdollUserUnityTPC : MonoBehaviour, IRagdollUser
    {

        // unity third person character script reference
        private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter m_UnityCharacter;
        private RagdollManager m_Ragdoll;   // ragdoll manager reference
        private UnityEngine.AI.NavMeshAgent m_NavAgent;    // reference to nav agent is exists
        private Bounds m_Bounds;                // bounds of ragdoll user
        private CapsuleCollider m_Capsule;      // bound capsule of ragdoll user
        private Rigidbody m_Rigidbody;          // rigid body of ragdoll user
        private Animator m_Animator;            // animator component
        private bool m_Initialized = false;     // is compoenent initialized ?
        private Collider[] m_Colliders;         // for calculating bounds


        /// <summary>
        /// IRagdollUser interface
        /// gets and sets ignore hits flag
        /// </summary>
        public bool IgnoreHit { get; set; }

        /// <summary>
        /// IRagdollUser interface implementation
        /// gets bounds of character
        /// </summary>
        public Bounds Bound { get { return m_Bounds; } }

        /// <summary>
        /// IRagdollUser interface implementation
        /// gets reference to ragdoll manager component
        /// </summary>
        public RagdollManager RagdollManager { get { return m_Ragdoll; } }

        /// <summary>
        /// initialize component
        /// </summary>
        public void Initialize()
        {
            if (m_Initialized) return;

            m_UnityCharacter = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
            if (!m_UnityCharacter) { Debug.LogError("cannot find Unity ThirdPersonCharacter script."); return; }

            m_NavAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            m_Capsule = GetComponent<CapsuleCollider>();
            if (!m_Capsule) { Debug.LogWarning("Cannot find capsule collider"); return; }

            m_Rigidbody = GetComponent<Rigidbody>();
            if (!m_Rigidbody) { Debug.LogWarning("Cannot find rigidbody"); return; }

            m_Animator = GetComponent<Animator>();
            if (!m_Animator) { Debug.LogError("Cannot find animator component."); return; }

            m_Ragdoll = GetComponent<RagdollManager>();
            if (!m_Ragdoll) { Debug.LogError("cannot find 'RagdollManager' component."); return; }

            Collider col = GetComponent<Collider>();
            if (!col) { Debug.LogError("object cannot be null."); return; }

            m_Bounds = col.bounds;

            // setup important ragdoll events

            // event that will fire when hit
            m_Ragdoll.OnHit = () =>
            {
                m_UnityCharacter.simulateRootMotion = false;
                m_UnityCharacter.DisableMove = true;
                m_UnityCharacter.RigidBody.velocity = Vector3.zero;

                m_UnityCharacter.RigidBody.detectCollisions = false;
                m_UnityCharacter.RigidBody.isKinematic = true;
                m_UnityCharacter.Capsule.enabled = false;
                if (m_NavAgent) { m_NavAgent.enabled = false; }
            };

            // allow movement when transitioning to animated
            m_Ragdoll.OnStartTransition = () =>
            {
                /* 
                    Enable simulating root motion if 
                    character is not in full ragdoll to
                    make character not freeze on place when hit.
                    Otherwise root motion will interfere with getting up animation.
                */

                if (!m_Ragdoll.IsFullRagdoll && !m_Ragdoll.IsGettingUp)
                {
                    m_UnityCharacter.simulateRootMotion = true;
                    m_UnityCharacter.RigidBody.detectCollisions = true;
                    m_UnityCharacter.RigidBody.isKinematic = false;
                    m_UnityCharacter.Capsule.enabled = true;
                }
            };

            // event that will be last fired ( when full ragdoll - on get up, when hit reaction - on blend end 
            m_Ragdoll.LastEvent = () =>
            {
                m_UnityCharacter.simulateRootMotion = true;
                m_UnityCharacter.DisableMove = false;

                m_UnityCharacter.RigidBody.detectCollisions = true;
                m_UnityCharacter.RigidBody.isKinematic = false;
                m_UnityCharacter.Capsule.enabled = true;

                if (m_NavAgent) { m_NavAgent.enabled = true; }
            };

            // event that will be fired when ragdoll counter reach event time
            m_Ragdoll.RagdollEventTime = 3.0f;
            m_Ragdoll.OnTimeEnd = () =>
            {
                m_Ragdoll.BlendToMecanim();
            };

            IgnoreHit = false;


            m_Initialized = true;
        }

        // Use this for initialization
        void Start()
        {
            Initialize();
        }


        /// <summary>
        /// IRagdollUser interface implementation
        /// start hit reaction flag
        /// </summary>
        /// <param name="hitParts">hit body parts</param>
        /// <param name="hitForce">hit velocity</param>
        public void StartHitReaction(int[] hitParts, Vector3 hitForce)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized.");
                return;
            }
#endif
            m_Ragdoll.StartHitReaction(hitParts, hitForce);
        }


        /// <summary>
        /// IRagdollUser interface implementation
        /// starts full ragdoll method
        /// </summary>
        /// <param name="bodyParts">hit parts</param>
        /// <param name="bodyPartForce">force on hit parts</param>
        /// <param name="overallForce">force on all parts</param>
        public void StartRagdoll(int[] bodyParts, Vector3 bodyPartForce, Vector3 overallForce)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("component not initialized.");
                return;
            }
#endif
            m_Ragdoll.StartRagdoll(bodyParts, bodyPartForce, overallForce);
        }

#if DEBUG_INFO
        void OnDrawGizmosSelected()
        {
            calculateBounds();
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(m_Bounds.center, m_Bounds.size);
        }
#endif

        // calculate bounds by combining all ragdoll colliders
        private void calculateBounds()
        {
            if (!m_Ragdoll) m_Ragdoll = GetComponent<RagdollManager>();
#if DEBUG_INFO
            if (!m_Ragdoll) { Debug.LogError("Cannot find RagdollManager component."); return; }
#endif
            if (m_Colliders == null)
                m_Colliders = m_Ragdoll.RagdollBones[0].GetComponentsInChildren<Collider>();

            Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 vMax = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                vMin = Vector3.Min(vMin, m_Colliders[i].bounds.min);
                vMax = Vector3.Max(vMax, m_Colliders[i].bounds.max);
            }
            m_Bounds.SetMinMax(vMin, vMax);
        }
    }
}
