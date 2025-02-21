using Petricords.API.Workers;
using Microsoft.EntityFrameworkCore;
using slp.light.Interfaces;

public class DbUserCleaner : BackgroundService
{
    private const String improperUsernamePrefix = "ak-outpost";
    private readonly ILogger<DbUserCleaner> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DbUserCleaner(IServiceProvider serviceProvider, ILogger<DbUserCleaner> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                var usersToRemove = await dbContext.Users
                    .Where(user => user.Username.StartsWith(improperUsernamePrefix)).ToListAsync(stoppingToken);
                
                if (usersToRemove.Any())
                {
                     dbContext.Users.RemoveRange(usersToRemove);
                    await dbContext.SaveChanges(stoppingToken);
                    _logger.LogInformation($"Removed {usersToRemove.Count} invalid users.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while removing invalid users.");
        }
    }
}