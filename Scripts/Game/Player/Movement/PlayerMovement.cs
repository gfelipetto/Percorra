using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
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

    [Header("Animator")]
    public Animator animator;

    [Header("Meshs to disable")]
    public SkinnedMeshRenderer[] partsMesh;

    private CharacterController cc;
    private Vector3 velocity;

    private float speedRun;
    private float speedCrounch;
    private float backupSpeed;
    private float xRotation = 0f;

    private void Start()
    {
        if (hasAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;

            backupSpeed = speed;
            speedRun = speed * 2;
            speedCrounch = speed / 2;

            cc = this.GetComponent<CharacterController>();


            foreach (SkinnedMeshRenderer mesh in partsMesh)
            {
                mesh.materials = new Material[0];
            }
        }

        else
        {
            mainCamera.SetActive(false);
        }
    }
    private void Update()
    {
        if (hasAuthority)
        {
            if (!PlayerHpManager.isDead || !GamePlayer.escaped)
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

        if (Input.GetKey(KeyCode.LeftControl)) SetCrouch();
        else SetNoCrouch();

        animator.SetFloat("Speed", Mathf.Abs(axis.x) + Mathf.Abs(axis.z));

        CheckSoundAndPlay(Mathf.Abs(axis.x) + Mathf.Abs(axis.z));

        cc.Move(StaticMethodsMovement.GetVectorMovement(this.transform, axis) * speed * Time.deltaTime);

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
    private void SetCrouch()
    {
        if (!animator.GetBool("Crouched"))
        {
            animator.SetBool("Crouched", true);
            if (!animator.GetBool("Run")) speed = speedCrounch;
        }
    }
    private void SetNoCrouch()
    {
        if (animator.GetBool("Crouched"))
        {
            animator.SetBool("Crouched", false);
            if (animator.GetBool("Run")) speed = speedRun;
            else speed = backupSpeed;
        }
    }
    private void SetRun()
    {
        if (!animator.GetBool("Run"))
        {
            animator.SetBool("Run", true);
            if (!animator.GetBool("Crouched")) speed = speedRun;
        }
    }
    private void SetNoRun()
    {
        if (animator.GetBool("Run"))
        {
            animator.SetBool("Run", false);
            if (animator.GetBool("Crouched")) speed = speedCrounch;
            else speed = backupSpeed;
        }
    }

    private void CheckSoundAndPlay(float speed)
    {
        if (speed > 0.1f && !animator.GetBool("Run") && !animator.GetBool("Crouched"))
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
