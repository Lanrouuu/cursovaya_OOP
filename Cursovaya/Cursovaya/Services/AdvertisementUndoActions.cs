using Cursovaya.Models;

namespace Cursovaya.Services;

public class AddedAdvertisementAction : IUndoableAction
{
    private readonly AdvertisementService _advertisementService;
    private readonly int _advertisementId;

    public AddedAdvertisementAction(AdvertisementService advertisementService, int advertisementId)
    {
        _advertisementService = advertisementService;
        _advertisementId = advertisementId;
    }

    public string Name => "Добавление объявления";

    public async Task UndoAsync()
    {
        await _advertisementService.SetStatusInternalAsync(_advertisementId, AdvertisementStatus.Deleted);
    }

    public async Task RedoAsync()
    {
        await _advertisementService.SetStatusInternalAsync(_advertisementId, AdvertisementStatus.Active);
    }
}

public class DeletedAdvertisementAction : IUndoableAction
{
    private readonly AdvertisementService _advertisementService;
    private readonly Advertisement _oldSnapshot;

    public DeletedAdvertisementAction(AdvertisementService advertisementService, Advertisement oldSnapshot)
    {
        _advertisementService = advertisementService;
        _oldSnapshot = oldSnapshot;
    }

    public string Name => "Удаление объявления";

    public async Task UndoAsync()
    {
        await _advertisementService.RestoreSnapshotInternalAsync(_oldSnapshot);
    }

    public async Task RedoAsync()
    {
        await _advertisementService.SetStatusInternalAsync(_oldSnapshot.Id, AdvertisementStatus.Deleted);
    }
}

public class EditedAdvertisementAction : IUndoableAction
{
    private readonly AdvertisementService _advertisementService;
    private readonly Advertisement _oldSnapshot;
    private readonly Advertisement _newSnapshot;

    public EditedAdvertisementAction(
        AdvertisementService advertisementService,
        Advertisement oldSnapshot,
        Advertisement newSnapshot)
    {
        _advertisementService = advertisementService;
        _oldSnapshot = oldSnapshot;
        _newSnapshot = newSnapshot;
    }

    public string Name => "Редактирование объявления";

    public async Task UndoAsync()
    {
        await _advertisementService.RestoreSnapshotInternalAsync(_oldSnapshot);
    }

    public async Task RedoAsync()
    {
        await _advertisementService.RestoreSnapshotInternalAsync(_newSnapshot);
    }
}
