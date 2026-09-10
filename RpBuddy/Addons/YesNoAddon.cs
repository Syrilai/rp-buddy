using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;
using Syrilib.Extensions.Dalamud;

namespace RpBuddy.Addons;

public record YesNoAddonConfig
{
    public required ReadOnlySeString PromptText;
    public required Action OnConfirm;
    public bool CheckboxConfirm = false;
    public bool HoldButton = false;
    
}

public unsafe class YesNoAddon : NativeAddon
{
    private const float BaseHeight = 96f;
    private const float CheckboxHeight = 29f;
    
    private ReadOnlySeString _promptText = string.Empty;
    private bool _checkboxConfirm = false;
    private bool _holdButton = false;
    private Action? _action = null;

    // I assume it's safe to assume that it's not null
    private WindowNode BaseWindow => (WindowNode)WindowNode!;
    
    private TextNode _promptTextNode = null!;
    private ResNode _buttonContainer = null!;
    private TextButtonNode _confirmButtonNode = null!;
    private HoldButtonNode _confirmHoldButtonNode = null!;
    private TextButtonNode _cancelButtonNode = null!;
    
    protected override void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);

        var screenSize = ImGui.GetMainViewport().Size;
        var windowSize = new Vector2(400f, BaseHeight);
        if (_checkboxConfirm) windowSize.Y += CheckboxHeight;
        var windowPos = new Vector2((screenSize.X - windowSize.X) / 2, (screenSize.Y - windowSize.Y) / 2);
        
        BaseWindow.DividingLineNode.IsVisible = false;
        BaseWindow.TitleNode.IsVisible = false;
        
        SetWindowSize(windowSize);
        SetWindowPosition(windowPos);

        _promptTextNode = new TextNode
        {
            Position = new Vector2(28, 22),
            Size = new Vector2(344, 96),
            AlignmentType = AlignmentType.TopLeft,
            TextFlags = TextFlags.WordWrap | TextFlags.MultiLine,
            FontSize = 14,
            String = _promptText,
        };
        _promptTextNode.AttachNode(RootNode);

        _buttonContainer = new ResNode
        {
            Position = new Vector2(0, windowSize.Y - 46f),
            Size = windowSize with { Y = 28f }
        };
        _buttonContainer.AttachNode(RootNode);

        const float totalButtons = 2;
        const float buttonSpacing = 12f;
        const float buttonWidth = 100f;
        
        const float totalWidth = totalButtons * buttonWidth + (totalButtons - 1f) * buttonSpacing;
        var startX = (windowSize.X - totalWidth) / 2f;
        
        if (_holdButton)
        {
            // We do nothing, lol
            _confirmHoldButtonNode = new HoldButtonNode
            {
                Position = new Vector2(startX, -5f),
                Size = new Vector2(buttonWidth, 28f),
                String = "Yes",
            };
            _confirmHoldButtonNode.OnClick += () =>
            {
                _action?.Invoke();
                Close();
            };
            _confirmHoldButtonNode.AttachNode(_buttonContainer);
        }
        else
        {
            _confirmButtonNode = new TextButtonNode
            {   
                // We center the buttons, according to totalButtons and buttonSpacing. This goes into slot 1
                Position = new Vector2(startX, 0f),
                Size = new Vector2(buttonWidth, 28f),
                String = "Yes"
            };
            _confirmButtonNode.OnClick += () =>
            {
                _action?.Invoke();
                Close();
            };
            _confirmButtonNode.AttachNode(_buttonContainer);
        }
        _cancelButtonNode = new TextButtonNode
        {
            // This goes into slot 2
            Position = new Vector2(startX + buttonWidth + buttonSpacing, 0f),
            Size = new Vector2(buttonWidth, 28f),
            String = "No",
        };
        _cancelButtonNode.AttachNode(_buttonContainer);
        _cancelButtonNode.OnClick += Close;
    }

    public void QueueSelect(YesNoAddonConfig config)
    {
        _promptText = config.PromptText;
        _checkboxConfirm = config.CheckboxConfirm;
        _holdButton = config.HoldButton;
        _action = config.OnConfirm;
        Open();
    }
}