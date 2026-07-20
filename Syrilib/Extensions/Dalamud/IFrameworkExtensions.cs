// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace Syrilib.Extensions.Dalamud;

// ReSharper disable once InconsistentNaming
public static class IFrameworkExtensions
{
    extension(IFramework framework)
    {
        public Task RunSafely(Action runAction)
            => framework.IsFrameworkUnloading ? Task.CompletedTask : framework.Run(runAction);
    }
}