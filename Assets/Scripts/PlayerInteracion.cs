using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Pengaturan Interaksi")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Referensi")]
    [SerializeField] private Transform interactionOrigin;
    
    private HandheldItemController handheldItemController;

    void Start()
    {
        if (interactionOrigin == null)
        {
            Debug.LogError("Referensi 'Interaction Origin' belum diatur pada PlayerInteraction!");
            enabled = false;
            return;
        }

        handheldItemController = GetComponent<HandheldItemController>();
        if (handheldItemController == null)
        {
            Debug.LogError("HandheldItemController tidak ditemukan pada GameObject pemain. Pastikan sudah terpasang!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformInteraction();
        }
    }

    private void PerformInteraction()
    {
        Ray ray = new Ray(interactionOrigin.position, interactionOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            FarmPlot farmPlot = hit.collider.GetComponent<FarmPlot>();
            if (farmPlot != null)
            {
                ItemData itemInHand = handheldItemController.GetCurrentItemInHand();

                bool itemConsumed = farmPlot.Interact(itemInHand);
                if (itemConsumed)
                {
                    handheldItemController.ClearHand();
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (interactionOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(interactionOrigin.position, interactionOrigin.forward * interactionDistance);
        }
    }
}