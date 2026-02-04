using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

	public PlayerData Data;

	// When false, movement and jump input are ignored (e.g. when a console/question UI is open).
	public bool inputEnabled = true;

	#region Variables
	//Components
    public Rigidbody2D RB { get; private set; }

	//Variables control the various actions the player can perform at any time.
	//These are fields which can are public allowing for other sctipts to read them
	//but can only be privately written to.
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

	[Header("Collider Checks")]
	[Tooltip("Optional. If assigned, grounded/wall checks use these trigger colliders instead of OverlapBox.")]
	[SerializeField] private Collider2D _groundCheckCollider;
	[SerializeField] private Collider2D _frontWallCheckCollider;
	[SerializeField] private Collider2D _backWallCheckCollider;

    [Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;

	// Reused buffer to avoid GC allocations during overlap checks
	private readonly Collider2D[] _overlapResults = new Collider2D[8];
	private readonly RaycastHit2D[] _raycastResults = new RaycastHit2D[8];

	private Collider2D _mainCollider;

	// If the ground layer isn't configured, we fall back to "ground-like" accel so movement still works.
	// (Otherwise, the controller can become permanently "airborne", and accelInAir = 0 results in no movement.)
	private bool _warnedMissingGroundLayer;
	#endregion

    private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
		_mainCollider = GetComponent<Collider2D>();
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

			// Create a trigger collider for collider-based ground checks
			if (_groundCheckCollider == null)
			{
				BoxCollider2D bc = ground.AddComponent<BoxCollider2D>();
				bc.isTrigger = true;
				bc.size = _groundCheckSize;
				_groundCheckCollider = bc;
			}
		}
		else if (_groundCheckCollider == null)
		{
			_groundCheckCollider = _groundCheckPoint.GetComponent<Collider2D>();
		}

		// Front Wall Check: on the right when facing right (positive X)
		if (_frontWallCheckPoint == null)
		{
			GameObject frontWall = new GameObject("FrontWallCheck");
			frontWall.transform.SetParent(transform, worldPositionStays: false);
			frontWall.transform.localPosition = new Vector3(halfWidth, 0f, 0f);
			_frontWallCheckPoint = frontWall.transform;

			if (_frontWallCheckCollider == null)
			{
				BoxCollider2D bc = frontWall.AddComponent<BoxCollider2D>();
				bc.isTrigger = true;
				bc.size = _wallCheckSize;
				_frontWallCheckCollider = bc;
			}
		}
		else if (_frontWallCheckCollider == null)
		{
			_frontWallCheckCollider = _frontWallCheckPoint.GetComponent<Collider2D>();
		}

		// Back Wall Check: on the left when facing right (negative X)
		if (_backWallCheckPoint == null)
		{
			GameObject backWall = new GameObject("BackWallCheck");
			backWall.transform.SetParent(transform, worldPositionStays: false);
			backWall.transform.localPosition = new Vector3(-halfWidth, 0f, 0f);
			_backWallCheckPoint = backWall.transform;

			if (_backWallCheckCollider == null)
			{
				BoxCollider2D bc = backWall.AddComponent<BoxCollider2D>();
				bc.isTrigger = true;
				bc.size = _wallCheckSize;
				_backWallCheckCollider = bc;
			}
		}
		else if (_backWallCheckCollider == null)
		{
			_backWallCheckCollider = _backWallCheckPoint.GetComponent<Collider2D>();
		}
	}

	private bool AnyGroundOverlap(Collider2D checkCollider)
	{
		if (checkCollider == null)
			return false;

		// We generally want to detect solid ground/walls (non-triggers). Our check colliders are triggers, but Overlap() can still report non-trigger colliders overlapping them.
		ContactFilter2D filter = new ContactFilter2D
		{
			useTriggers = false
		};
		filter.SetLayerMask(_groundLayer);
		filter.useLayerMask = true;

		int hitCount = checkCollider.Overlap(filter, _overlapResults);
		for (int i = 0; i < hitCount; i++)
		{
			Collider2D hit = _overlapResults[i];
			if (IsSelfCollider(hit))
				continue;
			return true;
		}
		return false;
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
			_mainCollider = GetComponent<Collider2D>();
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

	private void Start()
	{
		// If not set in Inspector, try loading default from Resources (e.g. Resources/PlayerData.asset)
		if (Data == null)
		{
			Data = Resources.Load<PlayerData>("PlayerData");
			if (Data == null)
			{
				Debug.LogWarning("PlayerMovement: Assign a PlayerData asset in the Inspector, or place one at Resources/PlayerData.asset.", this);
				return;
			}
		}
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
		if (inputEnabled)
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
		if (!IsJumping)
		{
			//Ground Check
			bool grounded = _useRaycastGroundCheck
				? IsGroundedRaycast()
				: (AnyGroundOverlap(_groundCheckCollider) || AnyGroundOverlapBox(_groundCheckPoint, _groundCheckSize));

			if (grounded && !IsJumping) //checks if set box overlaps with ground
			{
				LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
            }		

			//Right Wall Check
			bool rightWall =
				(IsFacingRight
					? (AnyGroundOverlap(_frontWallCheckCollider) || AnyGroundOverlapBox(_frontWallCheckPoint, _wallCheckSize))
					: (AnyGroundOverlap(_backWallCheckCollider) || AnyGroundOverlapBox(_backWallCheckPoint, _wallCheckSize)));

			if (rightWall && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;

			//Right Wall Check
			bool leftWall =
				(!IsFacingRight
					? (AnyGroundOverlap(_frontWallCheckCollider) || AnyGroundOverlapBox(_frontWallCheckPoint, _wallCheckSize))
					: (AnyGroundOverlap(_backWallCheckCollider) || AnyGroundOverlapBox(_backWallCheckPoint, _wallCheckSize)));

			if (leftWall && !IsWallJumping)
				LastOnWallLeftTime = Data.coyoteTime;

			//Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.linearVelocity.y < 0)
		{
			IsJumping = false;

			if(!IsWallJumping)
				_isJumpFalling = true;
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
			_isJumpCut = false;

			if(!IsJumping)
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
		if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
			IsSliding = true;
		else
			IsSliding = false;
		#endregion

		#region GRAVITY
		//Higher gravity if we've released the jump input or are falling
		if (IsSliding)
		{
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
		#endregion
    }

    private void FixedUpdate()
	{
		if (Data == null)
			return;
		
        //Handle Run
		if (IsWallJumping)
			Run(Data.wallJumpRunLerp);
		else
			Run(1);

		//Handle Slide
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
		if(Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
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

	#region OTHER MOVEMENT METHODS
	private void Slide()
	{
		//Works the same as the Run but only in the y-axis
		//THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
		float speedDif = Data.slideSpeed - RB.linearVelocity.y;	
		float movement = speedDif * Data.slideAccel;
		//So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
		//The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		RB.AddForce(movement * Vector2.up);
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
		else if (_groundCheckCollider != null)
			Gizmos.DrawWireCube(_groundCheckCollider.bounds.center, _groundCheckCollider.bounds.size);
		else if (_groundCheckPoint != null)
			Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);

		Gizmos.color = Color.blue;
		if (_frontWallCheckCollider != null)
			Gizmos.DrawWireCube(_frontWallCheckCollider.bounds.center, _frontWallCheckCollider.bounds.size);
		else if (_frontWallCheckPoint != null)
			Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);

		if (_backWallCheckCollider != null)
			Gizmos.DrawWireCube(_backWallCheckCollider.bounds.center, _backWallCheckCollider.bounds.size);
		else if (_backWallCheckPoint != null)
			Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
	}
    #endregion
}

// created by Dawnosaur :D