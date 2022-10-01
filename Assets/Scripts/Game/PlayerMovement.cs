using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using System.Linq;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviourPun
{
    [Tooltip("Character Controller main motor, that controles the character position behaviour")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Vector3 playerVelocity;

    [SerializeField] private float playerSpeed = 10.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private int trajectoryRotSpeed = 1;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private int numPoints = 50;
    [SerializeField] private float timeBetweenPoints = 0.1f;

    [SerializeField] private Level_SceneManager sceneManager;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] public CinemachineVirtualCamera winCam;
    public LayerMask TrajectoryLayerMask;

    [SerializeField] private float _rotationVelocity;
    [SerializeField] private Animator anim;

    public List<MagicStaff> magicStaffList;
    public int currentWeaponIndex;
    public Transform firePos;
    public MagicStaff GetStaff;
    public Projectile GetProjectile;
    public float GetDamage;
    public float airFlow;
    public int health = 100;
    public bool CanMove;
    public bool isDead;
    public bool isExecuted;

    [Header("Lose Repalcement Setup")]
    public SkinnedMeshRenderer mesh;
    public Material onFireDissolvel;
    public GameObject onFireFX;

    // Start is called before the first frame update
    void Start()
    {
        sceneManager = GameObject.FindObjectOfType<Level_SceneManager>();

        /// Problem On Assigning Players to The Scene Manager !! =============================================>>>>>>>>>>
        // Still trying to solve it...

        controller = gameObject.GetComponent<CharacterController>();
        lineRenderer = GetComponent<LineRenderer>();
        anim = GetComponent<Animator>();
        CanMove = true;

        // check we are on local player setting OnStart
        if (!photonView.IsMine)
            return;

        // Assign cameras
        vcam = GameObject.FindObjectOfType<CinemachineVirtualCamera>();

        if (photonView.IsMine)
        {
            // Setting up send rates:
            PhotonNetwork.SendRate = 16;
            PhotonNetwork.SerializationRate = 16;

            // Sync current stick
            photonView.RPC(nameof(SelectWeapon), RpcTarget.AllBuffered, currentWeaponIndex);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckPlayerDeath();

        if (!CanMove)
        {
            lineRenderer.enabled = false;
            return;
        }


        WeaponSwitching();
        MovePlayer();
        UseSpell();
        DrawTrajectory();
    }

    /// Step-1
    [PunRPC]
    public void TakeDamage()
    {
        if (health < 0)
        {
            if (gameObject.name.Contains("Red"))
            {
                sceneManager.EndGame(false, true);
            }
            if (gameObject.name.Contains("Blue"))
            {
                sceneManager.EndGame(true, false);
            }
            health = 0;
            return;
        }

        // Damage Value is 10
        health -= 10;
        sceneManager.Update_Objectives_Score();
    }

    /// Step-2
    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Attack" || col.tag == "Stun")
        {
            Debug.Log($"Player Got Hit by {col.tag} ==> {col.name}");
            photonView.RPC(nameof(TakeDamage), RpcTarget.AllBuffered);
        }

    }

    /// Step-3
    //IEnumerator Stunned()
    //{
    //    Debug.Log($"{gameObject.name} Got Stunned for 2 Secs !!");
    //    CanMove = false;
    //    yield return new WaitForSeconds(2f);
    //    CanMove = true;
    //}

    ///---------------------------------------- FINISHED !! -----------------------------------------------------
    ///----------------------------------------------------------------------------------------------------------

    void WeaponSwitching()
    {
        if (!photonView.IsMine)
            return;

        float scrollValue = Input.mouseScrollDelta.y;
        if (scrollValue > 0)
        {
            currentWeaponIndex++;
            if (currentWeaponIndex == magicStaffList.Count) // Limit by the Maximum value
            {
                currentWeaponIndex = 0;
            }
            photonView.RPC(nameof(SelectWeapon), RpcTarget.All, currentWeaponIndex);
        }
        else if (scrollValue < 0)
        {
            currentWeaponIndex--;
            if (currentWeaponIndex == -1)
            {
                currentWeaponIndex = magicStaffList.Count - 1; // reset to Maximum value
            }
            photonView.RPC(nameof(SelectWeapon), RpcTarget.All, currentWeaponIndex);
        }
    }

    [PunRPC]
    public void SelectWeapon(int newWeapon)
    {
        foreach (var item in magicStaffList)
        {
            item.gameObject.SetActive(false);
        }

        magicStaffList[newWeapon].gameObject.SetActive(true);

        // Storing currrent info
        GetStaff = magicStaffList[newWeapon];
        GetProjectile = GetStaff.projectileList[newWeapon];

        currentWeaponIndex = newWeapon;
    }

    void MovePlayer()
    {
        // Checking if we use our character local connection
        if (!photonView.IsMine)
            return;

        // Simulating Gravity
        playerVelocity.y = gravityValue;
        controller.Move(playerVelocity * Time.deltaTime);

        // Moving the Character
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Cancelling Cam x Rotation on the Vector.forward Vector
        Vector3 move = vcam.transform.right * x + new Vector3(vcam.transform.forward.x, 0, vcam.transform.forward.z) * z;

        // Normalize move if has a magnitude > 1 to prevent faster diagonal movement
        if (move.sqrMagnitude > 1)
            move.Normalize();

        // Character Controller movement
        controller.Move(move * playerSpeed * Time.deltaTime);

        //Get the Screen positions of the object
        photonView.RPC(nameof(CursorFollow), RpcTarget.AllBuffered, Camera.main.ScreenToViewportPoint(Input.mousePosition), Camera.main.WorldToViewportPoint(transform.position));


        // Figuring the Absolute value of the X , Z
        var momentum = Mathf.Abs(x) + Mathf.Abs(z);
        anim.SetFloat("speed", momentum);
    }

    [PunRPC]
    void CursorFollow(Vector3 worldPosition, Vector3 playerPosition)
    {
        // Best practice : We could've left that done localy, and the Rotations will be sunced, however this is best practice to send the data from each client to the other clients (Based on local info of the Info owner such as => Mouse Position & Player's position )
        float angle = AngleBetweenTwoPoints((Vector2)playerPosition, (Vector2)worldPosition);
        transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, b.x - a.x) * Mathf.Rad2Deg;
    }

    void UseSpell()
    {
        if (!photonView.IsMine)
            return;

        // Acrivate special Projectile according to the Current Magical Stick being used
        if (Input.GetMouseButtonDown(0))
        {
            photonView.RPC("Rpc_UseSpell", RpcTarget.All);
        }
    }

    [PunRPC]
    void Rpc_UseSpell()
    {
        //magicStaffList[currentWeaponIndex].LowAttack(firePos.transform);
        // TryCatch here

        try
        {
            // Index 0 => DarkPower, Usage => Self-Shield
            if (currentWeaponIndex == 0)
            {
                // Direct accessing of the VFX inside the Projectile for this spell only [ Barrier ]
                var _projectile = Instantiate(GetProjectile.VFXspell, transform.position, firePos.rotation).GetComponent<Rigidbody>();
                _projectile.transform.parent = firePos.transform;
            }

            // Index 1 => IcePower, Usage => Enemy-Stun Spell
            if (currentWeaponIndex == 1)
            {
                var _projectile = Instantiate(GetProjectile, firePos.position, firePos.rotation).GetComponent<Rigidbody>();
                _projectile.velocity = firePos.up * airFlow;
            }

            // Index 2 => LightPower, Usage => Attack Spell
            if (currentWeaponIndex == 2)
            {
                var _projectile = Instantiate(GetProjectile, firePos.position, firePos.rotation).GetComponent<Rigidbody>();
                _projectile.velocity = firePos.up * airFlow;
            }
        }
        catch
        {
            Debug.Log("RPC Spell Usage Detected !");
        }

    }

    void DrawTrajectory()
    {
        if (!photonView.IsMine)
            return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = (int)numPoints;
        List<Vector3> points = new List<Vector3>();
        Vector3 startingPosition = firePos.position;
        Vector3 startingVelocity = firePos.up * airFlow;
        for (float t = 0; t < numPoints; t += timeBetweenPoints)
        {
            Vector3 newPoint = startingPosition + t * startingVelocity;
            newPoint.y = startingPosition.y + startingVelocity.y * t + Physics.gravity.y / 2f * t * t;
            points.Add(newPoint);

            if (Physics.OverlapSphere(newPoint, 2, TrajectoryLayerMask).Length > 0)
            {
                lineRenderer.positionCount = points.Count;
                break;
            }
        }

        lineRenderer.SetPositions(points.ToArray());
    }

    void CheckPlayerDeath()
    {
        // If we are still alive, then dont execute the following.
        // Reloads for Everyone
        if (!isDead)
            return;

        // Check if we still waiting the character to be executed.
        if (!isExecuted)
        {
            isExecuted = true;
            Execution_SitOnFire_DragonBoss();
        }
    }

    // Executed by the draon fire breath
    public void Execution_SitOnFire_DragonBoss()
    {
        onFireFX.SetActive(true);
        StartCoroutine(DissolveMesh());
    }

    IEnumerator DissolveMesh()
    {
        yield return new WaitForSeconds(1f);
        mesh.material = onFireDissolvel;
        mesh.GetComponent<SpawnEffect>().enabled = true;
    }

}

