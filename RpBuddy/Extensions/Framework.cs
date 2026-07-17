using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace RpBuddy.Extensions;

public static class FrameworkExtensions
{
    extension(IFramework framework)
    {
        public Task RunSafely(Action runAction)
            => framework.IsFrameworkUnloading ? Task.CompletedTask : framework.Run(runAction);
    }
}