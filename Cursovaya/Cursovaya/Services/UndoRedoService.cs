namespace Cursovaya.Services;

public interface IUndoableAction
{
    string Name { get; }
    Task UndoAsync();
    Task RedoAsync();
}

public class UndoRedoService
{
    private readonly Stack<IUndoableAction> _undoStack = new();
    private readonly Stack<IUndoableAction> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public event Action? StateChanged;

    public void AddAction(IUndoableAction action)
    {
        _undoStack.Push(action);
        _redoStack.Clear();
        StateChanged?.Invoke();
    }

    public async Task UndoAsync()
    {
        if (!CanUndo)
        {
            return;
        }

        var action = _undoStack.Pop();
        await action.UndoAsync();
        _redoStack.Push(action);
        StateChanged?.Invoke();
    }

    public async Task RedoAsync()
    {
        if (!CanRedo)
        {
            return;
        }

        var action = _redoStack.Pop();
        await action.RedoAsync();
        _undoStack.Push(action);
        StateChanged?.Invoke();
    }
}
