using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Wetstone.API;

namespace TrackCrafts;

[HarmonyPatch]
public class TrackCrafts : MonoBehaviour
{
    public static TrackCrafts Instance;
    private static GameObject tooltip;
    private static GameObject layout;
    private Keybinding ClearPinnedKeybind;

    private void Awake()
    {
        Instance = this;

        ClearPinnedKeybind = KeybindManager.Register(new KeybindingDescription
        {
            Id = "TrackCrafts",
            Category = "Track Crafting Recipes",
            Name = "Clear pinned recipe",
            DefaultKeybinding = KeyCode.F1
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(ClearPinnedKeybind.Primary) || Input.GetKeyDown(ClearPinnedKeybind.Secondary))
        {
            UnityEngine.Object.Destroy(tooltip);
            tooltip = null;
        }
    }

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
            {
                UnityEngine.Object.Destroy(tooltip);
                tooltip = null;
            }
            pinRecipe();
        }
    }

    private static void pinRecipe()
    {
        GameObject currentTooltip = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDMenuParent/WorkstationMenu(Clone)/MenuParent/WorkstationSubMenu(Clone)/MotionRoot/FakeTooltip");
        createAndAttachFakeToolTip(currentTooltip);
    }

    private static void createAndAttachFakeToolTip(GameObject currentTooltip)
    {
        tooltip = Instantiate(currentTooltip, layout.transform);
        VerticalLayoutGroup layoutGroup = tooltip.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        tooltip.transform.GetChild(2).gameObject.active = false;
        GameObject entries = tooltip.transform.FindChild("Entries").gameObject;
        deactivateExtraText(entries);
        tooltip.active = true;
        layoutGroup.OnTransformChildrenChanged();
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
        {
            UnityEngine.Object.Destroy(tooltip);
            tooltip = null;
        }
    }
}
