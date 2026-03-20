using System;
using Duckov.UI;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TotemOfUndying;

internal sealed class WorkbenchCraftSystem : MonoBehaviour
{
    private static WorkbenchCraftSystem? _instance;

    private ItemCustomizeSelectionView? _boundView;
    private GameObject? _panelRoot;
    private Image? _panelBackground;
    private TextMeshProUGUI? _titleText;
    private TextMeshProUGUI? _recipeText;
    private Button? _craftButton;
    private TextMeshProUGUI? _craftButtonText;

    internal static void Initialize()
    {
        if (_instance != null)
        {
            return;
        }

        var go = new GameObject("TotemOfUndying_WorkbenchCraftSystem");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<WorkbenchCraftSystem>();
    }

    internal static void Deinitialize()
    {
        if (_instance == null)
        {
            return;
        }

        Destroy(_instance.gameObject);
        _instance = null;
    }

    private void Update()
    {
        var view = ItemCustomizeSelectionView.Instance;
        if (view == null)
        {
            return;
        }

        if (_boundView != view)
        {
            BindView(view);
        }

        RefreshPanelState();
    }

    private void BindView(ItemCustomizeSelectionView view)
    {
        _boundView = view;
        ModLog.Info("[TotemOfUndying] Binding ItemCustomizeSelectionView for custom workbench panel.");

        var beginCustomizeButton = ReflectionUtil.GetPrivateField<Button>(view, "beginCustomizeButton");
        var selectedItemName = ReflectionUtil.GetPrivateField<TextMeshProUGUI>(view, "selectedItemName");
        if (beginCustomizeButton == null || selectedItemName == null)
        {
            ModLog.Warn("[TotemOfUndying] Failed to bind workbench UI references.");
            return;
        }

        if (_panelRoot != null)
        {
            Destroy(_panelRoot);
        }

        var parent = view.transform as RectTransform;
        if (parent == null)
        {
            ModLog.Warn("[TotemOfUndying] Failed to resolve workbench panel parent.");
            return;
        }

        _panelRoot = new GameObject("TotemOfUndying_WorkbenchPanel", typeof(RectTransform), typeof(Image));
        var panelRect = (RectTransform)_panelRoot.transform;
        panelRect.SetParent(parent, false);
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(620f, 190f);
        panelRect.anchoredPosition = new Vector2(0f, 36f);
        panelRect.SetAsLastSibling();

        _panelBackground = _panelRoot.GetComponent<Image>();
        if (_panelBackground != null)
        {
            _panelBackground.color = new Color(0f, 0f, 0f, 0.78f);
            _panelBackground.raycastTarget = false;
        }

        _titleText = Instantiate(selectedItemName, panelRect);
        _titleText.name = "TotemOfUndying_WorkbenchTitle";
        var titleRect = (RectTransform)_titleText.transform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -16f);
        titleRect.sizeDelta = new Vector2(560f, 36f);
        _titleText.alignment = TextAlignmentOptions.Center;
        _titleText.text = "不死图腾";

        _recipeText = Instantiate(selectedItemName, panelRect);
        _recipeText.name = "TotemOfUndying_WorkbenchRecipe";
        var recipeRect = (RectTransform)_recipeText.transform;
        recipeRect.anchorMin = new Vector2(0.5f, 1f);
        recipeRect.anchorMax = new Vector2(0.5f, 1f);
        recipeRect.pivot = new Vector2(0.5f, 1f);
        recipeRect.anchoredPosition = new Vector2(0f, -58f);
        recipeRect.sizeDelta = new Vector2(560f, 92f);
        _recipeText.alignment = TextAlignmentOptions.Center;
        _recipeText.enableWordWrapping = true;
        _recipeText.fontSize = Mathf.Max(24f, _recipeText.fontSize - 8f);
        _recipeText.text = "10个羽毛  150个蓝色方块  3个狗牌  2个顶级有机纤维";

        _craftButton = Instantiate(beginCustomizeButton, panelRect);
        _craftButton.name = "TotemOfUndying_WorkbenchCraftButton";
        var buttonRect = (RectTransform)_craftButton.transform;
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 18f);
        buttonRect.sizeDelta = new Vector2(240f, buttonRect.sizeDelta.y);
        _craftButton.onClick.RemoveAllListeners();
        _craftButton.onClick.AddListener(OnCraftButtonClicked);

        _craftButtonText = _craftButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        if (_craftButtonText != null)
        {
            _craftButtonText.text = "制作不死图腾";
        }

        ModLog.Info("[TotemOfUndying] Custom workbench panel created.");
    }

    private void RefreshPanelState()
    {
        if (_panelRoot == null || _boundView == null)
        {
            return;
        }

        var isVisible = _boundView.open && View.ActiveView == _boundView;
        _panelRoot.SetActive(isVisible);
        if (!isVisible)
        {
            return;
        }

        if (_recipeText != null && string.IsNullOrWhiteSpace(_recipeText.text))
        {
            ModLog.Info("[TotemOfUndying] Custom workbench panel is visible.");
        }

        if (!ModBehaviour.TryBuildTotemCraftCost(out var cost))
        {
            if (_recipeText != null)
            {
                _recipeText.text = "无法解析制作材料，请检查日志。";
            }

            if (_craftButton != null)
            {
                _craftButton.interactable = false;
            }

            return;
        }

        if (_recipeText != null)
        {
            _recipeText.text = "10个羽毛  150个蓝色方块  3个狗牌  2个顶级有机纤维";
        }

        if (_craftButton != null)
        {
            _craftButton.interactable = cost.Enough;
        }
    }

    private void OnCraftButtonClicked()
    {
        CraftTotem();
    }

    private void CraftTotem()
    {
        if (!ModBehaviour.TryBuildTotemCraftCost(out var cost))
        {
            NotificationText.Push("不死图腾材料解析失败");
            return;
        }

        if (!cost.Enough)
        {
            NotificationText.Push("不死图腾材料不足");
            return;
        }

        if (!cost.Pay())
        {
            NotificationText.Push("不死图腾制作失败");
            return;
        }

        var item = ItemAssetsCollection.InstantiateSync(ModBehaviour.TotemOfUndyingTypeId);
        if (item == null)
        {
            NotificationText.Push("不死图腾生成失败");
            return;
        }

        ItemUtilities.SendToPlayer(item, dontMerge: false, sendToStorage: true);
        NotificationText.Push("已制作：不死图腾");
        RefreshPanelState();
    }
}