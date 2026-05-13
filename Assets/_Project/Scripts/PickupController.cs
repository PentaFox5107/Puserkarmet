using UnityEngine;
using UnityEngine.InputSystem;

public class PickupController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint;
    public FurniturePlacementController furniturePlacementController;

    [Header("Spawn Box")]
    public GameObject boxPrefab;
    public Vector3 boxSpawnPosition = new Vector3(0f, 1f, 10f);
    public bool spawnBoxInFrontOfPlayer = true;
    public float boxSpawnDistance = 2f;

    [Header("Pickup Settings")]
    public float pickupDistance = 3f;
    public float heldObjectMoveSpeed = 15f;

    [Header("Throw")]
    public float throwForce = 8f;

    [Header("Input")]
    public InputActionReference pickupAction;     // Left mouse
    public InputActionReference interactAction;   // E
    public InputActionReference throwAction;      // Right mouse
    public InputActionReference spawnBoxAction;   // C

    private Rigidbody heldRigidbody;
    private Collider heldCollider;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (furniturePlacementController == null)
        {
            furniturePlacementController = GetComponent<FurniturePlacementController>();
        }
    }

    private void OnEnable()
    {
        EnableAction(pickupAction);
        EnableAction(interactAction);
        EnableAction(throwAction);
        EnableAction(spawnBoxAction);
    }

    private void OnDisable()
    {
        DisableAction(pickupAction);
        DisableAction(interactAction);
        DisableAction(throwAction);
        DisableAction(spawnBoxAction);
    }

    private void Update()
    {
        if (spawnBoxAction != null && spawnBoxAction.action != null && spawnBoxAction.action.WasPressedThisFrame())
        {
            SpawnBox();
        }

        // If furniture placement is active, this script should not interfere.
        if (IsFurniturePlacementActive())
        {
            return;
        }

        // Left mouse: pick up or drop.
        if (pickupAction != null && pickupAction.action != null && pickupAction.action.WasPressedThisFrame())
        {
            HandlePickup();
        }

        // E: interact with the held object, like opening a shelf box.
        if (interactAction != null && interactAction.action != null && interactAction.action.WasPressedThisFrame())
        {
            HandleInteract();
        }

        // Right mouse: throw held object.
        if (throwAction != null && throwAction.action != null && throwAction.action.WasPressedThisFrame())
        {
            if (heldRigidbody != null)
            {
                ThrowHeldObject();
            }
        }
    }

    private void FixedUpdate()
    {
        if (heldRigidbody != null)
        {
            MoveHeldObject();
        }
    }

    private bool IsFurniturePlacementActive()
    {
        return furniturePlacementController != null && furniturePlacementController.IsMovingFurniture;
    }

    private void SpawnBox()
    {
        if (boxPrefab == null)
        {
            Debug.LogError("PickupController: Box Prefab is not assigned.");
            return;
        }

        Vector3 spawnPosition = boxSpawnPosition;

        if (spawnBoxInFrontOfPlayer && playerCamera != null)
        {
            spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * boxSpawnDistance;
        }

        Instantiate(boxPrefab, spawnPosition, Quaternion.identity);
    }

    private void HandlePickup()
    {
        if (heldRigidbody == null)
        {
            TryPickup();
        }
        else
        {
            DropHeldObject();
        }
    }

    private void HandleInteract()
    {
        if (heldRigidbody == null)
        {
            Debug.Log("Interact pressed, but no object is being held.");
            return;
        }

        FurnitureBox furnitureBox =
            heldRigidbody.GetComponent<FurnitureBox>();

        if (furnitureBox == null)
        {
            furnitureBox = heldRigidbody.GetComponentInChildren<FurnitureBox>();
        }

        if (furnitureBox == null)
        {
            furnitureBox = heldRigidbody.GetComponentInParent<FurnitureBox>();
        }

        if (furnitureBox != null)
        {
            if (furnitureBox.furniturePrefab == null)
            {
                Debug.LogError("Held box has FurnitureBox script, but Furniture Prefab is not assigned.");
                return;
            }

            Debug.Log("Turning held box into furniture: " + furnitureBox.furniturePrefab.name);
            StartPlacingFurnitureFromBox(furnitureBox.furniturePrefab);
            return;
        }

        MovableFurniture heldFurniture =
            heldRigidbody.GetComponent<MovableFurniture>();

        if (heldFurniture == null)
        {
            heldFurniture = heldRigidbody.GetComponentInChildren<MovableFurniture>();
        }

        if (heldFurniture == null)
        {
            heldFurniture = heldRigidbody.GetComponentInParent<MovableFurniture>();
        }

        if (heldFurniture != null)
        {
            TurnHeldFurnitureBackIntoBox();
            return;
        }

        Debug.Log("Held object is not a FurnitureBox or MovableFurniture: " + heldRigidbody.name);
    }

    private void TryPickup()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PickupController: No player camera assigned.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

            if (rb == null)
            {
                Debug.Log("Looked at object, but it has no Rigidbody.");
                return;
            }

            PickUp(rb);
        }
    }

    private void PickUp(Rigidbody rb)
    {
        heldRigidbody = rb;
        heldCollider = rb.GetComponentInChildren<Collider>();

        heldRigidbody.useGravity = false;
        heldRigidbody.linearVelocity = Vector3.zero;
        heldRigidbody.angularVelocity = Vector3.zero;
        heldRigidbody.freezeRotation = true;

        if (heldCollider != null && characterController != null)
        {
            Physics.IgnoreCollision(heldCollider, characterController, true);
        }
    }

    private void MoveHeldObject()
    {
        if (holdPoint == null)
        {
            Debug.LogError("PickupController: Hold Point is not assigned.");
            return;
        }

        Vector3 targetPosition = holdPoint.position;
        Vector3 direction = targetPosition - heldRigidbody.position;

        heldRigidbody.linearVelocity = direction * heldObjectMoveSpeed;
        heldRigidbody.MoveRotation(holdPoint.rotation);
    }

    private void DropHeldObject()
    {
        if (heldCollider != null && characterController != null)
        {
            Physics.IgnoreCollision(heldCollider, characterController, false);
        }

        heldRigidbody.useGravity = true;
        heldRigidbody.freezeRotation = false;

        heldRigidbody = null;
        heldCollider = null;
    }

    private void ThrowHeldObject()
    {
        Rigidbody rb = heldRigidbody;

        if (heldCollider != null && characterController != null)
        {
            Physics.IgnoreCollision(heldCollider, characterController, false);
        }

        rb.useGravity = true;
        rb.freezeRotation = false;

        heldRigidbody = null;
        heldCollider = null;

        rb.linearVelocity = playerCamera.transform.forward * throwForce;
    }

    private void StartPlacingFurnitureFromBox(GameObject furniturePrefab)
    {
        if (furniturePlacementController == null)
        {
            Debug.LogError("PickupController: FurniturePlacementController is not assigned.");
            return;
        }

        if (furniturePrefab == null)
        {
            Debug.LogError("PickupController: Cannot place furniture because furniturePrefab is null.");
            return;
        }

        GameObject heldBoxObject = heldRigidbody.gameObject;

        if (heldCollider != null && characterController != null)
        {
            Physics.IgnoreCollision(heldCollider, characterController, false);
        }

        heldRigidbody = null;
        heldCollider = null;

        Destroy(heldBoxObject);

        furniturePlacementController.BeginPlacingNewFurniture(furniturePrefab, boxPrefab);
    }

    private void TurnHeldFurnitureBackIntoBox()
    {
        if (boxPrefab == null)
        {
            Debug.LogError("PickupController: Box Prefab is not assigned.");
            return;
        }

        GameObject heldObject = heldRigidbody.gameObject;
        Vector3 boxPosition = heldObject.transform.position + Vector3.up * 0.5f;

        if (heldCollider != null && characterController != null)
        {
            Physics.IgnoreCollision(heldCollider, characterController, false);
        }

        heldRigidbody = null;
        heldCollider = null;

        Destroy(heldObject);
        Instantiate(boxPrefab, boxPosition, Quaternion.identity);
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Enable();
        }
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Disable();
        }
    }
}