using System;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syrilib.Extensions.Dalamud;

namespace Syrilib;

public class SyrilibHost
{
    public void Create()
    {
        throw new NotImplementedException();
    }

    public IHostBuilder InitializeHost(IDalamudPluginInterface pluginInterface)
    {
        if (SyrilibMain._isInitialized)
            throw new Exception("Use either SyrilibMain.Initialize or SyrilibHost.InitializeHost");
        
        SyrilibMain.Initialize(pluginInterface);

        var host = new HostBuilder()
            .UseContentRoot(pluginInterface.ConfigDirectory.FullName)
            .ConfigureServices(c =>
            {
                c.AddSingleton(pluginInterface)
                    .AddSingleton(IAddonEventManager.Get())
                    .AddSingleton(IAddonLifecycle.Get())
                    .AddSingleton(IAetheryteList.Get())
                    .AddSingleton(IAgentLifecycle.Get())
                    .AddSingleton(IBuddyList.Get())
                    .AddSingleton(IChatGui.Get())
                    .AddSingleton(IClientState.Get())
                    .AddSingleton(ICommandManager.Get())
                    .AddSingleton(ICondition.Get())
                    .AddSingleton(IContextMenu.Get())
                    .AddSingleton(IDataManager.Get())
                    .AddSingleton(IDtrBar.Get())
                    .AddSingleton(IDutyState.Get())
                    .AddSingleton(IFateTable.Get())
                    .AddSingleton(IFlyTextGui.Get())
                    .AddSingleton(IFramework.Get())
                    .AddSingleton(IGameConfig.Get())
                    .AddSingleton(IGameGui.Get())
                    .AddSingleton(IGameInteropProvider.Get())
                    .AddSingleton(IGameInventory.Get())
                    .AddSingleton(IGameLifecycle.Get())
                    .AddSingleton(IGamepadState.Get())
                    .AddSingleton(IJobGauges.Get())
                    .AddSingleton(IKeyState.Get())
                    .AddSingleton(IMarketBoard.Get())
                    .AddSingleton(INamePlateGui.Get())
                    .AddSingleton(INotificationManager.Get())
                    .AddSingleton(IObjectTable.Get())
                    .AddSingleton(IPartyFinderGui.Get())
                    .AddSingleton(IPlayerState.Get())
                    .AddSingleton(IReliableFileStorage.Get())
                    .AddSingleton(ISelfTestRegistry.Get())
                    .AddSingleton(ISeStringEvaluator.Get())
                    .AddSingleton(ISigScanner.Get())
                    .AddSingleton(ITargetManager.Get())
                    .AddSingleton(ITextureProvider.Get())
                    .AddSingleton(ITextureReadbackProvider.Get())
                    .AddSingleton(ITextureSubstitutionProvider.Get())
                    .AddSingleton(ITitleScreenMenu.Get())
                    .AddSingleton(IToastGui.Get())
                    .AddSingleton(IUnlockState.Get())
                    .AddSingleton(pluginInterface.UiBuilder);
            });

        return host;
    }
}