using UnityEngine;

[DisallowMultipleComponent]
public class ProductObject : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ProductData productData;

    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("URP/Lit uses _BaseMap. Built-in Standard uses _MainTex.")]
    [SerializeField] private string texturePropertyName = "_BaseMap";

    private MaterialPropertyBlock propertyBlock;

    public ProductData Data => productData;

    public string ProductID => productData != null ? productData.productID : string.Empty;

    public string DisplayName
    {
        get
        {
            if (productData == null) return "Missing Product Data";
            return productData.DisplayName;
        }
    }

    public float Price
    {
        get
        {
            if (productData == null) return 0f;
            return productData.startingPrice;
        }
    }

    private void Reset()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void Awake()
    {
        ApplyProductData();
    }

    private void OnValidate()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        ApplyProductData();
    }

    public void SetProductData(ProductData newProductData)
    {
        productData = newProductData;
        ApplyProductData();
    }

    public void ApplyProductData()
    {
        if (productData == null || targetRenderer == null || productData.productTexture == null)
        {
            return;
        }

        propertyBlock ??= new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(texturePropertyName, productData.productTexture);
        targetRenderer.SetPropertyBlock(propertyBlock);

        gameObject.name = $"Product_{productData.productID}";
    }
}