using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Movement2D : MonoBehaviour
{
    // Player movement parameters
    [SerializeField] private float _movementAcceleration = 70f; // Acceleration when moving
    [SerializeField] private float _maxMoveSpeed = 12f; // Maximum movement speed
    [SerializeField] private float _GroundlinearDrag = 7f; // Deceleration when on the ground
    private float _horizontalDirection; // Horizontal input direction
    private float _verticalDirection; // Vertical input direction
    private bool _changingDirection => (_rb.velocity.x > 0f && _horizontalDirection < 0f) || (_rb.velocity.x < 0f && _horizontalDirection > 0f); // Detects if the player is changing direction

    // RigidBody component
    private Rigidbody2D _rb;

    // Jump parameters
    [SerializeField] private float _jumpForce = 12f; // Jump force
    [SerializeField] private float _airLinearDrag = 2.5f; // Deceleration in the air
    [SerializeField] private float _fallMultiplier = 8f; // Fall gravity multiplier
    [SerializeField] private float _lowJumpFallMultiplier = 5f; // Low jump gravity multiplier
    private bool _canJump => jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || _doubleJumpCount > 0 || _onWall); // Can the player jump?
    [SerializeField] private float _jumpvelocityFalloff = 12; // Jump velocity falloff

    // Double jump
    [SerializeField] private int _doubleJump = 1;
    private int _doubleJumpCount;

    // Coyote time (hang time)
    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    // Jump buffer
    [SerializeField] private float jumpBuffer = 0.2f;
    private float jumpBufferCounter;
    private bool _isJumping = false;

    // Layer masks
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private LayerMask _trap;

    // Grounded or not
    [SerializeField] private float _groundRayCastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    public bool _onGround;

    // Wall animation
    [SerializeField] private float _wallRayCastLength;
    [SerializeField] private float _wallSlideSpeed = 0.4f;
    [SerializeField] private float _wallRunSpeed = 0.7f;
    public bool _onWall;
    public bool _onRightWall;
    private bool _wallGrab => _onWall && !_onGround && Input.GetButton("grabTheWall") && !_wallRun;
    private bool _notWallGrab => !_wallGrab;
    private bool _wallSlide => _onWall && !_onGround && !Input.GetButton("grabTheWall") && _rb.velocity.y < 0f && !_wallRun;
    private bool _wallRun => _onWall && _verticalDirection > 0f;

    // Dash variables
    [Header("Dash Variables")]
    [SerializeField] private float _dashSpeed = 15f; // Dash speed
    [SerializeField] private float _dashLength = 0.3f; // Dash duration
    [SerializeField] private float _dashBufferLength = 1f; // Dash input buffer length
    private float _dashBufferCounter;
    private bool _isDashing;
    private bool _hasDashed;
    private bool _canDash => _dashBufferCounter > 0f && !_hasDashed && !_onGround;

    // Animator
    Animator animator;
    private bool _faceRight = true;

    AudioManager audioManager;

    public bool isOnPlatform;
    public Rigidbody2D platformRb;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize rigid body and Animator
        _rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Get horizontal and vertical input
        _horizontalDirection = GetInput().x;
        _verticalDirection = GetInput().y;

        // Manage jump buffer, player able to jump even though is not yet on the ground
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBuffer;
            //audioManager.PlaySFX(audioManager.jump);
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Update dash buffer based on player input
        if (Input.GetButtonDown("Dash"))
        {
            _dashBufferCounter = _dashBufferLength;
            // audioManager.PlaySFX(audioManager.dash);
        }
        else
        {
            _dashBufferCounter -= Time.deltaTime;
        }

        // Check for wall jumping
        if (_onWall && Input.GetButtonDown("Jump"))
        {
            // Wall jump in the opposite direction of the wall
            WallJump();
            flipX();
        }

        // Update animations based on various conditions
        Animation();
    }

    
    private void FixedUpdate()
    {
        
        // Check for collisions with the ground and walls
        CheckCollision();

        // Check if the player is on a platform
        if (isOnPlatform)
        {
            CharacterMovement();
        }

        if(_canDash)
        {
            StartCoroutine(Dash(_horizontalDirection, _verticalDirection));
            audioManager.PlaySFX(audioManager.dash);
        }

        if(!_isDashing)
        {
            //Make sure player only can move when not grabbing the walls
            if(_notWallGrab)
            {
                // If not wall grabbing, allow character movement
                CharacterMovement();
            }else{
                // If wall grabbing, apply horizontal movement while grabbing
                // This slows down horizontal movement when grabbing a wall 
                _rb.velocity = Vector2.Lerp(_rb.velocity, (new Vector2(_horizontalDirection*_maxMoveSpeed,_rb.velocity.y)), .5f *Time.deltaTime);
            };

            // Apply ground linear drag to gradually slow down horizontal movement
            ApplyGroundLinearDrag();

            
            Debug.Log("_onGround: " + _onGround);

            // Apply fall multiplier
            FallMultiplier();

            if (_onGround)
            {
                ApplyGroundLinearDrag();
                _doubleJumpCount = _doubleJump;
                coyoteTimeCounter = coyoteTime;
                _hasDashed = false;
            }
            else{
                ApplyAirLinearDrag();
                FallMultiplier();
                coyoteTimeCounter -= Time.fixedDeltaTime;
                if(!_onWall || _rb.velocity.y>0f ||_wallRun) _isJumping =false;
            }

            if (_canJump)
            {
                if(_onWall && !_onGround)
                {
                    WallJump();
                    flipX();
                }
                else{
                    Jump(Vector2.up);
                    audioManager.PlaySFX(audioManager.jump);
                }
            }
            if (!_isJumping)
            {
                if (_wallGrab) WallGrab();
                if (_wallSlide) WallSlide();
                if (_wallRun) WallRun();
                if (_onWall) PushToWall();
            }
        }

        
    }

    //get the user input
    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void CharacterMovement()
    {
        // Apply force for character movement
        _rb.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        // Limit maximum horizontal speed
        if (Mathf.Abs(_rb.velocity.x) > _maxMoveSpeed)
        {
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpeed, _rb.velocity.y);
        }
    }

    // Apply deceleration when on the ground
    private void ApplyGroundLinearDrag()
    {
        if (Mathf.Abs(_horizontalDirection) < 0.4f || _changingDirection)
        {
            _rb.drag = _GroundlinearDrag;
        }
        else
        {
            _rb.drag = 0f;
        }
    }

    //Apply deceleration in the air
    private void ApplyAirLinearDrag()
    {
 
        _rb.drag = _airLinearDrag;

    }


    private void Jump(Vector2 direction)
    {
        // Apply air linear drag, reset vertical velocity, and add jump force
        ApplyAirLinearDrag();
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

        // Only allow double jump when not grounded or on a wall
        if (!_onGround && !_onWall) _doubleJumpCount--;

        // Apply gravity falloff for better jump control
        if (_rb.velocity.y < _jumpvelocityFalloff)
        {
            _rb.velocity += Vector2.up * Physics2D.gravity.y * _fallMultiplier * Time.deltaTime;
        }

        // Disable coyote time and jump buffer to prevent multiple jumps
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        _isJumping = true;
    }

    private void WallJump()
    {
        // Determine the jump direction based on the wall side
        Vector2 jumpDirection = _onRightWall ? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
    }

    private void ResetDoubleJump()
    {
        _doubleJumpCount = _doubleJump;
    }

    // grounds and walls collision checking
    private void CheckCollision()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRayCastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRayCastLength, _groundLayer);

        // Detect collision with walls
        _onWall = Physics2D.Raycast(transform.position, Vector2.left, _wallRayCastLength, _wallLayer) ||
                        Physics2D.Raycast(transform.position, Vector2.right, _wallRayCastLength, _wallLayer);

        // Detect collision with the right wall
        _onRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRayCastLength, _wallLayer);

        // Reset the double jump count when touching the ground or a wall
        if (_onGround || _onWall)
        {
            ResetDoubleJump();
            _hasDashed = false;
        }


    }

    //on wall animation
    private void WallGrab()
    {
        _rb.gravityScale= 0f;
        _rb.velocity = new Vector2 (_rb.velocity.x, 0f);
        
    }


    // Push the character towards the wall for better control
    private void PushToWall()
    {
        //push player towards the walls
        if(_onRightWall && _horizontalDirection >=0f)
        {
            _rb.velocity =new Vector2(1f, _rb.velocity.y);
        }
        else if(!_onRightWall && _horizontalDirection<= 0f)
        {
            _rb.velocity= new Vector2(-1f, _rb.velocity.y);
        }
    }

    // Handle wall sliding
    private void WallSlide()
    {
        _rb.velocity =  new Vector2(_rb.velocity.x , -_maxMoveSpeed *_wallSlideSpeed);
    }

    // Handle wall running
    private void WallRun()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, _verticalDirection* _maxMoveSpeed* _wallRunSpeed);
    }

    //visual purpose for checking grounded or on wall
    private void OnDrawGizmos()
    {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRayCastLength);
            Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRayCastLength);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position+ Vector3.right *_wallRayCastLength);
            Gizmos.DrawLine(transform.position, transform.position+ Vector3.left *_wallRayCastLength);
    }

    // Apply gravity multiplier for better falling control
    private void FallMultiplier()
    {
        if(_rb.velocity.y<0)
        {
            _rb.gravityScale= _fallMultiplier;
        }
        else if(_rb.velocity.y >0 && !Input.GetButton("Jump"))
        {
            _rb.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            _rb.gravityScale=1f;
        }
    }

    // Flip the character's direction
    private void flipX()
    {
        _faceRight = !_faceRight;
        transform.Rotate(0, 180f,0f);
    }

    // Coroutine for handling dashing
    IEnumerator Dash(float x, float y)
    {
        float dashStartTime = Time.time;
        _hasDashed = true;
        _isDashing = true;
        _isJumping = false;
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.drag = 0f;

        Vector2 dir;
        if (x != 0f || y != 0f) dir = new Vector2(x,y);
        else
        {
            if (_faceRight) dir = new Vector2(1f, 0f);
            else dir = new Vector2(-1f, 0f);
        }

        while (Time.time < dashStartTime + _dashLength)
        {
            _rb.velocity = dir.normalized * _dashSpeed;
            yield return null;
        }
        _isDashing = false;
    }

    // Handle character animations
    private void Animation()
    {
        if((_horizontalDirection<0f && _faceRight||_horizontalDirection>0f && !_faceRight)&& !_wallGrab && !_wallSlide)
        {
            flipX();
        }

        if(_onGround)
        {
            animator.SetBool("grounded", true);
            animator.SetBool("falling", false);
            animator.SetBool("wallGrabbing", false);
            animator.SetFloat("moveHorizontal", Mathf.Abs(_horizontalDirection));
        }
        else
        {
            animator.SetBool("grounded", false);
        }

        if(_isJumping)
        {
            animator.SetBool("jumping", true);
            animator.SetBool("falling", false);
            animator.SetBool("wallGrabbing", false);
            animator.SetFloat("movingVertically", 0f);
        }
        else
        {
            animator.SetBool("jumping", false);

            if(_wallGrab || _wallSlide)
            {
                animator.SetBool("wallGrabbing", true);
                animator.SetBool("falling", false);
                animator.SetFloat("movingVertically", 0f);
            }
            else if(_rb.velocity.y <0f)
            {
                animator.SetBool("falling", true);
                animator.SetBool("wallGrabbing", false);
                animator.SetFloat("movingVertically", 0f);
            }

            if(_canJump)
            {
                animator.SetBool("jumping", true);
                animator.SetBool("falling", false);
                animator.SetBool("wallGrabbing", false);
                animator.SetFloat("movingVertically", 0f);
            }
            else
            {
                animator.SetBool("jumping", false);
                if(_wallGrab || _wallSlide)
                {
                    animator.SetBool("wallGrabbing", true);
                    animator.SetBool("falling", false); 
                    animator.SetFloat("movingVertically", 0f);
                }
                else if(_rb.velocity.y <0f)
                {
                    animator.SetBool("falling", true);
                    animator.SetBool("wallGrabbing", false);
                    animator.SetFloat("movingVertically", 0f);
                }
                if(_wallRun)
                {
                    animator.SetBool("falling", false);
                    animator.SetBool("wallGrabbing", false);
                    animator.SetFloat("movingVertically", Mathf.Abs(_verticalDirection)); 
                }
            }        
        }
    }


}
