using UnityEngine;

[CreateAssetMenu(fileName = "PD_NewProduct", menuName = "Supermarket/Product Data")]
public class ProductData : ScriptableObject
{
    [Header("Identity")]
    public string productID;
    public string brandName;
    public string productName;
    public ProductCategory category;

    [Header("Storage")]
    public ShelfStorageType storageType;

    [Header("Pricing")]
    [Min(0f)] public float lowestPossiblePrice = 0.99f;
    [Min(0f)] public float highestPossiblePrice = 9.99f;
    [Min(0f)] public float startingPrice = 4.99f;

    [Header("Packaging")]
    public PackageDefinition packageDefinition;

    [Header("Visuals")]
    public Texture2D productTexture;
    public Sprite productIcon;

    public string DisplayName => $"{brandName} {productName}";

    private void OnValidate()
    {
        if (highestPossiblePrice < lowestPossiblePrice)
        {
            highestPossiblePrice = lowestPossiblePrice;
        }

        startingPrice = Mathf.Clamp(
            startingPrice,
            lowestPossiblePrice,
            highestPossiblePrice
        );
    }
}