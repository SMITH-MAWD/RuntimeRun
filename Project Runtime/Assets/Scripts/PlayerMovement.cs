using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

	public PlayerData Data;	
    Animator animator;

	// When false, movement and jump input are ignored (e.g. when a console/question UI is open).
	public bool inputEnabled = true;
	private bool _isMoving = false;
	private bool _isGrounded = false;

	#region Variables
	//Components
	public Rigidbody2D RB { get; private set; }

	//Variables control the various actions the player can perform at any time.
	public bool IsMoving { 
		get { 
		return _isMoving; 
		} private set {
			_isMoving = value;
			animator.SetBool("isMoving", value);
		} 
	}
	public bool IsGrounded { 
		get {
			return _isGrounded;
		} private set {
			_isGrounded = value;
			animator.SetBool("isGrounded", value);
		}
	}
	public bool canMove {
		get {
			if (animator == null)
				return true;
			return animator.GetBool("canMove");
		}
		private set {
			if (animator == null)
				return;
			animator.SetBool("canMove", value);
		}
	}
	public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
	public bool IsWallJumping { get; private set; }
	public bool IsSliding { get; private set; }

	//Timers (also all fields, could be private and a method returning a bool could be used)
	public float LastOnGroundTime { get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }

	//Jump
	private bool _isJumpCut;
	private bool _isJumpFalling;

	//Wall Jump
	private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	// Live (this-frame) wall overlap flags. Used by slide so it can't trigger from a stale
	// wall-coyote timer (which can be falsely refreshed if the wall-check box overlaps the floor).
	private bool _isTouchingWallRight;
	private bool _isTouchingWallLeft;

	private Vector2 _moveInput;
	public float LastPressedJumpTime { get; private set; }

	//Set all of these up in the inspector
	[Header("Checks")]
	[SerializeField] private Transform _groundCheckPoint;
	//Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

	[Header("Grounded Raycast")]
	[SerializeField] private bool _useRaycastGroundCheck = true;
	[SerializeField, Min(0.01f)] private float _groundRayLength = 0.15f;
	[Tooltip("Inwards inset from the collider edges for left/right rays.")]
	[SerializeField, Min(0f)] private float _groundRayInset = 0.05f;
	[SerializeField] private bool _drawGroundRays = true;

	[Header("Wall Raycast")]
	// Use horizontal raycasts for wall detection so the wall check can never spuriously hit the floor.
	[SerializeField] private bool _useRaycastWallCheck = true;
	[SerializeField, Min(0.01f)] private float _wallRayLength = 0.1f;
	[Tooltip("Vertical inset from the top/bottom of the collider for the wall rays (avoids scraping floor/ceiling).")]
	[SerializeField, Min(0f)] private float _wallRayVerticalInset = 0.08f;
	[SerializeField] private bool _drawWallRays = true;

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;

	// Reused buffer to avoid GC allocations during overlap checks
	private readonly Collider2D[] _overlapResults = new Collider2D[8];
	private readonly RaycastHit2D[] _raycastResults = new RaycastHit2D[8];

	private CapsuleCollider2D _mainCollider;

	// If the ground layer isn't configured, we fall back to "ground-like" accel so movement still works.
	// (Otherwise, the controller can become permanently "airborne", and accelInAir = 0 results in no movement.)
	private bool _warnedMissingGroundLayer;

	[Header("Attack")]
	[SerializeField, Min(0.05f)] private float _attackLockDuration	 = 0.5f;
	private float _attackLockEndTime;
	public bool IsAttacking => Time.time < _attackLockEndTime;
	
	#endregion

	private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
		_mainCollider = GetComponent<CapsuleCollider2D>();
		animator = GetComponentInChildren<Animator>();
		CreateCheckPointsIfMissing();
	}


	// Creates Ground Check and two Wall Check child GameObjects at the player's feet and sides
	// when their references are not set in the Inspector. Uses Collider2D bounds for positioning if present.

	private void CreateCheckPointsIfMissing()
	{
		// Use collider bounds for positioning if available; otherwise use default half-extents
		float halfWidth = 0.5f;
		float halfHeight = 0.5f;
		Collider2D col = GetComponent<Collider2D>();
		if (col != null)
		{
			Bounds b = col.bounds;
			halfWidth = b.extents.x;
			halfHeight = b.extents.y;
		}

		// Ground Check: child at player's feet (center-bottom)
		if (_groundCheckPoint == null)
		{
			GameObject ground = new GameObject("GroundCheck");
			ground.transform.SetParent(transform, worldPositionStays: false);
			ground.transform.localPosition = new Vector3(0f, -halfHeight, 0f);
			_groundCheckPoint = ground.transform;
		}

		// Front Wall Check: on the right when facing right (positive X)
		if (_frontWallCheckPoint == null)
		{
			GameObject frontWall = new GameObject("FrontWallCheck");
			frontWall.transform.SetParent(transform, worldPositionStays: false);
			frontWall.transform.localPosition = new Vector3(halfWidth, 0f, 0f);
			_frontWallCheckPoint = frontWall.transform;
		}

		// Back Wall Check: on the left when facing right (negative X)
		if (_backWallCheckPoint == null)
		{
			GameObject backWall = new GameObject("BackWallCheck");
			backWall.transform.SetParent(transform, worldPositionStays: false);
			backWall.transform.localPosition = new Vector3(-halfWidth, 0f, 0f);
			_backWallCheckPoint = backWall.transform;
		}
	}

	private bool AnyGroundOverlapBox(Transform checkPoint, Vector2 checkSize)
	{
		if (checkPoint == null)
			return false;

		int hitCount = Physics2D.OverlapBoxNonAlloc(checkPoint.position, checkSize, 0f, _overlapResults, _groundLayer);
		for (int i = 0; i < hitCount; i++)
		{
			Collider2D hit = _overlapResults[i];
			if (IsSelfCollider(hit))
				continue;
			return true;
		}
		return false;
	}


	// Returns true if the collider belongs to this Player (so we should ignore it for ground/wall detection).
	private bool IsSelfCollider(Collider2D col)
	{
		if (col == null)
			return true;

		// Same Rigidbody2D => same character
		if (col.attachedRigidbody != null && col.attachedRigidbody == RB)
			return true;

		// Same transform hierarchy => part of this character (includes check-colliders)
		Transform t = col.transform;
		if (t == transform || t.IsChildOf(transform))
			return true;

		return false;
	}


	//Raycast-based grounded check (3 downward rays from the bottom of the player's collider).
	private bool IsGroundedRaycast()
	{
		if (_mainCollider == null)
			_mainCollider = GetComponent<CapsuleCollider2D>();
		if (_mainCollider == null)
			return false;

		Bounds b = _mainCollider.bounds;

		// Cast from slightly above the feet so we don't start inside the ground collider.
		float originY = b.min.y + 0.02f;
		float halfWidth = b.extents.x;
		float inset = Mathf.Clamp(_groundRayInset, 0f, Mathf.Max(0f, halfWidth - 0.01f));

		Vector2 center = new Vector2(b.center.x, originY);
		Vector2 left = new Vector2(b.center.x - (halfWidth - inset), originY);
		Vector2 right = new Vector2(b.center.x + (halfWidth - inset), originY);

		return RaycastGroundFrom(center) || RaycastGroundFrom(left) || RaycastGroundFrom(right);
	}

	private bool RaycastGroundFrom(Vector2 origin)
	{
		ContactFilter2D filter = new ContactFilter2D
		{
			useTriggers = false
		};
		filter.SetLayerMask(_groundLayer);
		filter.useLayerMask = true;

		int hitCount = Physics2D.Raycast(origin, Vector2.down, filter, _raycastResults, _groundRayLength);
		for (int i = 0; i < hitCount; i++)
		{
			Collider2D hitCol = _raycastResults[i].collider;
			if (IsSelfCollider(hitCol))
				continue;
			return true;
		}
		return false;
	}

	// Horizontal-raycast wall check. Casts 3 short rays sideways from inside the collider at top/mid/lower-mid.
	// dir = +1 for right, -1 for left.
	private bool IsTouchingWallRaycast(int dir)
	{
		if (_mainCollider == null)
			_mainCollider = GetComponent<CapsuleCollider2D>();
		if (_mainCollider == null)
			return false;

		Bounds b = _mainCollider.bounds;
		float halfWidth = b.extents.x;
		float halfHeight = b.extents.y;
		// Start origins just inside the collider edge so we don't begin already touching outside geometry.
		float originX = b.center.x + dir * Mathf.Max(0f, halfWidth - 0.02f);
		float vInset = Mathf.Clamp(_wallRayVerticalInset, 0f, Mathf.Max(0f, halfHeight - 0.01f));

		Vector2 originTop = new Vector2(originX, b.max.y - vInset);
		Vector2 originMid = new Vector2(originX, b.center.y);
		Vector2 originBot = new Vector2(originX, b.min.y + vInset);
		Vector2 direction = new Vector2(dir, 0f);

		return RaycastWallFrom(originTop, direction)
			|| RaycastWallFrom(originMid, direction)
			|| RaycastWallFrom(originBot, direction);
	}

	private bool RaycastWallFrom(Vector2 origin, Vector2 direction)
	{
		ContactFilter2D filter = new ContactFilter2D
		{
			useTriggers = false
		};
		filter.SetLayerMask(_groundLayer);
		filter.useLayerMask = true;

		int hitCount = Physics2D.Raycast(origin, direction, filter, _raycastResults, _wallRayLength);
		for (int i = 0; i < hitCount; i++)
		{
			Collider2D hitCol = _raycastResults[i].collider;
			if (IsSelfCollider(hitCol))
				continue;
			return true;
		}
		return false;
	}

	private void Start()
	{
		SetGravityScale(Data.gravityScale);
		IsFacingRight = true;
	}

	private void Update()
	{
		// Ensure check points exist (e.g. if created at runtime or references were cleared)
		if (_groundCheckPoint == null || _frontWallCheckPoint == null || _backWallCheckPoint == null)
			CreateCheckPointsIfMissing();
		if (Data == null)
			return;

		#region TIMERS
		LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		#endregion

		#region INPUT HANDLER
		if (inputEnabled && Input.GetKeyDown(KeyCode.J) && IsGrounded)
		{
			onAttackInput();
		}

		if (inputEnabled && canMove && !IsAttacking)
		{
			_moveInput.x = Input.GetAxisRaw("Horizontal");
			_moveInput.y = Input.GetAxisRaw("Vertical");

			if (_moveInput.x != 0)
				CheckDirectionToFace(_moveInput.x > 0);

			// Jump inputs (keyboard)
			if (Input.GetKeyDown(KeyCode.W))
			{
				OnJumpInput();
			}

			if (Input.GetKeyUp(KeyCode.W))
			{
				OnJumpUpInput();
			}
		}
		else
		{
			_moveInput = Vector2.zero;
		}
		#endregion

		#region COLLISION CHECKS
		// Reset live wall-touch flags every frame so they only stay true while actually overlapping.
		_isTouchingWallRight = false;
		_isTouchingWallLeft = false;


		bool grounded = _useRaycastGroundCheck
			? IsGroundedRaycast()
			: AnyGroundOverlapBox(_groundCheckPoint, _groundCheckSize);
		IsGrounded = grounded;

		if (!IsJumping)
		{
			if (grounded) //checks if set box overlaps with ground
			{
				LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
			}

			//Right Wall Check
			bool rightWall = _useRaycastWallCheck
				? IsTouchingWallRaycast(+1)
				: (IsFacingRight
					? AnyGroundOverlapBox(_frontWallCheckPoint, _wallCheckSize)
					: AnyGroundOverlapBox(_backWallCheckPoint, _wallCheckSize));

			if (rightWall && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;

			//Left Wall Check
			bool leftWall = _useRaycastWallCheck
				? IsTouchingWallRaycast(-1)
				: (!IsFacingRight
					? AnyGroundOverlapBox(_frontWallCheckPoint, _wallCheckSize)
					: AnyGroundOverlapBox(_backWallCheckPoint, _wallCheckSize));

			if (leftWall && !IsWallJumping)
				LastOnWallLeftTime = Data.coyoteTime;

			// Cache live overlap state for slide (do NOT use coyote timers, which can be stale/falsely refreshed).
			_isTouchingWallRight = rightWall;
			_isTouchingWallLeft = leftWall;

			//Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.linearVelocity.y < 0)
		{
			IsJumping = false;

			if (!IsWallJumping)
				_isJumpFalling = true;
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
		{
			_isJumpCut = false;

			if (!IsJumping)
				_isJumpFalling = false;
		}

		//Jump
		if (CanJump() && LastPressedJumpTime > 0)
		{
			IsJumping = true;
			IsWallJumping = false;
			_isJumpCut = false;
			_isJumpFalling = false;
			Jump();
		}
		//WALL JUMP
		else if (CanWallJump() && LastPressedJumpTime > 0)
		{
			IsWallJumping = true;
			IsJumping = false;
			_isJumpCut = false;
			_isJumpFalling = false;
			_wallJumpStartTime = Time.time;
			_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

			WallJump(_lastWallJumpDir);
		}
		#endregion

		#region SLIDE CHECKS
		// Sliding requires an ACTIVE wall overlap this frame in the pressed direction (not coyote-time).
		// This prevents gliding caused by the wall-check box spuriously overlapping the floor while grounded.
		if (CanSlide() && ((_isTouchingWallLeft && _moveInput.x < 0) || (_isTouchingWallRight && _moveInput.x > 0)))
			IsSliding = true;
		else
			IsSliding = false;
		#endregion

		#region GRAVITY
		//Higher gravity if we've released the jump input or are falling
		if (IsSliding)
		{
			// While sliding, disable gravity so Slide() can drive the y-velocity directly.
			SetGravityScale(0);
		}
		else if (RB.linearVelocity.y < 0 && _moveInput.y < 0)
		{
			//Much higher gravity if holding down
			SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
			//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFastFallSpeed));
		}
		else if (_isJumpCut)
		{
			//Higher gravity if jump button released
			SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
			RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
		}
		else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
		{
			SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
		}
		else if (RB.linearVelocity.y < 0)
		{
			//Higher gravity if falling
			SetGravityScale(Data.gravityScale * Data.fallGravityMult);
			//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
		}
		else
		{
			//Default gravity if standing on a platform or moving upwards
			SetGravityScale(Data.gravityScale);
		}
		IsMoving = Mathf.Abs(RB.linearVelocity.x) > 0.1f;	
		animator.SetFloat("yVelocity", RB.linearVelocity.y);
	}
#endregion
	private void FixedUpdate()
	{
		if (Data == null)
			return;
		if (IsWallJumping)
			Run(Data.wallJumpRunLerp);
		else
			Run(1);
		if (IsSliding)
			Slide();
	}

	#region INPUT CALLBACKS
	//Methods which whandle input detected in Update()
	public void OnJumpInput()
	{
		LastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut())
			_isJumpCut = true;
	}

	public void ForceJump()
	{
		if (Data == null || RB == null)
			return;

		// Prevent "stacking" extra upward velocity which would exceed the normal max jump height.
		if (RB.linearVelocity.y > 0f)
			RB.linearVelocity = new Vector2(RB.linearVelocity.x, 0f);

		IsJumping = true;
		IsWallJumping = false;
		_isJumpCut = false;
		_isJumpFalling = false;
		Jump();
	}

	public void onAttackInput()
	{
		animator.SetTrigger("attackTrigger");
		// Stamp/extend the movement lock window. Each press resets the timer, so the
		// player stays locked through chained hits without measuring animation length.
		_attackLockEndTime = Time.time + _attackLockDuration;
	}

	#endregion

	#region GENERAL METHODS
	public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}
	#endregion

	//MOVEMENT METHODS
	#region RUN METHODS
	private void Run(float lerpAmount)
	{
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * Data.runMaxSpeed;
		//We can reduce are control using Lerp() this smooths changes to are direction and speed
		targetSpeed = Mathf.Lerp(RB.linearVelocity.x, targetSpeed, lerpAmount);

		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		bool hasGroundLayerConfigured = _groundLayer.value != 0;
		bool treatAsGroundedForRun = LastOnGroundTime > 0 || !hasGroundLayerConfigured;
		if (treatAsGroundedForRun)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;

		if (!hasGroundLayerConfigured && !_warnedMissingGroundLayer)
		{
			_warnedMissingGroundLayer = true;
			Debug.LogWarning("PlayerMovement: _groundLayer is not set (Nothing). Movement will use ground accel fallback; set _groundLayer to your Ground layer(s) for correct grounded checks.", this);
		}
		#endregion

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if (Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0;
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - RB.linearVelocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
	}

	private void Turn()
	{
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;

		IsFacingRight = !IsFacingRight;
	}
	#endregion

	#region OTHER MOVEMENT METHODS
	private void Slide()
	{
		// Drives y-velocity toward Data.slideSpeed using a force, similar to Run() but on the y-axis.
		float speedDif = Data.slideSpeed - RB.linearVelocity.y;
		float movement = speedDif * Data.slideAccel;
		// Clamp the per-frame force so we don't overshoot the target slide speed (force * fixedDeltaTime <= |speedDif|).
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		RB.AddForce(movement * Vector2.up);
	}
	#endregion

	#region JUMP METHODS
	private void Jump()
	{
		//Ensures we can't call Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		#region Perform Jump
		//We increase the force applied if we are falling
		//This means we'll always feel like we jump the same amount 
		//(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
		float force = Data.jumpForce;
		if (RB.linearVelocity.y < 0)
			force -= RB.linearVelocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion

		animator.SetTrigger("jumpTrigger");
	}

	private void WallJump(int dir)
	{
		//Ensures we can't call Wall Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(RB.linearVelocity.x) != Mathf.Sign(force.x))
			force.x -= RB.linearVelocity.x;

		if (RB.linearVelocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			force.y -= RB.linearVelocity.y;

		//Unlike in the run we want to use the Impulse mode.
		//The default mode will apply are force instantly ignoring masss
		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	#region CHECK METHODS
	public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}

	private bool CanJump()
	{
		return LastOnGroundTime > 0 && !IsJumping;
	}

	private bool CanWallJump()
	{
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
	{
		return IsJumping && RB.linearVelocity.y > 0;
	}

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.linearVelocity.y > 0;
	}

	// Player can slide when touching a wall, not jumping/wall-jumping, and not on the ground.
	public bool CanSlide()
	{
		if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && LastOnGroundTime <= 0)
			return true;
		else
			return false;
	}
	#endregion


	#region EDITOR METHODS
	private void OnDrawGizmosSelected()
	{
		// Prefer collider bounds when available; otherwise fall back to OverlapBox gizmos
		Gizmos.color = Color.green;
		if (_drawGroundRays && _useRaycastGroundCheck)
		{
			Collider2D col = _mainCollider != null ? _mainCollider : GetComponent<Collider2D>();
			if (col != null)
			{
				Bounds b = col.bounds;
				float originY = b.min.y + 0.02f;
				float halfWidth = b.extents.x;
				float inset = Mathf.Clamp(_groundRayInset, 0f, Mathf.Max(0f, halfWidth - 0.01f));

				Vector3 center = new Vector3(b.center.x, originY, 0f);
				Vector3 left = new Vector3(b.center.x - (halfWidth - inset), originY, 0f);
				Vector3 right = new Vector3(b.center.x + (halfWidth - inset), originY, 0f);

				Gizmos.DrawLine(center, center + Vector3.down * _groundRayLength);
				Gizmos.DrawLine(left, left + Vector3.down * _groundRayLength);
				Gizmos.DrawLine(right, right + Vector3.down * _groundRayLength);
			}
		}
		else if (_groundCheckPoint != null)
			Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);

		Gizmos.color = Color.blue;
		if (_drawWallRays && _useRaycastWallCheck)
		{
			Collider2D col = _mainCollider != null ? _mainCollider : GetComponent<Collider2D>();
			if (col != null)
			{
				Bounds b = col.bounds;
				float halfWidth = b.extents.x;
				float halfHeight = b.extents.y;
				float vInset = Mathf.Clamp(_wallRayVerticalInset, 0f, Mathf.Max(0f, halfHeight - 0.01f));

				for (int dir = -1; dir <= 1; dir += 2)
				{
					float originX = b.center.x + dir * Mathf.Max(0f, halfWidth - 0.02f);
					Vector3 top = new Vector3(originX, b.max.y - vInset, 0f);
					Vector3 mid = new Vector3(originX, b.center.y, 0f);
					Vector3 bot = new Vector3(originX, b.min.y + vInset, 0f);
					Vector3 step = new Vector3(dir * _wallRayLength, 0f, 0f);

					Gizmos.DrawLine(top, top + step);
					Gizmos.DrawLine(mid, mid + step);
					Gizmos.DrawLine(bot, bot + step);
				}
			}
		}
		else
		{
			if (_frontWallCheckPoint != null)
				Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
			if (_backWallCheckPoint != null)
				Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
		}
	}
	#endregion

	// Called when the player should die (e.g., touched spikes)
	public void Die()
	{
		// Try to respawn at the most recent console
		console1 recentConsole = console1.GetMostRecentConsole();
		if (recentConsole != null)
		{
			// Respawn at the console position
			RespawnAtPosition(recentConsole.transform.position);
		}
		else
		{
			// Fallback: Reload the scene if no console has been used
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}

	// Respawn the player at a specific position and reset their state
	private void RespawnAtPosition(Vector3 respawnPos)
	{
		// Move player to respawn position
		transform.position = respawnPos;

		// Reset physics state
		if (RB != null)
		{
			RB.linearVelocity = Vector2.zero;
			RB.angularVelocity = 0f;
		}

		// Reset jump and movement states
		IsJumping = false;
		IsWallJumping = false;
		IsSliding = false;
		_isJumpCut = false;
		_isJumpFalling = false;
		_moveInput = Vector2.zero;
		inputEnabled = true;

		Debug.Log("Player respawned at console position: " + respawnPos);
	}

}
