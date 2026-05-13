using UnityEngine;
using UnityEngine.InputSystem;

public class FurniturePlacementController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Prefabs")]
    public GameObject defaultReturnBoxPrefab;

    [Header("Settings")]
    public float interactionDistance = 4f;
    public float holdTimeToMove = 3f;
    public float placementDistance = 3f;
    public float moveSmoothness = 12f;

    [Header("Placement")]
    public bool snapToGrid = false;
    public float gridSize = 0.5f;
    public float newFurnitureY = 1f;

    [Header("Input")]
    public InputActionReference placeAction;      // Left mouse
    public InputActionReference interactAction;   // E

    private MovableFurniture lookedAtFurniture;
    private MovableFurniture movingFurniture;

    public bool IsMovingFurniture
    {
        get { return movingFurniture != null; }
    }

    private float holdTimer;
    private float lockedY;
    private Quaternion lockedRotation;

    private GameObject returnBoxPrefab;
    private bool isPlacingFromBox;

    private float placementInputDelay;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    private void OnEnable()
    {
        EnableAction(placeAction);
        EnableAction(interactAction);
    }

    private void OnDisable()
    {
        DisableAction(placeAction);
        DisableAction(interactAction);
    }

    private void Update()
    {
        if (placementInputDelay > 0f)
        {
            placementInputDelay -= Time.deltaTime;
        }

        if (movingFurniture == null)
        {
            CheckForFurniture();
            HandleHoldToMove();
        }
        else
        {
            MoveFurnitureHorizontally();
            HandlePlaceFurniture();
            HandleTurnFurnitureBackIntoBox();
        }
    }

    public void BeginPlacingNewFurniture(GameObject furniturePrefab, GameObject boxPrefab)
    {
        if (furniturePrefab == null)
        {
            Debug.LogError("FurniturePlacementController: Furniture prefab is missing.");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("FurniturePlacementController: Player camera is missing.");
            return;
        }

        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * placementDistance;
        spawnPosition.y = newFurnitureY;

        Quaternion spawnRotation = furniturePrefab.transform.rotation;

        GameObject furnitureObject = Instantiate(furniturePrefab, spawnPosition, spawnRotation);

        MovableFurniture furniture = furnitureObject.GetComponent<MovableFurniture>();

        if (furniture == null)
        {
            furniture = furnitureObject.AddComponent<MovableFurniture>();
        }

        returnBoxPrefab = boxPrefab;
        isPlacingFromBox = true;

        BeginMovingFurniture(furniture, true);
    }

    private void CheckForFurniture()
    {
        lookedAtFurniture = null;

        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            lookedAtFurniture = hit.collider.GetComponentInParent<MovableFurniture>();
        }
    }

    private void HandleHoldToMove()
    {
        if (lookedAtFurniture == null)
        {
            holdTimer = 0f;
            return;
        }

        if (placeAction == null || placeAction.action == null)
        {
            return;
        }

        if (placeAction.action.IsPressed())
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTimeToMove)
            {
                isPlacingFromBox = false;
                returnBoxPrefab = null;

                BeginMovingFurniture(lookedAtFurniture, false);
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void BeginMovingFurniture(MovableFurniture furniture, bool createdFromBox)
    {
        movingFurniture = furniture;

        if (createdFromBox)
        {
            lockedY = newFurnitureY;
        }
        else
        {
            lockedY = movingFurniture.transform.position.y;
        }

        lockedRotation = movingFurniture.transform.rotation;
        placementInputDelay = 0.2f;

        Rigidbody rb = movingFurniture.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Moving furniture: " + movingFurniture.name);
    }

    private void MoveFurnitureHorizontally()
    {
        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * placementDistance;

        targetPosition.y = lockedY;

        if (snapToGrid)
        {
            targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
            targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
        }

        movingFurniture.transform.position = Vector3.Lerp(
            movingFurniture.transform.position,
            targetPosition,
            moveSmoothness * Time.deltaTime
        );

        movingFurniture.transform.rotation = lockedRotation;
    }

    private void HandlePlaceFurniture()
    {
        if (placementInputDelay > 0f)
        {
            return;
        }

        if (placeAction == null || placeAction.action == null)
        {
            return;
        }

        if (placeAction.action.WasPressedThisFrame())
        {
            PlaceFurniture();
        }
    }

    private void HandleTurnFurnitureBackIntoBox()
    {
        if (placementInputDelay > 0f)
        {
            return;
        }

        if (interactAction == null || interactAction.action == null)
        {
            return;
        }

        if (interactAction.action.WasPressedThisFrame())
        {
            TurnFurnitureBackIntoBox();
        }
    }

    private void PlaceFurniture()
    {
        Rigidbody rb = movingFurniture.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Placed furniture: " + movingFurniture.name);

        movingFurniture = null;
        returnBoxPrefab = null;
        isPlacingFromBox = false;
    }

    private void TurnFurnitureBackIntoBox()
{
    GameObject boxPrefabToSpawn = returnBoxPrefab;

    if (boxPrefabToSpawn == null)
    {
        boxPrefabToSpawn = defaultReturnBoxPrefab;
    }

    if (boxPrefabToSpawn == null)
    {
        Debug.LogError("FurniturePlacementController: No box prefab assigned. Assign Default Return Box Prefab in the Inspector.");
        return;
    }

    Vector3 boxPosition = movingFurniture.transform.position + Vector3.up * 1f;
    GameObject furnitureObject = movingFurniture.gameObject;

    movingFurniture = null;

    Destroy(furnitureObject);

    Instantiate(boxPrefabToSpawn, boxPosition, boxPrefabToSpawn.transform.rotation);

    returnBoxPrefab = null;
    isPlacingFromBox = false;

    Debug.Log("Furniture turned back into box.");
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