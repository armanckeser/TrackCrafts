using HarmonyLib;
using System;
using Bloodstone.API;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectM.UI;

namespace TrackCrafts;

[HarmonyPatch]
public class TrackCrafts : MonoBehaviour
{
    public static TrackCrafts Instance;
    private static GameObject RecipesParent { get; set; }
    private static GameObject Layout { get; set; }

    private static VerticalLayoutGroup RecipesParentLayout { get; set; }
    private static ContentSizeFitter RecipesParentSizeFitter { get; set; }

    private Keybinding ClearPinnedKeybind { get; set; }
    private Keybinding HidePinnedKeybind { get; set; }

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
            DestroyRecipes();
        }

        if (Input.GetKeyDown(HidePinnedKeybind.Primary) || Input.GetKeyDown(HidePinnedKeybind.Secondary))
        {
            HideRecipes();
        }
    }

    [HarmonyPatch(typeof(GridSelectionEntry), nameof(GridSelectionEntry.OnPointerClick))]
    [HarmonyPostfix]
    private static void OnPointerClick(GridSelectionEntry __instance, PointerEventData eventData)
    {
        //this path changed
        Layout ??= GameObject.Find("HUDCanvas(Clone)/JournalCanvas/JournalParent(Clone)/Layout");
        RecipesParent ??= new GameObject("Recipes Parent")
        {
            transform =
            {
                localScale = new Vector3(Plugin.TrackerItemScale.Value, Plugin.TrackerItemScale.Value, 1f),
                parent = Layout.transform
            }
        };
        
        RecipesParentLayout ??= RecipesParent.AddComponent<VerticalLayoutGroup>();
        RecipesParentLayout.childControlHeight = true;
        RecipesParentLayout.childControlWidth = true;
        RecipesParentLayout.spacing = 20;

        RecipesParentSizeFitter ??= RecipesParent.AddComponent<ContentSizeFitter>();

        var entry = __instance.GetComponent<WorkstationRecipeGridSelectionEntry>();
        if (entry == null)
            return;
        if (eventData.button != PointerEventData.InputButton.Middle) return;
        if (RecipesParent.transform.GetChildCount() < Plugin.TrackQuantity.Value)
            PinRecipe(__instance);
    }

    private static void HideRecipes()
    {
        if (RecipesParent != null)
        {
            RecipesParent.active = !RecipesParent.active;
        }
    }

    private static void DestroyRecipes()
    {
        //no need to destroy children. When you destroy the parent, the children are destroyed as well
        if (RecipesParent == null) return;
        Destroy(RecipesParent);
        RecipesParent = null;
    }


    private static void PinRecipe(GridSelectionEntry gridSelectionEntry)
    {
        var currentTooltip = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDMenuParent/WorkstationMenu(Clone)/MenuParent/WorkstationSubMenu(Clone)/MotionRoot/FakeTooltip");
        if (currentTooltip == null) return;
        CreateAndAttachFakeToolTip(currentTooltip, RecipesParent, gridSelectionEntry);
    }


    private static void CreateAndAttachFakeToolTip(GameObject currentTooltip, GameObject parent, GridSelectionEntry gridSelectionEntry)
    {
        var tooltip = Instantiate(currentTooltip, parent.transform);
        var layoutGroup = tooltip.GetComponent<VerticalLayoutGroup>();
        
        layoutGroup.childControlHeight = true;
        if (tooltip.transform.childCount < Plugin.TrackQuantity.Value) return;
        tooltip.transform.GetChild(3).gameObject.SetActive(false);
        //this transform name changed
        var entries = tooltip.transform.Find("InformationEntries");

        var entriesGameObject = entries.gameObject;
        DeactivateExtraText(entriesGameObject);
        layoutGroup.OnTransformChildrenChanged();

        tooltip.AddComponent<GraphicRaycaster>();
        var sampleButton = GameObject.Find("HUDCanvas(Clone)/Canvas/HUDMenuParent/WorkstationMenu(Clone)/MenuParent/CharacterInventorySubMenu2(Clone)/MotionRoot/SharedForNowAtLeast/SortButton");

        var deleteButtonObject = Instantiate(sampleButton, tooltip.transform);

        deleteButtonObject.name = "Delete Button";
        var textComponent = deleteButtonObject.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();


        textComponent.text = "X";
        var button = deleteButtonObject.GetComponent<SimpleStunButton>();

        void DeleteOnClick() => Destroy(tooltip);
        button.onClick.AddListener(new Action(DeleteOnClick));

        tooltip.SetActive(true);
        tooltip.name = $"TrackCrafts_{gridSelectionEntry.name}";
    }

    private static Transform FindTransform(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == name)
        {
            return parent;
        }

        foreach (var o in parent)
        {
            var child = o.TryCast<Transform>();
            var result = FindTransform(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }


    private static void DeactivateExtraText(GameObject entries)
    {
        var levelIcon = FindTransform(entries.gameObject.transform.parent, "LevelFrame");
        if (levelIcon != null)
        {
            levelIcon.gameObject.SetActive(false);
        }

        foreach (var o in entries.transform)
        {
            var child = o.TryCast<Transform>();
            if (child == null) continue;
            var childGameObject = child.gameObject;
            if (childGameObject == null) continue;
            if (!child.name.Equals("Tooltip_RecipeRequiredItems"))
            {
                childGameObject.SetActive(false);
            }
        }
    }

    public static void ResetAll()
    {
        DestroyRecipes();
    }
}