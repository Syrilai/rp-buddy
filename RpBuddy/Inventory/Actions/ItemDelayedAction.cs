using System.Threading.Tasks;

namespace RpBuddy.Inventory.Actions;

public class ItemDelayedAction(int delay, ItemActionBase[] actionsToExecute) : ItemActionBase
{
    public readonly int Delay = delay;
    public readonly ItemActionBase[] ActionsToExecute = actionsToExecute;
    
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