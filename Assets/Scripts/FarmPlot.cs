using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FarmPlot : MonoBehaviour
{
    public enum PlotState
    {
        Empty,
        Planted,
        Harvestable
    }

    public enum CropState
    {
        Seed,
        Small,
        Large,
        Mature
    }

    [Header("Pengaturan Petak Tanah")]
    public PlotState currentPlotState = PlotState.Empty;
    public CropState currentCropState = CropState.Seed;

    [Header("Pengaturan Pertumbuhan Tanaman")]
    [SerializeField] private float timeToGrowSmall = 5f;
    [SerializeField] private float timeToGrowLarge = 5f;
    [SerializeField] private float scaleGrowthDuration = 0.5f;

    [Header("Referensi Visual")]
    [SerializeField] private GameObject emptyVisual;
    [SerializeField] private GameObject seedVisual;
    [SerializeField] private GameObject smallCropVisual;
    [SerializeField] private GameObject largeCropVisual;

    [Header("Hasil Panen")]
    [SerializeField] private ItemData harvestItem;
    [SerializeField] private int harvestAmount = 1;

    private Coroutine growRoutine;
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    void Awake()
    {
        if (seedVisual != null) originalScales.Add(seedVisual, seedVisual.transform.localScale);
        if (smallCropVisual != null) originalScales.Add(smallCropVisual, smallCropVisual.transform.localScale);
        if (largeCropVisual != null) originalScales.Add(largeCropVisual, largeCropVisual.transform.localScale);

        SetVisualScalesToZero();
    }

    void Start()
    {
        if (emptyVisual == null || seedVisual == null || smallCropVisual == null || largeCropVisual == null)
        {
            Debug.LogError("Pastikan semua visual (Empty, Seed, Small, Large) diatur di Inspector FarmPlot untuk: " + gameObject.name);
            enabled = false;
            return;
        }

        UpdateVisuals();

        if (currentPlotState == PlotState.Planted || currentPlotState == PlotState.Harvestable)
        {
            Transform activeVisualTransform = GetActiveCropVisualTransform();
            if (activeVisualTransform != null && originalScales.ContainsKey(activeVisualTransform.gameObject))
            {
                activeVisualTransform.localScale = originalScales[activeVisualTransform.gameObject];
            }
            if (currentPlotState == PlotState.Planted)
            {
                StartGrowthProcess();
            }
        }
        else
        {
            if (emptyVisual != null) emptyVisual.transform.localScale = Vector3.one;
        }
    }

    public bool Interact(ItemData itemInHand)
    {
        if (currentPlotState == PlotState.Empty)
        {
            if (itemInHand != null && itemInHand.isSeed)
            {
                return PlantSeed(itemInHand);
            }
            else
            {
                Debug.Log(gameObject.name + ": Petak kosong. Pegang bibit untuk menanam!");
                return false;
            }
        }
        else if (currentPlotState == PlotState.Harvestable)
        {
            HarvestCrop();
            return false;
        }
        else if (currentPlotState == PlotState.Planted)
        {
            Debug.Log(gameObject.name + ": Tanaman masih tumbuh... (" + currentCropState + ")");
            return false;
        }
        return false;
    }


    private bool PlantSeed(ItemData seedItem)
    {
        if (InventoryManager.Instance.RemoveItem(seedItem, 1))
        {
            currentPlotState = PlotState.Planted;
            currentCropState = CropState.Seed;
            Debug.Log(gameObject.name + " - Bibit " + seedItem.itemName + " telah ditanam!");
            UpdateVisuals();
            StartGrowthProcess();
            return true;
        }
        else
        {
            Debug.LogWarning("Tidak cukup bibit " + seedItem.itemName + " di inventaris untuk menanam.");
            return false;
        }
    }

    private void StartGrowthProcess()
    {
        if (growRoutine != null)
        {
            StopCoroutine(growRoutine);
        }
        growRoutine = StartCoroutine(GrowCropRoutine());
    }

    private IEnumerator GrowCropRoutine()
    {
        Vector3 originalScale;

        if (seedVisual != null && originalScales.TryGetValue(seedVisual, out originalScale))
        {
            seedVisual.transform.localScale = Vector3.zero;
            yield return StartCoroutine(AnimateScaleGrowth(seedVisual.transform, originalScale));
        }

        if (currentCropState <= CropState.Seed)
        {
            yield return new WaitForSeconds(timeToGrowSmall);
            currentCropState = CropState.Small;
            Debug.Log(gameObject.name + " - Tanaman tumbuh ke fase Small!");
            UpdateVisuals();
            if (smallCropVisual != null && originalScales.TryGetValue(smallCropVisual, out originalScale))
            {
                smallCropVisual.transform.localScale = Vector3.zero;
                yield return StartCoroutine(AnimateScaleGrowth(smallCropVisual.transform, originalScale));
            }
        }

        if (currentCropState <= CropState.Small)
        {
            yield return new WaitForSeconds(timeToGrowLarge);
            currentCropState = CropState.Large;
            Debug.Log(gameObject.name + " - Tanaman tumbuh ke fase Large (Siap Panen)!");
            UpdateVisuals();
            if (largeCropVisual != null && originalScales.TryGetValue(largeCropVisual, out originalScale))
            {
                largeCropVisual.transform.localScale = Vector3.zero;
                yield return StartCoroutine(AnimateScaleGrowth(largeCropVisual.transform, originalScale));
            }

            currentPlotState = PlotState.Harvestable;
            Debug.Log(gameObject.name + " - Tanaman siap dipanen!");
        }
    }

    private IEnumerator AnimateScaleGrowth(Transform targetTransform, Vector3 targetScale)
    {
        if (targetTransform == null) yield break;

        Vector3 startScale = targetTransform.localScale;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime / scaleGrowthDuration;
            targetTransform.localScale = Vector3.Lerp(startScale, targetScale, timer);
            yield return null;
        }
        targetTransform.localScale = targetScale;
    }

    private Transform GetActiveCropVisualTransform()
    {
        switch (currentCropState)
        {
            case CropState.Seed: return seedVisual.transform;
            case CropState.Small: return smallCropVisual.transform;
            case CropState.Large: return largeCropVisual.transform;
            case CropState.Mature: return largeCropVisual.transform;
            default: return null;
        }
    }

    private void HarvestCrop()
    {
        if (currentPlotState == PlotState.Harvestable)
        {
            Debug.Log(gameObject.name + " - Tanaman dipanen!");

            if (harvestItem != null)
            {
                bool addedToInventory = InventoryManager.Instance.AddItem(harvestItem, harvestAmount);
                if (addedToInventory)
                {
                    Debug.Log($"Berhasil menambahkan {harvestAmount}x {harvestItem.itemName} ke inventaris!");
                }
                else
                {
                    Debug.LogWarning($"Gagal menambahkan {harvestItem.itemName} ke inventaris (mungkin penuh).");
                }
            }
            else
            {
                Debug.LogWarning("Harvest Item belum diatur di Inspector FarmPlot untuk: " + gameObject.name);
            }

            if (growRoutine != null)
            {
                StopCoroutine(growRoutine);
                growRoutine = null;
            }

            currentPlotState = PlotState.Empty;
            currentCropState = CropState.Seed;

            UpdateVisuals();
            if (emptyVisual != null) emptyVisual.transform.localScale = Vector3.one;
        }
    }

    private void UpdateVisuals()
    {
        emptyVisual.SetActive(false);
        seedVisual.SetActive(false);
        smallCropVisual.SetActive(false);
        largeCropVisual.SetActive(false);

        switch (currentPlotState)
        {
            case PlotState.Empty:
                emptyVisual.SetActive(true);
                break;
            case PlotState.Planted:
            case PlotState.Harvestable:
                switch (currentCropState)
                {
                    case CropState.Seed:
                        seedVisual.SetActive(true);
                        break;
                    case CropState.Small:
                        smallCropVisual.SetActive(true);
                        break;
                    case CropState.Large:
                    case CropState.Mature:
                        largeCropVisual.SetActive(true);
                        break;
                }
                break;
        }
    }

    private void SetVisualScalesToZero()
    {
        if (seedVisual != null) seedVisual.transform.localScale = Vector3.zero;
        if (smallCropVisual != null) smallCropVisual.transform.localScale = Vector3.zero;
        if (largeCropVisual != null) largeCropVisual.transform.localScale = Vector3.zero;
    }
}