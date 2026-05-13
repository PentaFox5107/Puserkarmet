using UnityEngine;

[CreateAssetMenu(fileName = "PKG_NewPackage", menuName = "Supermarket/Package Definition")]
public class PackageDefinition : ScriptableObject
{
    [Header("Identity")]
    public string packageID;
    public PackageType packageType;

    [Header("World Prefab")]
    [Tooltip("The base prefab used for this package shape.")]
    public GameObject basePrefab;

    [Header("Size")]
    [Tooltip("Physical package size in Unity units: X = width, Y = height/thickness, Z = depth/length.")]
    public Vector3 packageSize = Vector3.one;

    [Header("Shelf Layout")]
    [Min(1)] public int productsOnShelfX = 1;
    [Min(1)] public int productsOnShelfY = 1;
    [Min(1)] public int productsOnShelfZ = 1;

    [Header("Shelf Spacing")]
    [Min(0f)] public float spaceBetweenItemsX = 0.05f;
    [Min(0f)] public float spaceBetweenItemsY = 0.05f;
    [Min(0f)] public float spaceBetweenItemsZ = 0.02f;

    [Header("Box / Case Info")]
    [Min(1)] public int amountInBox = 12;
}