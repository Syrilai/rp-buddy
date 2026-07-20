using Dalamud.Plugin.Services;
using Syrilib.Extensions.Dalamud;

namespace RpBuddy.Inventory.Actions;

public class ItemCommandAction(string command, string? arguments = null) : ItemActionBase
{
    public readonly string Command = command;
    public readonly string? Arguments = arguments;
    
    public void Execute()
    {
        var fullCommand = $"/{Command}{(Arguments is not null ? $" {Arguments}" : "")}";

        if (!fullCommand.StartsWith('/'))
        {
            IPluginLog.Get().Warning($"Tried executing a command \"{fullCommand}\" but it did not pass safety checks.");
            return;
        }

        IFramework.Get().RunSafely(() => IChatGui.Get().ExecuteCommand(fullCommand));
    }
}