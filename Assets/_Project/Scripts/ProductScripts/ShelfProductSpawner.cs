using UnityEngine;

public class ShelfProductSpawner : MonoBehaviour
{
    [Header("Product")]
    [SerializeField] private ProductData productData;

    [Header("Spawn")]
    [SerializeField] private Transform startPoint;

    [ContextMenu("Fill Shelf")]
    public void FillShelf()
    {
        if (productData == null)
        {
            Debug.LogWarning("No ProductData assigned.");
            return;
        }

        PackageDefinition package = productData.packageDefinition;

        if (package == null || package.basePrefab == null)
        {
            Debug.LogWarning($"Product {productData.name} is missing package definition or base prefab.");
            return;
        }

        if (startPoint == null)
        {
            Debug.LogWarning("No start point assigned.");
            return;
        }

        for (int z = 0; z < package.productsOnShelfZ; z++)
        {
            for (int y = 0; y < package.productsOnShelfY; y++)
            {
                for (int x = 0; x < package.productsOnShelfX; x++)
                {
                    Vector3 offset = new Vector3(
                        x * (package.packageSize.x + package.spaceBetweenItemsX),
                        z * (package.packageSize.y + package.spaceBetweenItemsZ),
                        y * (package.packageSize.z + package.spaceBetweenItemsY)
                    );

                    GameObject spawnedProduct = Instantiate(
                        package.basePrefab,
                        startPoint.position + offset,
                        startPoint.rotation,
                        transform
                    );

                    ProductObject productObject = spawnedProduct.GetComponent<ProductObject>();

                    if (productObject != null)
                    {
                        productObject.SetProductData(productData);
                    }
                }
            }
        }
    }
}