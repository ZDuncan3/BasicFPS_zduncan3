using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouse Controls")]
    public Vector2 mouseSensitivity;
    private Vector3 offset;

    private Vector2 rotation;

    [Header("Object References")]
    public GameLogic gameLogic;
    public GameObject player, fPCamera, mainCamera, tacCamera, fPUi;
    private Camera _mainCamera;
    private Rigidbody _camRb, _rb;
    private Enemy enemy;

    [Header("Tactical Camera Controls")]
    public float smoothSpeed = 2.5f, rotateSpeed = 90f;

    [Header("Player Variables")]
    public float walkSpeed = 3f, sprintSpeed = 6f, jumpPower = 6.0f;
    public int remainingJumps;
    public uint maxJumps = 1;
    public float playerHeight = 2;

    private bool enablePlayerMovementControls, enablePlayerCameraControls;

    [Header("Misc")]
    public LayerMask groundLayer;
    public LayerMask resetLayer;
    private bool isGrounded;
    public float resetPointY;
    public Vector3 resetPosition;
    private bool objectMoved = false;

    [Header("Toggles")]
    public bool tacticalCamera = false;

    private void Start()
    {
        _mainCamera = Camera.main;
        _camRb = tacCamera.GetComponent<Rigidbody>();
        _rb = GetComponent<Rigidbody>();
        player = this.gameObject;

        if (resetPosition.x == 0 && resetPosition.y == 0 && resetPosition.z == 0)
            resetPosition = player.transform.position;

        if (tacticalCamera)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else if (!tacticalCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (offset.x == 0)
            offset.x = 7;
        if (offset.y == 0)
            offset.y = 5;

        enablePlayerMovementControls = gameLogic.enablePlayerMovementControls;
        enablePlayerCameraControls = gameLogic.enablePlayerCameraControls;

        if (mouseSensitivity.x == 0 || mouseSensitivity.y == 0 && enablePlayerCameraControls)
        {
            if (mouseSensitivity.x == 0)
                mouseSensitivity.x = 400;
            if (mouseSensitivity.y == 0)
                mouseSensitivity.y = 400;
        }

        if (remainingJumps <= 0)
            remainingJumps = (int)maxJumps;
    }

    private void Update()
    {
        if (player.transform.position.y <= resetPointY)
        {
            player.transform.position = resetPosition;
            _rb.velocity = Vector3.down * 0;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tacticalCamera = !tacticalCamera;
            if (!tacticalCamera)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (tacCamera.activeInHierarchy)
                    tacCamera.SetActive(false);
                if (!fPCamera.activeInHierarchy)
                    fPCamera.SetActive(true);
                if (!mainCamera.activeInHierarchy)
                    mainCamera.SetActive(true);

                fPUi.SetActive(true);
            }
            else if (tacticalCamera)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;

                if (fPCamera.activeInHierarchy)
                    fPCamera.SetActive(false);
                if (mainCamera.activeInHierarchy)
                    mainCamera.SetActive(false);
                if (!tacCamera.activeInHierarchy)
                    tacCamera.SetActive(true);

                fPUi.SetActive(false);
            }
        }

        HandleLeftClick();

        if (tacticalCamera)
            mainCamera.transform.rotation = player.transform.rotation;
    }

    void FixedUpdate()
    {
        HandleCamera();
        HandleMovement();
    }

    void LateUpdate()
    {
        enablePlayerMovementControls = gameLogic.enablePlayerMovementControls;
        enablePlayerCameraControls = gameLogic.enablePlayerCameraControls;

        if (!tacticalCamera)
            tacCamera.transform.position = new Vector3(player.transform.position.x + 1, player.transform.position.y + 1, player.transform.position.z);
    }

    private void HandleCamera()
    {
        if (enablePlayerCameraControls)
        {
            if (!tacticalCamera)
            {
                float mouseX = Input.GetAxis("Mouse X") * Time.fixedDeltaTime * mouseSensitivity.x;
                float mouseY = Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * mouseSensitivity.y;

                rotation.y += mouseX;

                rotation.x -= mouseY;
                rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

                _mainCamera.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
                transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            }
            else if (tacticalCamera)
            {
                Vector3 desiredPosition = player.transform.position + offset;
                Vector3 smoothedPosition = Vector3.Lerp(tacCamera.transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);
                _camRb.MovePosition(smoothedPosition);
            }
        }
    }

    private void HandleMovement()
    {
        float rotateAmount = Input.GetAxis("Horizontal");
        float moveAmount = Input.GetAxis("Vertical");

        if (transform != null)
        {
            // forward movement handler
            if (Input.GetKey(KeyCode.W))
            {
                if (!tacticalCamera)
                {
                    // sprint handler
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        // sprinting forward and left
                        if (Input.GetKey(KeyCode.A))
                        {
                            _rb.position += transform.forward * (sprintSpeed) * 0.65f * Time.fixedDeltaTime;
                            _rb.position += -transform.right * (sprintSpeed) * 0.65f * Time.fixedDeltaTime;
                        }
                        // sprinting forward and right
                        else if (Input.GetKey(KeyCode.D))
                        {
                            _rb.position += transform.forward * (sprintSpeed) * 0.65f * Time.fixedDeltaTime;
                            _rb.position += transform.right * (sprintSpeed) * 0.65f * Time.fixedDeltaTime;
                        }
                        // sprinting forward
                        else
                        {
                            _rb.position += transform.forward * (sprintSpeed) * Time.fixedDeltaTime;
                        }
                    }
                    // walk handler
                    else
                    {
                        // walking forward and left
                        if (Input.GetKey(KeyCode.A))
                        {
                            _rb.position += transform.forward * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                            _rb.position += -transform.right * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                        }
                        // walking forward and right
                        else if (Input.GetKey(KeyCode.D))
                        {
                            _rb.position += transform.forward * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                            _rb.position += transform.right * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                        }
                        // walking forward
                        else
                        {
                            _rb.position += transform.forward * (walkSpeed) * Time.fixedDeltaTime;
                        }
                    }
                }
                else if (tacticalCamera)
                {
                    // sprint handler
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        // sprinting forward and turning left
                        if (Input.GetKey(KeyCode.A))
                        {
                            _rb.position += transform.forward * (sprintSpeed) * Time.fixedDeltaTime;

                            rotateAmount *= rotateSpeed * Time.deltaTime;

                            transform.Rotate(Vector3.up, rotateAmount);
                        }
                        // sprinting forward and turning right
                        else if (Input.GetKey(KeyCode.D))
                        {
                            _rb.position += transform.forward * (sprintSpeed) * Time.fixedDeltaTime;

                            rotateAmount *= rotateSpeed * Time.deltaTime;

                            transform.Rotate(Vector3.up, rotateAmount);
                        }
                        // sprinting forward
                        else
                        {
                            _rb.position += transform.forward * (sprintSpeed) * Time.fixedDeltaTime;
                        }
                    }
                    // walk handler
                    else
                    {
                        // walking forward and turning left
                        if (Input.GetKey(KeyCode.A))
                        {
                            _rb.position += transform.forward * (walkSpeed) * Time.fixedDeltaTime;

                            rotateAmount *= rotateSpeed * Time.deltaTime;

                            transform.Rotate(Vector3.up, rotateAmount);
                        }
                        // walking forward and turning right
                        else if (Input.GetKey(KeyCode.D))
                        {
                            _rb.position += transform.forward * (walkSpeed) * Time.fixedDeltaTime;

                            rotateAmount *= rotateSpeed * Time.deltaTime;

                            transform.Rotate(Vector3.up, rotateAmount);
                        }
                        // walking forward
                        else
                        {
                            _rb.position += transform.forward * (walkSpeed) * Time.fixedDeltaTime;
                        }
                    }
                }
            }
            // backwards movement handler
            else if (Input.GetKey(KeyCode.S))
            {
                if (!tacticalCamera)
                {
                    // walking backwards and left
                    if (Input.GetKey(KeyCode.A))
                    {
                        _rb.position += -transform.forward * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                        _rb.position += -transform.right * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                    }
                    // walking backwards and right
                    else if (Input.GetKey(KeyCode.D))
                    {
                        _rb.position += -transform.forward * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                        _rb.position += transform.right * (walkSpeed) * 0.65f * Time.fixedDeltaTime;
                    }
                    // walking backwards
                    else
                    {
                        _rb.position += -transform.forward * (walkSpeed) * Time.fixedDeltaTime;
                    }
                }
                else if (tacticalCamera)
                {
                    // walking backwards and turning left
                    if (Input.GetKey(KeyCode.A))
                    {
                        _rb.position += -transform.forward * (walkSpeed) * 0.65f * Time.fixedDeltaTime;

                        rotateAmount *= rotateSpeed * Time.deltaTime;

                        transform.Rotate(Vector3.up, rotateAmount);
                    }
                    // walking backwards and turning right
                    else if (Input.GetKey(KeyCode.D))
                    {
                        _rb.position += -transform.forward * (walkSpeed) * 0.65f * Time.fixedDeltaTime;

                        rotateAmount *= rotateSpeed * Time.deltaTime;

                        transform.Rotate(Vector3.up, rotateAmount);
                    }
                    // walking backwards
                    else
                    {
                        _rb.position += -transform.forward * (walkSpeed) * Time.fixedDeltaTime;
                    }
                }
            }
            // left only movement handler
            else if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                // walk left
                if (!tacticalCamera)
                    _rb.position += -transform.right * (walkSpeed) * Time.fixedDeltaTime;
                // turn left
                else if (tacticalCamera)
                {
                    rotateAmount *= rotateSpeed * Time.deltaTime;

                    transform.Rotate(Vector3.up, rotateAmount);
                }
            }
            // right only movement handler
            else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                // walk right
                if (!tacticalCamera)
                    _rb.position += transform.right * (walkSpeed) * Time.fixedDeltaTime;
                // turn right
                else if (tacticalCamera)
                {
                    rotateAmount *= rotateSpeed * Time.deltaTime;

                    transform.Rotate(Vector3.up, rotateAmount);
                }
            }

            // jump handler
            if (Input.GetKey(KeyCode.Space))
            {
                if (remainingJumps > 0)
                {
                    _rb.velocity = (Vector2.down * 0);
                    _rb.AddForce(Vector2.up * (jumpPower), ForceMode.Impulse);

                    remainingJumps -= 1;
                }
            }
            isGrounded = Physics.Raycast(player.transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

            if (isGrounded)
                remainingJumps = (int)maxJumps;

            if (remainingJumps > (int)maxJumps)
                remainingJumps = (int)maxJumps;
        }
    }

    private void HandleLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;

                if (objectHit.tag.Equals("Destroy Me"))
                {
                    Destroy(objectHit.gameObject);
                }
                else if (objectHit.tag.Equals("Enemy"))
                {
                    enemy = objectHit.gameObject.GetComponent<Enemy>();

                    if (enemy != null)
                    {
                        enemy.currentHp -= 1;
                    }
                }
                else if (objectHit.tag.Equals("Move Me"))
                {
                    if (!objectMoved)
                    {
                        objectHit.transform.position = new Vector3(objectHit.transform.position.x, objectHit.transform.position.y, objectHit.transform.position.z - 7);
                        objectMoved = !objectMoved;
                    }
                    else if (objectMoved)
                    {
                        objectHit.transform.position = new Vector3(objectHit.transform.position.x, objectHit.transform.position.y, objectHit.transform.position.z + 7);
                        objectMoved = !objectMoved;
                    }
                }
                else if (objectHit.tag.Equals("Respawn Enemy"))
                {
                    RespawnEnemy respawnButton = objectHit.gameObject.GetComponent<RespawnEnemy>();
                    respawnButton.RespawnTheEnemy();
                }
            }
        }
    }
}