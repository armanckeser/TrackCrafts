using HarmonyLib;
using System.Collections;
using ProjectM;
using ProjectM.UI;
using Unity.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TrackCrafts;

[HarmonyPatch]
public class TrackCrafts : MonoBehaviour
{
    private static GameObject tooltip = null;
    private static GameObject layout = null;

    [HarmonyPatch(typeof(GridSelectionEntry), nameof(GridSelectionEntry.OnPointerEnter))]
    [HarmonyPostfix]
    static void OnPointerEnter(GridSelectionEntry __instance, PointerEventData eventData)
    {
        var entry = __instance.GetComponent<WorkstationRecipeGridSelectionEntry>();
        if (entry == null)
            return;
        layout = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDAchievements/JournalParent(Clone)/Layout");

    }
    [HarmonyPatch(typeof(GridSelectionEntry), nameof(GridSelectionEntry.OnPointerClick))]
    [HarmonyPostfix]
    static void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            if (tooltip != null)
                UnityEngine.Object.Destroy(tooltip);
            pinRecipe();
        }
    }

    private static void pinRecipe()
    {
        GameObject currentTooltip = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDMenuParent/WorkstationMenu(Clone)/MenuParent/WorkstationSubMenu(Clone)/MotionRoot/FakeTooltip");
        // recipe = new PinnedRecipe();
        // recipe.container = new GameObject("Recipe");
        // setupRecipeContainer();
        createAndAttachFakeToolTip(currentTooltip);
    }

    // private static void setupRecipeContainer()
    // {
    //     recipe.container.transform.parent = layout.transform;
    //     recipe.container.transform.localPosition = new UnityEngine.Vector3(0, 0, 0);
    //     recipe.container.transform.localScale = new UnityEngine.Vector3(1, 1, 1);
    //     recipe.container.AddComponent<BoxCollider>();
    //     // recipe.container.AddComponent<DestroyOnClick>();
    // }

    private static void createAndAttachFakeToolTip(GameObject currentTooltip)
    {
        Plugin.Logger.LogDebug($"In CreateandAttach");
        tooltip = Instantiate(currentTooltip, layout.transform);
        VerticalLayoutGroup layoutGroup = tooltip.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        tooltip.transform.localScale = new UnityEngine.Vector3(0.75f, .75f, .75f);
        Plugin.Logger.LogDebug($"Setting position from {tooltip.transform.localPosition}");
        tooltip.transform.localPosition = new UnityEngine.Vector3(365, tooltip.transform.localPosition.y, tooltip.transform.localPosition.z);
        Plugin.Logger.LogDebug($"Setting position to {tooltip.transform.localPosition}");
        tooltip.transform.GetChild(2).gameObject.active = false;
        GameObject entries = tooltip.transform.FindChild("Entries").gameObject;
        deactivateExtraText(entries);
        layoutGroup.OnTransformChildrenChanged();
        tooltip.active = true;
    }

    private static void deactivateExtraText(GameObject entries)
    {
        entries.transform.FindChild("Stats").gameObject.active = false;
        entries.transform.FindChild("TooltipDurability").gameObject.active = false;
        entries.transform.FindChild("TooltipDesc").gameObject.active = false;
    }

    public static void Reset()
    {
        if (tooltip != null)
            UnityEngine.Object.Destroy(tooltip);
        tooltip = null;
    }
    // class PinnedRecipe
    // {
    //     public GameObject container;

    //     public void Destroy()
    //     {
    //         container = null;
    //     }
    // }

    // class DestroyOnClick : MonoBehaviour
    // {
    //     private void OnMouseDown()
    //     {
    //         Destroy(gameObject);
    //     }
    // }
}
