using Cursovaya.Models;
using Cursovaya.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Services;

public class AppLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AppLog>> GetRecentAsync(int count = 200)
    {
        return await _unitOfWork.AppLogs
            .Query()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
