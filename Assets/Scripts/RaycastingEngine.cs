using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Oskar

public class RaycastingEngine : MonoBehaviour
{

    //Ärligt talat så är det mycket i dethär scriptet som jag har skrivit genom att följa en tutorial på internet.
    //Trots det så är det mycket utav koden som är annorlunda eller modifierad från vad videon sade.
    //https://www.youtube.com/watch?v=iX_pV0XVQWY är tutorialen jag använde, min kod i denna video är onekligen ganska lik denna, ända ner till variabel namnen.

    private enum JumpState
    {
        None = 0, Holding,


    }
    private enum ClingState
    {

        None = 0, Holding, Sliding,

    }
    private enum DashState
    {
        None, Dashing,



    }

    public GameObject Outline;
    public SpriteRenderer[] outlineShades;

    CircleCollider2D hitBox;
    BoxCollider2D hurtBox;

    public killEnemies killEnemies;

    [SerializeField]
    private LayerMask platformMask;
    [SerializeField]
    private float parallelInsetLen;
    [SerializeField]
    private float perpendicularInsetLen;
    [SerializeField]
    private float groundCheckLen;
    [SerializeField]
    private float wallCheckLen;
    [SerializeField]
    public float gravity;
    [SerializeField]
    private float xSpeedUpAccel;
    [SerializeField]
    private float xSpeedDownAccel;
    [SerializeField]
    private float xMaxSpeed;
    [SerializeField]
    private float xSnapSpeed;
    [SerializeField]
    private float CoyoteTime;
    [SerializeField]
    private float jumpStartSpeed;
    [SerializeField]
    private float jumpMaxHoldPeriod;
    [SerializeField]
    private float jumpMinSpeed;
    [SerializeField]
    private float extraJumps;
    [SerializeField]
    private float wallSlideAccel;

    public static Vector2 velocity;

    private RaycastMoveDirection moveDown;
    private RaycastMoveDirection moveLeft;
    private RaycastMoveDirection moveRight;
    private RaycastMoveDirection moveUp;

    private RaycastCheckTouching groundDown;
    private RaycastCheckTouching wallRight;
    private RaycastCheckTouching wallLeft;

    private Vector2 Position;
    Vector2 boundsTopRight;
    Vector2 boundsBottomLeft;

    private float jumps;
    private float jumpStartTimer;
    private float jumpHoldTimer;
    private bool jumpInputDown;
    private JumpState jumpState;
    private ClingState clingState;
    private DashState dashState;
    private float climbDirection;

    [SerializeField]
    float dashTime;
    float dashTimeLeft;
    [SerializeField]
    float dashCooldown;
    float dashCooldownLeft;

    Vector3 dashPosition;

    Vector3 mouse;

    TrailRenderer trail;
    public GameObject particles;

    public Animator animator;
    //public SpriteRenderer playerSprite;
    private bool facingRight;
    private int moveInput;
    public static bool isPogoing = false;

    void Start()
    {

        killEnemies = GetComponent<killEnemies>();

        outlineShades = Outline.GetComponentsInChildren<SpriteRenderer>();

        trail = GetComponent<TrailRenderer>();

        boundsTopRight.y = /*transform.position.y + */GetComponent<Renderer>().bounds.extents.y;
        boundsTopRight.x = /*transform.position.x + */GetComponent<Renderer>().bounds.extents.x;

        boundsBottomLeft.y = /*transform.position.y + */GetComponent<Renderer>().bounds.extents.y * -1;
        boundsBottomLeft.x = /*transform.position.x + */GetComponent<Renderer>().bounds.extents.x * -1;

        hitBox = GetComponent<CircleCollider2D>();
        hurtBox = GetComponent<BoxCollider2D>();


        platformMask = LayerMask.GetMask("Ground");

        ////Downwards HitBox
        moveDown = new RaycastMoveDirection(boundsBottomLeft, new Vector2(boundsTopRight.x, boundsBottomLeft.y), Vector2.down, platformMask,
        Vector2.right * parallelInsetLen, Vector2.up * perpendicularInsetLen);

        //Leftwards HitBox
        moveLeft = new RaycastMoveDirection(boundsBottomLeft, new Vector2(boundsBottomLeft.x, boundsTopRight.y), Vector2.left, platformMask,
        Vector2.up * parallelInsetLen, Vector2.right * perpendicularInsetLen);

        //Upwards Hitbox
        moveUp = new RaycastMoveDirection(new Vector2(boundsBottomLeft.x, boundsTopRight.y), boundsTopRight, Vector2.up, platformMask,
        Vector2.right * parallelInsetLen, Vector2.down * perpendicularInsetLen);

        //Rightwards Hitbox
        moveRight = new RaycastMoveDirection(new Vector2(boundsTopRight.x, boundsBottomLeft.y), boundsTopRight, Vector2.right, platformMask,
        Vector2.up * parallelInsetLen, Vector2.left * perpendicularInsetLen);

        //GroundCheck;
        groundDown = new RaycastCheckTouching(boundsBottomLeft, new Vector2(boundsTopRight.x, boundsBottomLeft.y), Vector2.down, platformMask,
        Vector2.right * parallelInsetLen, Vector2.up * perpendicularInsetLen, groundCheckLen);

        //Climbcheck Right
        wallRight = new RaycastCheckTouching(new Vector2(0.7f, -2.3f), new Vector2(0.7f, 2.0f), Vector2.right, platformMask,
        Vector2.up * parallelInsetLen, Vector2.left * perpendicularInsetLen, wallCheckLen);

        //Climbcheck Left
        wallLeft = new RaycastCheckTouching(new Vector2(-0.7f, -2.3f), new Vector2(-0.7f, 2.0f), Vector2.left, platformMask,
        Vector2.up * parallelInsetLen, Vector2.right * perpendicularInsetLen, wallCheckLen);

    }

    //Enkel medot för att endast ta reda på om ett nummer är positivt, negativt eller 0
    private int GetSign(float v)
    {
        if (Mathf.Approximately(v, 0))
        {

            return 0;

        }
        else if (v > 0)
        {

            return 1;

        }
        else
        {

            return -1;

        }
    }


    private void Update()
    {


        //else
        //{
        trail.emitting = false;
        particles.SetActive(false);

        jumpStartTimer -= Time.deltaTime;

        bool jumpBtn = Input.GetButton("Jump");


        if (jumpBtn && jumpInputDown == false)
        {

            jumpStartTimer = CoyoteTime;


        }


        //Koden för wallclinging är helt min egen, här kollar den om det finns en vägg till höger eller vänster.

        if (wallRight.RaycastCheckClimbable(transform.position) == true)
        {

            climbDirection = 1;

        }
        else if (wallLeft.RaycastCheckClimbable(transform.position) == true)
        {

            climbDirection = -1;
        }
        else
        {

            climbDirection = 0;

        }


        jumpInputDown = jumpBtn;

        Position.x = transform.position.x;
        Position.y = transform.position.y;
        //}

        //private void FixedUpdate()
        ////{
        //animator.SetFloat("Speed", velocity.magnitude);
        //facingRight = playerSprite.flipX;


        switch (jumpState)
        {

            case JumpState.None:

                //JumpState none är alltså när man inte hoppar
                //den byter till "Holding" när spelaren klickar på den angivna knappen och har jumps kvar

                if (jumps > 0 && jumpStartTimer > 0)
                {
                    //jumps = jumps - 1;

                    jumpStartTimer = 0;
                    jumpState = JumpState.Holding;
                    jumpHoldTimer = 0;

                    //Då ökas även den verikala velocityn.
                    velocity.y = jumpStartSpeed;

                }
                break;

            case JumpState.Holding:

                //JumpState Holding stängs av om man släpper knappen eller efter att man har hållt inne för länge.
                jumpHoldTimer += Time.deltaTime;

                //animator.SetBool("IsJumping", true);

                if (jumpInputDown == false || jumpHoldTimer >= jumpMaxHoldPeriod)
                {

                    jumps = jumps - 1;
                    jumpState = JumpState.None;

                    velocity.y = Mathf.Lerp(jumpMinSpeed, jumpStartSpeed, jumpHoldTimer / jumpMaxHoldPeriod);


                }
                break;

        }

        if (groundDown.DoRaycast(transform.position) == true)
        {
            //animator.SetBool("IsJumping", false);
            jumps = extraJumps + 1;

        }
        else
        {

        }

        float xInput = Input.GetAxisRaw("Horizontal");

        int wantedDirection = GetSign(xInput);
        int velocityDirection = GetSign(velocity.x);

        moveInput = wantedDirection;

        if (facingRight == false && moveInput > 0) //this part is for flipping the player sprite depending on what direction its moving
        {
            Flip(); //call flip method
        }
        else if (facingRight == true && moveInput < 0)
        {
            Flip();
        }

        if (wantedDirection != 0)
        {
            //animator.SetFloat("Speed", 1);



            if (wantedDirection != velocityDirection)
            {
                velocity.x = xSnapSpeed * wantedDirection;

            }
            else
            {

                velocity.x = Mathf.MoveTowards(velocity.x, xMaxSpeed * wantedDirection, xSpeedUpAccel * Time.deltaTime);

            }

        }
        else
        {

            //animator.SetFloat("Speed", 0);
            velocity.x = Mathf.MoveTowards(velocity.x, 0, xSpeedDownAccel * Time.deltaTime);

        }




        switch (clingState)
        {
            case ClingState.None:


                if (climbDirection == wantedDirection && climbDirection != 0 && groundDown.DoRaycast(transform.position) != true)
                {

                    clingState = ClingState.Holding;
                    jumps = extraJumps + 1;

                }
                break;

            case ClingState.Holding:


                jumpState = JumpState.None;
                velocity.y = 0f;

                if (wantedDirection == 0 && climbDirection != 0)
                {


                    clingState = ClingState.Sliding;


                }
                else if (climbDirection == 1 && wantedDirection < 0 || climbDirection == -1 && wantedDirection > 0)
                {

                    clingState = ClingState.None;

                }
                break;

            case ClingState.Sliding:

                velocity.y -= wallSlideAccel * Time.deltaTime;

                if (velocity.y >= gravity || wantedDirection == climbDirection * -1)
                {

                    clingState = ClingState.None;

                }
                else if (climbDirection == wantedDirection && climbDirection != 0)
                {

                    clingState = ClingState.Holding;
                    jumps = extraJumps + 1;

                }
                break;

        }


        if (jumpState == JumpState.None && clingState == ClingState.None && isPogoing == false)
        {

            velocity.y -= gravity * Time.deltaTime;

        }




        Vector2 displacement = Vector2.zero;
        Vector2 wantedDisplacement = velocity * Time.deltaTime;



        if (velocity.x > 0)
        {

            displacement.x = moveRight.DoRaycast(transform.position, wantedDisplacement.x);
        }
        else if (velocity.x < 0)
        {

            displacement.x = -moveLeft.DoRaycast(transform.position, -wantedDisplacement.x);
        }

        if (velocity.y > 0)
        {

            displacement.y = moveUp.DoRaycast(transform.position, wantedDisplacement.y);

        }
        else if (velocity.y < 0)
        {

            displacement.y = -moveDown.DoRaycast(transform.position, -wantedDisplacement.y);

        }



        if (Mathf.Approximately(displacement.x, wantedDisplacement.x) == false)
        {
            velocity.x = 0;
        }
        if (Mathf.Approximately(displacement.y, wantedDisplacement.y) == false)
        {
            velocity.y = 0;
        }



        transform.Translate(displacement);


        //}

        switch (dashState)
        {
            case DashState.None:


                if (dashCooldownLeft >= 0)
                {
                    dashCooldownLeft -= Time.deltaTime;

                    //foreach (var item in outlineShades)
                    //{

                    //    item.color = new Color(0, 0, 0, 0);

                    //}

                }
                else
                {

                    foreach (var item in outlineShades)
                    {

                        item.color = new Color(0, 0, 0, 255);

                    }

                }

                if (Input.GetMouseButtonDown(0) && dashCooldownLeft < 0)
                {

                    hurtBox.enabled = false;
                    hitBox.enabled = true;

                    foreach (var item in outlineShades)
                    {

                        item.color = new Color(0, 0, 0, 0);

                    }

                    particles.SetActive(true);

                    if (facingRight && transform.position.x - dashPosition.x < 0)
                    {

                        Flip();

                    }
                    if (!facingRight && transform.position.x - dashPosition.x > 0)
                    {

                        Flip();

                    }

                    velocity = Vector3.zero;

                    dashCooldownLeft = dashCooldown;

                    dashPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    dashPosition.z = 0;

                    dashTimeLeft = dashTime;

                    dashState = DashState.Dashing;

                }



                break;

            case DashState.Dashing:




                if (dashTimeLeft >= 0)
                {

                    transform.position = Vector3.Lerp(transform.position, dashPosition, dashTimeLeft);
                    dashTimeLeft -= Time.deltaTime;

                    trail.emitting = true;



                }
                else
                {

                    hitBox.enabled = false;
                    hurtBox.enabled = true;

                    if(killEnemies.enemiesToMurder != null)
                    {

                        killEnemies.KillEnemies();

                    }

                    dashState = DashState.None;

                }

                break;


        }




        }

    private void Flip()
    {
        //interactRange *= -1;
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale; //create a temporary vector3 
        Scaler.x *= -1; //sets the scaler to the current value * -1
        transform.localScale = Scaler; //sets local scale to the scaler value

    }

    private void OnDrawGizmos()
    {



    }
}
