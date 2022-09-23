
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("UI Components")]
    public RectTransform bar;
    public Image guage;

    [Header("Variables")]
    [SerializeField] float jumpHeight; //점프 높이
    [SerializeField] float timeToReachJumpApex; //점프 높이 도달 시간
    [SerializeField] float moveSpeed; // 이동 속도

    float Gravity => -2 * jumpHeight / (timeToReachJumpApex * timeToReachJumpApex);
    float JumpForce => 2 * jumpHeight / timeToReachJumpApex;
    Vector3 velocity;
    Vector3 oldVelocity;
    float maxHeightReached = Mathf.NegativeInfinity;
    float startHeight = Mathf.NegativeInfinity; //디버그용
    bool reachedApex = true;
    //float velocityXSmoothing;
    Vector2 input;
    bool isControlledJump = false;
    bool isGrounded = false;
    float jumpTimer = 0; //디버그용
    Controller2D controller;
    //
    bool isLookingLeft = false;

    enum JumpState { Idle, SettingDirection, SettingPower, Jumping };
    JumpState curJumpState = JumpState.Idle;

    int barRotateDir = 1;
    [SerializeField] float barRotateTime; //게이지 바 회전 시간 (최소각도~최대각도)
    [SerializeField] float barStopTime; //게이지 바 회전 시간 (최소각도~최대각도)
    float barStopTimer = 0;
    bool barStop = false;
    [SerializeField] int rotateMin; // 최소 점프 각도
    [SerializeField] int rotateMax; // 최대 점프 각도
    [SerializeField] float fullChargeTime; // 최대 게이지 도달 시간(초)
    float fullChargeTimer = 0;
    [SerializeField] float minChargeTime; // 이 시간(초) 아래는 이 시간만큼 모은것처럼 점프
    [SerializeField] float chargeTimeLimit; // 강제 점프 발동 시간(초)
    float maxChargeTimer = 0;
    [SerializeField] bool logarithm_jump; //각도 낮은 점프때 멀리 안가게 보정?

    [Header("Debug Jump Setting")]
    [SerializeField] float debugJumpDegree = 90; // 각도 0~90
    [SerializeField] float debugJumpChargeRatio = 1; // 충전량 0~1.0
    bool isDebugJump = false;
    [Header("Trace Jump")]
    [SerializeField] SpriteRenderer traceDotPrefab;
    [SerializeField] bool traceJump; //점프한 궤적 보여주기
    Color dotColor;
    //

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        //    isControlledJump = true;
        //else isControlledJump = false;

        if (isGrounded)
        {
            if (curJumpState == JumpState.Idle)
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && (curJumpState == JumpState.Idle || curJumpState == JumpState.SettingDirection))
            {
                isLookingLeft = false;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && (curJumpState == JumpState.Idle || curJumpState == JumpState.SettingDirection))
            {
                isLookingLeft = true;
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                switch (curJumpState)
                {
                    case JumpState.Idle:
                        curJumpState = JumpState.SettingDirection;
                        bar.gameObject.SetActive(true);
                        velocity.x = 0;
                        break;
                    case JumpState.SettingDirection:
                        curJumpState = JumpState.SettingPower;
                        guage.gameObject.SetActive(true);
                        break;
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                if (curJumpState == JumpState.SettingPower)
                {
                    StartJump();
                }
            }
        }

        if (curJumpState == JumpState.SettingDirection)
        {
            input = Vector2.zero;
            SetDirection();
        }
        else if (curJumpState == JumpState.SettingPower)
        {
            if (fullChargeTimer < fullChargeTime)
            {
                fullChargeTimer += Time.deltaTime;
            }
            else fullChargeTimer = fullChargeTime;

            guage.fillAmount = fullChargeTimer / fullChargeTime;

            maxChargeTimer += Time.deltaTime;
            if (maxChargeTimer >= chargeTimeLimit)
            {
                maxChargeTimer = 0;
                StartJump();
            }
        }

        //Debug Jump
        if (isGrounded && (curJumpState == JumpState.Idle) && !isDebugJump)
        {
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                isDebugJump = true;
                DebugJump();
            }
        }
        //Trace Jump
        if(traceJump&&(curJumpState == JumpState.Jumping))
        {
            SpriteRenderer temp = Instantiate(traceDotPrefab);
            temp.transform.position = transform.position;
            temp.color = dotColor;
        }
    }

    private void FixedUpdate()
    {
        if (isControlledJump && controller.collisions.below)
        {
            ControlledJump();
        }

        if (!isGrounded && !reachedApex)
        {
            jumpTimer += Time.deltaTime;
        }

        if (!reachedApex && maxHeightReached > transform.position.y)
        {
            //float delta = maxHeightReached - startHeight;
            //float error = jumpHeight - delta;
            //Debug.Log($"jump result: start:{startHeight:F4}, end:{maxHeightReached:F4}, delta:{delta:F4}, error:{error:F4}, time:{jumpTimer:F4}, gravity:{Gravity:F4}, jumpForce:{JumpForce:F4}");
            reachedApex = true;
        }
        maxHeightReached = Mathf.Max(transform.position.y, maxHeightReached);

        oldVelocity = velocity;
        velocity.y += Gravity * Time.deltaTime;
        Vector3 deltaPosition = 0.5f * Time.fixedDeltaTime * (oldVelocity + velocity);
        if (curJumpState == JumpState.Idle)
        {
            velocity.x = input.x * moveSpeed;
            if (input.x != 0)
                SoundManager.Play("walk", false);
            else SoundManager.Stop("walk");
        }
        //float targetVelocityX = input.x * moveSpeed;
        //velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        controller.Move(deltaPosition);

        isGrounded = controller.collisions.below;// (controller.collisions.above || controller.collisions.below);
        if (isGrounded)
        {
            velocity.y = 0;
            if (curJumpState == JumpState.Jumping)
            {
                curJumpState = JumpState.Idle;
                fullChargeTimer = 0;
                maxChargeTimer = 0;
            }
            isDebugJump = false;
            dotColor = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f), 0.75f);
        }
        else
        {
            if (controller.collisions.left || controller.collisions.right)
            {
                if ((!controller.collisions.onSlope))
                    velocity.x = -velocity.x / 2;
                SoundManager.PlayOneShot("crash",true);
            }
            if (controller.collisions.above)
            {
                velocity.x /= 3;
                velocity.y = 0;//-velocity.y/2;
                SoundManager.PlayOneShot("crash",true);
            }
        }
    }

    void StartJump()
    {
        curJumpState = JumpState.Jumping;

        Vector2 directionVector = new Vector2();
        if (isLookingLeft)
        {
            directionVector.Set(-Mathf.Cos(Mathf.Deg2Rad * bar.localRotation.eulerAngles.z), Mathf.Sin(Mathf.Deg2Rad * bar.localRotation.eulerAngles.z));
            directionVector.x = Mathf.Round(directionVector.x * 10) * 0.1f;
            directionVector.y = Mathf.Round(directionVector.y * 10) * 0.1f;
            //directionVector *= Mathf.Sqrt((360-bar.rotation.eulerAngles.z) / 90);//test
            if (logarithm_jump)
                directionVector *= Mathf.Log((360 - bar.rotation.eulerAngles.z), 90);//test
        }
        else
        {
            directionVector.Set(Mathf.Cos(Mathf.Deg2Rad * bar.localRotation.eulerAngles.z), Mathf.Sin(Mathf.Deg2Rad * bar.localRotation.eulerAngles.z));
            directionVector.x = Mathf.Round(directionVector.x * 10) * 0.1f;
            directionVector.y = Mathf.Round(directionVector.y * 10) * 0.1f;
            //directionVector *= Mathf.Sqrt(bar.rotation.eulerAngles.z / 90);//test
            if (logarithm_jump)
                directionVector *= Mathf.Log(bar.rotation.eulerAngles.z, 90);//test
        }

        if (controller.collisions.below)
            Jump(directionVector);

        guage.fillAmount = 0;
        guage.gameObject.SetActive(false);
        bar.gameObject.SetActive(false);
    }
    void Jump(Vector2 direction)
    {
        float jumpForce;
        float chargeRatio;

        if (fullChargeTimer < minChargeTime)
            chargeRatio = (minChargeTime / fullChargeTime);
        else
            chargeRatio = (fullChargeTimer / fullChargeTime);

        jumpForce = 2 * jumpHeight / timeToReachJumpApex;

        jumpForce *= Mathf.Sqrt(chargeRatio);

        velocity = direction * jumpForce;
        jumpTimer = 0;
        maxHeightReached = Mathf.NegativeInfinity;

        startHeight = transform.position.y;
        reachedApex = false;

        SoundManager.PlayOneShot("jump", true);
    }
    void ControlledJump()
    {
        jumpTimer = 0;
        maxHeightReached = Mathf.NegativeInfinity;
        velocity.y = JumpForce;
        startHeight = transform.position.y;
        reachedApex = false;
    }
    void DebugJump()
    {
        curJumpState = JumpState.Jumping;
        Vector2 directionVector = new Vector2();
        directionVector.Set(Mathf.Cos(Mathf.Deg2Rad * debugJumpDegree), Mathf.Sin(Mathf.Deg2Rad * debugJumpDegree));
        directionVector.x = Mathf.Round(directionVector.x * 10) * 0.1f;
        directionVector.y = Mathf.Round(directionVector.y * 10) * 0.1f;
        if (isLookingLeft)
            directionVector.x = -directionVector.x;
        //directionVector *= Mathf.Sqrt(debugJumpDegree / 90);//test
        //directionVector *= Mathf.Log(debugJumpDegree, 90);//test
        float jumpForce = 2 * jumpHeight / timeToReachJumpApex;
        jumpForce *= Mathf.Sqrt(debugJumpChargeRatio);
        velocity = directionVector * jumpForce;
        jumpTimer = 0;
        maxHeightReached = Mathf.NegativeInfinity;
        startHeight = transform.position.y;
        reachedApex = false;
        SoundManager.PlayOneShot("jump", true);
    }

    void SetDirection()
    {
        float moveAmount = (rotateMax - rotateMin) / barRotateTime;

        if (barStop)
        {
            barStopTimer += Time.deltaTime;
            if (barStopTimer >= barStopTime)
            {
                barStopTimer = 0;
                barStop = false;
            }
        }
        else if (isLookingLeft)
        {
            //360~270
            bar.rotation = Quaternion.Euler(0, 0, bar.rotation.eulerAngles.z - (moveAmount * barRotateDir * Time.deltaTime));

            if (bar.rotation.eulerAngles.z <= (360 - rotateMax) && (bar.rotation.eulerAngles.z > 180))
            {
                bar.rotation = Quaternion.Euler(0, 0, -rotateMax);
                barRotateDir = -barRotateDir;
                barStop = true;
            }
            if ((bar.rotation.eulerAngles.z >= 360 - rotateMin) || (bar.rotation.eulerAngles.z < 90))
            {
                bar.rotation = Quaternion.Euler(0, 0, -rotateMin);
                barRotateDir = -barRotateDir;
                barStop = true;
            }
        }
        else
        {
            //0~90
            bar.rotation = Quaternion.Euler(0, 0, bar.rotation.eulerAngles.z + (moveAmount * barRotateDir * Time.deltaTime));

            if ((bar.rotation.eulerAngles.z >= rotateMax) && (bar.rotation.eulerAngles.z < 180))
            {
                bar.rotation = Quaternion.Euler(0, 0, rotateMax);
                barRotateDir = -barRotateDir;
                barStop = true;
            }
            if ((bar.rotation.eulerAngles.z <= rotateMin) || (bar.rotation.eulerAngles.z > 270))
            {
                bar.rotation = Quaternion.Euler(0, 0, rotateMin);
                barRotateDir = -barRotateDir;
                barStop = true;
            }
        }
    }
}
