using UnityEngine;
using Mirror;

public class MonsterMovement : NetworkBehaviour
{
    [Header("Audio Player")]
    public SoundManager sound;

    [Header("Player config")]
    public float speed = 3f;
    public float mouseSensitivity = 100f;
    public float gravity = -9f;
    public float groundDistance = .2f;

    [Header("Player component")]
    public GameObject mainCamera;
    public Transform groundCheck;
    public Transform playerBody;
    public Transform playerHead;
    public Transform playerCam;
    public LayerMask groundMask;
    public Light camLight; 

    [Header("Animator")]
    public Animator animator;

    [Header("Meshs to disable")]
    public SkinnedMeshRenderer[] partsMesh;

    [Header("Menu inside game")]
    [SerializeField] private GameObject menuInsideGame;


    private CharacterController cc;
    private Vector3 velocity;

    private float speedRun;
    private float backupSpeed;
    private float xRotation = 0f;

    private void Start()
    {
        if (hasAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;

            backupSpeed = speed;
            speedRun = speed * 2;

            cc = this.GetComponent<CharacterController>();

            foreach (SkinnedMeshRenderer mesh in partsMesh)
            {
                mesh.materials = new Material[0];
            }
        }
        else
        {
            mainCamera.SetActive(false);
            camLight.enabled = false;
        }
    }
    private void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.instance.ChangeGameMenu(menuInsideGame);
            }

            string animationName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (animationName != "MonsterUsingSkill" && animationName != "MonsterAttack" && animationName != "MonsterDying")
            {
                if (!GameManager.instance.gameMenuIsOpened)
                    RotateCamera();
                MovePlayer();
            }
        }
    }

    private void MovePlayer()
    {
        Vector3 axis = StaticMethodsMovement.GetAxisMovement();

        if (Input.GetKey(KeyCode.LeftShift)) SetRun();
        else SetNoRun();

        animator.SetFloat("Speed", Mathf.Abs(axis.x) + Mathf.Abs(axis.z));

        CheckSoundAndPlay(Mathf.Abs(axis.x) + Mathf.Abs(axis.z));

        cc.Move(StaticMethodsMovement.GetVectorMovement(transform, axis) * speed * Time.deltaTime);

        if (StaticMethodsMovement.IsGrounded(groundCheck.position, groundDistance, groundMask) && velocity.y < 0)
        {
            velocity.y = -.02f;
        }
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
    private void RotateCamera()
    {
        playerCam.transform.position = new Vector3(playerHead.transform.position.x, playerHead.transform.position.y + .1f, playerHead.transform.position.z);

        Vector2 axis = StaticMethodsMovement.GetAxisRotation();

        float mouseX = axis.x * mouseSensitivity;
        float mouseY = axis.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -50f, 50f);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerHead.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
    private void SetRun()
    {
        if (!animator.GetBool("Run"))
        {
            animator.SetBool("Run", true);
            speed = speedRun;
        }
    }
    private void SetNoRun()
    {
        if (animator.GetBool("Run"))
        {
            animator.SetBool("Run", false);
            speed = backupSpeed;
        }
    }
    private void CheckSoundAndPlay(float speed)
    {
        if (speed > 0.1f && !animator.GetBool("Run"))
        {
            if (!sound.IsPlaying("Walk"))
            {
                CmdPlaySound("Walk");
            }
        }
        else
        {
            CmdStopSound("Walk");
        }

        if (speed > 0.1f && animator.GetBool("Run"))
        {
            if (!sound.IsPlaying("Run"))
            {
                CmdPlaySound("Run");
            }
        }
        else
        {
            CmdStopSound("Run");
        }
    }
    [Command]
    private void CmdPlaySound(string soundName)
    {
        RpcPlaySound(soundName);
    }
    [Command]
    private void CmdStopSound(string soundName)
    {
        RpcStopSound(soundName);
    }

    [ClientRpc]
    private void RpcPlaySound(string soundName)
    {
        sound.Play(soundName);
    }
    [ClientRpc]
    private void RpcStopSound(string soundName)
    {
        sound.Stop(soundName);
    }
}