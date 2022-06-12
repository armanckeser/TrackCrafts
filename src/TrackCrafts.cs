using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectM;
using ProjectM.UI;
using Wetstone.API;

namespace TrackCrafts;

[HarmonyPatch]
public class TrackCrafts : MonoBehaviour
{
    public static TrackCrafts Instance;
    private static GameObject recipesParent;
    private static GameObject layout;
    private Keybinding ClearPinnedKeybind;
    private Keybinding HidePinnedKeybind;

    private void Awake()
    {
        Instance = this;

        ClearPinnedKeybind = KeybindManager.Register(new KeybindingDescription
        {
            Id = "TrackCraftsClear",
            Category = "Track Crafting Recipes",
            Name = "Clear pinned recipes",
            DefaultKeybinding = KeyCode.F1
        });

        HidePinnedKeybind = KeybindManager.Register(new KeybindingDescription
        {
            Id = "TrackCraftsHide",
            Category = "Track Crafting Recipes",
            Name = "Hide/Show pinned recipes",
            DefaultKeybinding = KeyCode.F2
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(ClearPinnedKeybind.Primary) || Input.GetKeyDown(ClearPinnedKeybind.Secondary))
        {
            destroyRecipes();
        }

        if (Input.GetKeyDown(HidePinnedKeybind.Primary) || Input.GetKeyDown(HidePinnedKeybind.Secondary))
        {
            hideRecipes();
        }
    }

    [HarmonyPatch(typeof(GridSelectionEntry), nameof(GridSelectionEntry.OnPointerClick))]
    [HarmonyPostfix]
    static void OnPointerClick(GridSelectionEntry __instance, PointerEventData eventData)
    {
        if (layout == null)
            layout = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDAchievements/JournalParent(Clone)/Layout");
        if (recipesParent == null)
        {
            recipesParent = new GameObject("Recipes Parent");
            recipesParent.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            recipesParent.transform.parent = layout.transform;
            VerticalLayoutGroup recipesParentLayout = recipesParent.AddComponent<VerticalLayoutGroup>();
            recipesParentLayout.childControlHeight = true;
            recipesParentLayout.childControlWidth = true;
            recipesParentLayout.spacing = 20;
            recipesParent.AddComponent<ContentSizeFitter>();
        }
        var entry = __instance.GetComponent<WorkstationRecipeGridSelectionEntry>();
        if (entry == null)
            return;
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            if (recipesParent.transform.GetChildCount() < 3)
                pinRecipe();
        }
    }

    private static void hideRecipes()
    {
        if (recipesParent != null)
        {
            recipesParent.active = !recipesParent.active;
        }
    }
    private static void destroyRecipes()
    {
        if (recipesParent != null)
        {
            foreach (Transform child in recipesParent.transform.GetAllChildren())
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            UnityEngine.Object.Destroy(recipesParent);
            recipesParent = null;
        }
    }

    private static void pinRecipe()
    {
        GameObject currentTooltip = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDMenuParent/WorkstationMenu(Clone)/MenuParent/WorkstationSubMenu(Clone)/MotionRoot/FakeTooltip");
        if (currentTooltip == null)
            return;
        createAndAttachFakeToolTip(currentTooltip, recipesParent);
    }


    private static void createAndAttachFakeToolTip(GameObject currentTooltip, GameObject parent)
    {
        GameObject tooltip = Instantiate(currentTooltip, parent.transform);
        VerticalLayoutGroup layoutGroup = tooltip.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        tooltip.transform.GetChild(2).gameObject.active = false;
        GameObject entries = tooltip.transform.FindChild("Entries").gameObject;
        deactivateExtraText(entries);
        layoutGroup.OnTransformChildrenChanged();
        tooltip.AddComponent<GraphicRaycaster>();
        GameObject sampleButton = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDMenuParent/WorkstationMenu(Clone)/MenuParent/CharacterInventorySubMenu2(Clone)/MotionRoot/SharedForNowAtLeast/SortButton");
        GameObject buttonParent = tooltip;
        GameObject deleteButtonObject = Instantiate(sampleButton, buttonParent.transform);
        deleteButtonObject.name = "Delete Button";
        deleteButtonObject.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "X";
        SimpleStunButton button = deleteButtonObject.GetComponent<ProjectM.UI.SimpleStunButton>();
        var deleteOnClick = () => UnityEngine.Object.Destroy(tooltip);
        button.onClick.AddListener(new Action(deleteOnClick));
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
        destroyRecipes();
    }
}
