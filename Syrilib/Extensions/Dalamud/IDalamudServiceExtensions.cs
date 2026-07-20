using System;
using Dalamud.Plugin.Services;

namespace Syrilib.Extensions.Dalamud;

// ReSharper disable once InconsistentNaming
public static class IDalamudServiceExtensions
{
    private static class Holder<T> where T : class, IDalamudService
    {
        public static T? Instance
            => field ??= SyrilibMain._pluginInterface.GetService(typeof(T)) as T;
    }

    extension<T>(T) where T : class, IDalamudService
    {
        public static T Get()
            => Holder<T>.Instance ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found.");
    }
}