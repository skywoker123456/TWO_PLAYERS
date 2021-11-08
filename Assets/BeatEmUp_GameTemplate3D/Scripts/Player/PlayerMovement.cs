using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnitState))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour {

	[Header("Linked Components")]
	private UnitAnimator animator;
	private Rigidbody rb;
	private UnitState playerState;
	private CapsuleCollider capsule;

	[Header("Settings")]
	public float walkSpeed = 3f;
	public float runSpeed = 6f;
	public float JumpForce = 8f;
	public float rotationSpeed = 15f;
	public float jumpRotationSpeed = 30f;
	public float lookAheadDistance = 0.2f;
	public float landRecoveryTime = 0.1f;
	public float landTime = 0f;
	public LayerMask CollisionLayer;

	[Header("Audio")]
	public string jumpUpVoice = "";
	public string jumpLandVoice = "";

	[Header("Stats")]
	public DIRECTION currentDirection;
	public Vector2 inputDirection;
	public bool jumpInProgress;

	private bool isDead = false;
	private bool JumpNextFixedUpdate;

	//Список состояний в которых возможно движение
	private List<UNITSTATE> MovementStates = new List<UNITSTATE> {
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
		InputManager.onInputEvent += OnInputEvent;
		InputManager.onDirectionInputEvent += OnDirectionInputEvent;
	}

	void OnDisable()
    {
		InputManager.onInputEvent -= OnInputEvent;
		InputManager.onDirectionInputEvent -= OnDirectionInputEvent;
	}

	void Start()
    {
		//поиск компонентов
		if (!animator) animator = GetComponentInChildren<UnitAnimator>();
		if (!rb) rb = GetComponent<Rigidbody>();
		if (!playerState) playerState = GetComponent<UnitState>();
		if (!capsule) capsule = GetComponent<CapsuleCollider>();

		//сообщения об ошибках
		if (!animator) Debug.LogError("No animator found inside " + gameObject.name);
		if (!rb) Debug.LogError("No Rigidbody component found on " + gameObject.name);
		if (!playerState) Debug.LogError("No UnitState component found on " + gameObject.name);
		if (!capsule) Debug.LogError("No Capsule Collider found on " + gameObject.name);
	}

	void FixedUpdate()
	{
		/////////////////////
		//ТЕСТ//
		Debug.Log("horizontal player speed: " + new Vector2(rb.velocity.x, rb.velocity.z).magnitude);
		/////////////////////

		//если не разрешенное состояние playerState или игрок мертв -> выход
		if (!MovementStates.Contains(playerState.currentState) || isDead) return;

		//блок
		if (playerState.currentState == UNITSTATE.DEFEND)
		{
			TurnToCurrentDirection();
			return;
		}

		//прыжок
		if (JumpNextFixedUpdate)
		{
			Jump();
			return;
		}

		//приземление
		if (jumpInProgress && IsGrounded())
        {
			HasLanded();
			return;
		}

        //восстановление после приземления, переход в IDLE
        if (playerState.currentState == UNITSTATE.LAND && Time.time - landTime > landRecoveryTime) playerState.SetState(UNITSTATE.IDLE);

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

		//если есть ригибоди и если инпут и перед игроком нет препятствия
		if (rb != null && (inputDirection.sqrMagnitude > 0 && !WallInFront()))
		{
			//выбор скорости передвижения
			float movementSpeed = playerState.currentState == UNITSTATE.RUN? runSpeed : walkSpeed;

			rb.velocity = new Vector3(inputDirection.x * -movementSpeed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, inputDirection.y * -movementSpeed);
			if (animator) animator.SetAnimatorFloat("MovementSpeed", rb.velocity.magnitude);
		}
		else
		{
			//только граыитация, без движения
			rb.velocity = new Vector3(0, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0);

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
		if (rb.velocity.y < 0.1f && playerState.currentState != UNITSTATE.KNOCKDOWN) animator.SetAnimatorBool("Falling", true);
	}

	//Прыжок
	void Jump()
	{
		playerState.SetState(UNITSTATE.JUMPING);
		JumpNextFixedUpdate = false;
		jumpInProgress = true;
		rb.velocity = new Vector3(rb.velocity.x, JumpForce, rb.velocity.z);

		//анимация
		animator.SetAnimatorBool("JumpInProgress", true);
		animator.SetAnimatorBool("Run", false);
		animator.SetAnimatorTrigger("JumpUp");
		animator.ShowDustEffectJump();

		//звук
		if (jumpUpVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(jumpUpVoice, transform.position);
	}

	//Приземление
	void HasLanded()
	{
		jumpInProgress = false;
		playerState.SetState(UNITSTATE.LAND);
		rb.velocity = Vector2.zero;
		landTime = Time.time;

		//настройки аниматора
		animator.SetAnimatorFloat("MovementSpeed", 0f);
		animator.SetAnimatorBool("JumpInProgress", false);
		animator.SetAnimatorBool("JumpKickActive", false);
		animator.SetAnimatorBool("Falling", false);
		animator.ShowDustEffectLand();

		//звук
		GlobalAudioPlayer.PlaySFX("FootStep");
		if (jumpLandVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(jumpLandVoice, transform.position);
	}

    #region controller input

    //Событие инпута направления
    void OnDirectionInputEvent(Vector2 dir, bool doubleTapActive)
	{
		//ignore input when we are dead or when this state is not active
		if (!MovementStates.Contains(playerState.currentState) || isDead) return;

		//set current direction based on the input vector
		inputDirection = dir.normalized;

		//start running on double tap
		if (doubleTapActive && IsGrounded()) playerState.SetState(UNITSTATE.RUN);
	}

	//Событие инпута кнопок
	void OnInputEvent(string action, BUTTONSTATE buttonState)
    {
		//ignore input when we are dead or when this state is not active
		if (!MovementStates.Contains(playerState.currentState) || isDead) return;

		//start a jump
		if (action == "Jump" && buttonState == BUTTONSTATE.PRESS && IsGrounded() && playerState.currentState != UNITSTATE.JUMPING) JumpNextFixedUpdate = true;

		//start running when a run button is pressed (e.g. Joypad controls)
		//if (action == "Run") playerState.SetState(UNITSTATE.RUN);
	}

	#endregion
		
	//interrups an ongoing jump
	public void CancelJump(){
		jumpInProgress = false;
	}
		
	//set current direction
	public void SetDirection(DIRECTION dir) {
		currentDirection = dir;
		if(animator) animator.currentDirection = currentDirection;
	}

	//returns the current direction
	public DIRECTION getCurrentDirection() {
		return currentDirection;
	}

	//returns true if the player is grounded
	public bool IsGrounded() {

		//check for capsule collisions with a 0.1 downwards offset from the capsule collider
		Vector3 bottomCapsulePos = transform.position + (Vector3.up) * (capsule.radius - 0.1f);
		return Physics.CheckCapsule(transform.position + capsule.center, bottomCapsulePos, capsule.radius, CollisionLayer);
	}
		
	//look (and turns) towards a direction
	public void TurnToCurrentDirection()
	{

		if (inputDirection != Vector2.zero)
		{
			Vector3 inputDirection3d = new Vector3(-inputDirection.y, 0f, inputDirection.x);

			float turnSpeed = jumpInProgress ? jumpRotationSpeed : rotationSpeed;
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


	/*void Kill() {
		isDead = true;
		rb.velocity = Vector3.zero;
	}*/

	//returns true if there is a environment collider in front of us
	bool WallInFront() {
		var MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
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
		Vector3 MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
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