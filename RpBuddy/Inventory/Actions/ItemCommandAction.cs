using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Syrilib.Extensions.Dalamud;

namespace RpBuddy.Inventory.Actions;

[method: JsonConstructor]
public class ItemCommandAction(string command, string arguments = "") : IItemActionBase
{
    public string  Command { get; set; } = command;
    public string Arguments { get; set; } = arguments;

    public void Execute()
    {
        var fullCommand = $"/{Command}{(!string.IsNullOrEmpty(Arguments) ? $" {Arguments}" : "")}";

        if (!fullCommand.StartsWith('/'))
        {
            IPluginLog.Get().Warning($"Tried executing a command \"{fullCommand}\" but it did not pass safety checks.");
            return;
        }

        IFramework.Get().RunSafely(() => IChatGui.Get().ExecuteCommand(fullCommand));
    }
}