using System;
using Dalamud.Plugin;

namespace Syrilib;

public class SyrilibMain
{
    internal static bool _isInitialized = false;
    internal static IDalamudPluginInterface _pluginInterface = null!;
    
    public static void Initialize(IDalamudPluginInterface pluginInterface)
    {
        if (_isInitialized)
            throw new Exception("Already initialized");
        
        _isInitialized = true;
        _pluginInterface = pluginInterface;
    }

    public static void Dispose()
    {
        if (!_isInitialized)
            return;
    }
}