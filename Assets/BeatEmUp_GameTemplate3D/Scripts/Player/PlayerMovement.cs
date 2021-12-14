using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnitState))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour {

	[Header("Linked Components")]
	private UnitAnimator animator;
	private Rigidbody rigidbody;
	private UnitState playerState;
	private CapsuleCollider collider;

	[Header("Settings")]
	public float WalkSpeed = 3f;
	public float RunSpeed = 6f;
	public float JumpForce = 8f;
	public float RotationSpeed = 15f;
	public float JumpRotationSpeed = 30f;
	public float LookAheadDistance = 0.2f;
	public float LandRecoveryTime = 0.1f;
	public float LandTime = 0f;
	public LayerMask CollisionLayer;

	[Header("Audio")]
	public string JumpUpVoice = "";
	public string JumpLandVoice = "";

	[Header("Stats")]
	public DIRECTION CurrentDirection;
	public Vector2 InputDirection;
	public bool JumpInProgress;

	private bool isDead = false;
	private bool jumpNextFixedUpdate;

	//Список состояний в которых возможно движение
	private List<UNITSTATE> movementStates = new List<UNITSTATE>
	{
		UNITSTATE.IDLE,
		UNITSTATE.WALK,
		UNITSTATE.RUN,
		UNITSTATE.JUMPING,
		UNITSTATE.JUMPKICK,
		UNITSTATE.LAND,
		UNITSTATE.DEFEND,
	};

	//Включени/выключение Событий инпута
	void OnEnable()
    {
		NewInputManager.onInputEvent += OnInputEvent;
		NewInputManager.onDirectionInputEvent += OnDirectionInputEvent;
	}

	void OnDisable()
    {
		NewInputManager.onInputEvent -= OnInputEvent;
		NewInputManager.onDirectionInputEvent -= OnDirectionInputEvent;
	}

	void Start()
    {
		//поиск компонентов
		if (!animator) animator = GetComponentInChildren<UnitAnimator>();
		if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
		if (!playerState) playerState = GetComponent<UnitState>();
		if (!collider) collider = GetComponent<CapsuleCollider>();

		//сообщения об ошибках
		if (!animator) Debug.LogError("No animator found inside " + gameObject.name);
		if (!rigidbody) Debug.LogError("No Rigidbody component found on " + gameObject.name);
		if (!playerState) Debug.LogError("No UnitState component found on " + gameObject.name);
		if (!collider) Debug.LogError("No Capsule Collider found on " + gameObject.name);
	}

	void FixedUpdate()
	{
		/////////////////////
		//ТЕСТ//
		//Debug.Log("horizontal player speed: " + new Vector2(rb.velocity.x, rb.velocity.z).magnitude);
		/////////////////////

		//если не разрешенное состояние playerState или игрок мертв -> выход
		if (!movementStates.Contains(playerState.currentState) || isDead) return;

		//блок
		if (playerState.currentState == UNITSTATE.DEFEND)
		{
			TurnToCurrentDirection();
			return;
		}

		//прыжок
		if (jumpNextFixedUpdate)
		{
			Jump();
			return;
		}

		//приземление
		if (JumpInProgress && IsGrounded())
        {
			HasLanded();
			return;
		}

        //восстановление после приземления, переход в IDLE
        if (playerState.currentState == UNITSTATE.LAND && Time.time - LandTime > LandRecoveryTime) playerState.SetState(UNITSTATE.IDLE);

		//передвижение
		bool isGrounded = IsGrounded();
		animator.SetAnimatorBool("isGrounded", isGrounded);
		if (isGrounded) animator.SetAnimatorBool("Falling", false);

		if (isGrounded)
        {
            MoveGrounded();
		}
        else
        {
			MoveAirborne();
		}

		//поворот к направлению инпута
		TurnToCurrentDirection();
	}

	//Движение на замле
	void MoveGrounded()
	{
		//если приземляемся то ничего не делаем
		if (playerState.currentState == UNITSTATE.LAND) return;

		//если есть ригибоди и если есть инпут и перед игроком нет препятствия
		if (rigidbody != null && InputDirection.sqrMagnitude > 0 && !WallInFront())
		{
			//выбор скорости передвижения
			float movementSpeed = playerState.currentState == UNITSTATE.RUN? RunSpeed : WalkSpeed;

			rigidbody.velocity = new Vector3(InputDirection.x * -movementSpeed, rigidbody.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, InputDirection.y * -movementSpeed);
			if (animator) animator.SetAnimatorFloat("MovementSpeed", rigidbody.velocity.magnitude);
		}
		else
		{
			//только граыитация, без движения
			rigidbody.velocity = new Vector3(0, rigidbody.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0);

			if (animator) animator.SetAnimatorFloat("MovementSpeed", 0);
			playerState.SetState(UNITSTATE.IDLE);
		}

		//устанавливаем состояние бега в аниматор
		animator.SetAnimatorBool("Run", playerState.currentState == UNITSTATE.RUN);
	}

	//Движение в воздухе
	void MoveAirborne()
	{
		//падение
		if (rigidbody.velocity.y < 0.1f && playerState.currentState != UNITSTATE.KNOCKDOWN) animator.SetAnimatorBool("Falling", true);
	}

	//Прыжок
	void Jump()
	{
		playerState.SetState(UNITSTATE.JUMPING);
		jumpNextFixedUpdate = false;
		JumpInProgress = true;
		rigidbody.velocity = new Vector3(rigidbody.velocity.x, JumpForce, rigidbody.velocity.z);

		//анимация
		animator.SetAnimatorBool("JumpInProgress", true);
		animator.SetAnimatorBool("Run", false);
		animator.SetAnimatorTrigger("JumpUp");
		animator.ShowDustEffectJump();

		//звук
		if (JumpUpVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(JumpUpVoice, transform.position);
	}

	//Приземление
	void HasLanded()
	{
		JumpInProgress = false;
		playerState.SetState(UNITSTATE.LAND);
		rigidbody.velocity = Vector2.zero;
		LandTime = Time.time;

		//настройки аниматора
		animator.SetAnimatorFloat("MovementSpeed", 0f);
		animator.SetAnimatorBool("JumpInProgress", false);
		animator.SetAnimatorBool("JumpKickActive", false);
		animator.SetAnimatorBool("Falling", false);
		animator.ShowDustEffectLand();

		//звук
		GlobalAudioPlayer.PlaySFX("FootStep");
		if (JumpLandVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(JumpLandVoice, transform.position);
	}

    #region controller input

    //Событие инпута направления
    void OnDirectionInputEvent(Vector2 dir, bool doubleTapActive)
	{
		//ignore input when we are dead or when this state is not active
		if (!movementStates.Contains(playerState.currentState) || isDead) return;

		//set current direction based on the input vector
		InputDirection = dir.normalized;

		//start running on double tap
		if (doubleTapActive && IsGrounded()) playerState.SetState(UNITSTATE.RUN);
	}

	//Событие инпута кнопок
	void OnInputEvent(string action, BUTTONSTATE buttonState)
    {
		//ignore input when we are dead or when this state is not active
		if (!movementStates.Contains(playerState.currentState) || isDead) return;

		//start a jump
		if (action == "Jump" && buttonState == BUTTONSTATE.PRESS && IsGrounded() && playerState.currentState != UNITSTATE.JUMPING) jumpNextFixedUpdate = true;

		//start running when a run button is pressed (e.g. Joypad controls)
		//if (action == "Run") playerState.SetState(UNITSTATE.RUN);
	}

	#endregion
		
	//interrups an ongoing jump
	public void CancelJump(){
		JumpInProgress = false;
	}
		
	//set current direction
	public void SetDirection(DIRECTION dir) {
		CurrentDirection = dir;
		if(animator) animator.currentDirection = CurrentDirection;
	}

	//returns the current direction
	public DIRECTION getCurrentDirection() {
		return CurrentDirection;
	}

	//returns true if the player is grounded
	public bool IsGrounded() {

		//check for capsule collisions with a 0.1 downwards offset from the capsule collider
		Vector3 bottomCapsulePos = transform.position + (Vector3.up) * (collider.radius - 0.1f);
		return Physics.CheckCapsule(transform.position + collider.center, bottomCapsulePos, collider.radius, CollisionLayer);
	}
		
	//look (and turns) towards a direction
	public void TurnToCurrentDirection()
	{

		if (InputDirection != Vector2.zero)
		{
			Vector3 inputDirection3d = new Vector3(-InputDirection.y, 0f, InputDirection.x);

			float turnSpeed = JumpInProgress ? JumpRotationSpeed : RotationSpeed;
			transform.rotation = Quaternion.Slerp(a: transform.rotation, b: Quaternion.LookRotation(inputDirection3d), t: turnSpeed * Time.fixedDeltaTime);
		}
		/*
		if (currentDirection == DIRECTION.Right || currentDirection == DIRECTION.Left) {
			float turnSpeed = jumpInProgress? jumpRotationSpeed : rotationSpeed;
			Vector3 newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * -(int)currentDirection, turnSpeed * Time.fixedDeltaTime, 0.0f);
			transform.rotation = Quaternion.LookRotation(newDir);
		}
		*/
	}

	//update the direction based on the current input		?????
	/*public void updateDirection()
	{
		TurnToCurrentDirection();
	}*/

	/*void Death()										?????
	 *{
		isDead = true;
		rb.velocity = Vector3.zero;
	}*/

	//returns true if there is a environment collider in front of us
	bool WallInFront() {
		var MovementOffset = new Vector3(InputDirection.x, 0, InputDirection.y) * LookAheadDistance;
		var c = GetComponent<CapsuleCollider>();
		Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up * (c.radius + .1f) + -MovementOffset, c.radius, CollisionLayer);

		int i = 0;
		bool hasHitwall = false;
		while(i < hitColliders.Length) {
			if(CollisionLayer == (CollisionLayer | 1 << hitColliders[i].gameObject.layer)) hasHitwall = true;
			i++;
		}
		return hasHitwall;
	}

	//draw a lookahead sphere in the unity editor
	#if UNITY_EDITOR
	void OnDrawGizmos()
    {
		var c = GetComponent<CapsuleCollider>();
		Gizmos.color = Color.yellow;
		Vector3 MovementOffset = new Vector3(InputDirection.x, 0, InputDirection.y) * LookAheadDistance;
		Gizmos.DrawWireSphere(transform.position + Vector3.up * (c.radius + .1f) + -MovementOffset, c.radius);
	}
	#endif
}

public enum DIRECTION
{
	Right = -1,
	Left = 1,
	Up = 2,
	Down = -2,
};