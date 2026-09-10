using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RpBuddy.Utils;

namespace RpBuddy.Inventory.Actions;

[method: JsonConstructor]
public class ItemDelayedAction(int delay, 
                               [JsonProperty(ItemConverterType = typeof(ItemActionConverter))]
                               List<IItemActionBase> actionsToExecute) : IItemActionBase
{
    public int Delay { get; set; } = delay;
    [JsonProperty(ItemConverterType = typeof(ItemActionConverter))]
    public List<IItemActionBase> ActionsToExecute { get; set; } = actionsToExecute;
    
    public void Execute()
    {
        Task.Run(async delegate
        {
            await Task.Delay(Delay);
            foreach (var actionToExecute in ActionsToExecute)
                actionToExecute.Execute();
        });
    }
}