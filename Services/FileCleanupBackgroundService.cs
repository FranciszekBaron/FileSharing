
using FileSharing.Services.Interfaces;

public class FileCleanupBackgroundService : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FileCleanupBackgroundService> _logger;

    public FileCleanupBackgroundService(IServiceScopeFactory scopeFactory,ILogger<FileCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[BackgroundService] File Cleanup Background Service STARTED");


        //czekamy na pełen load aplikacji
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

                await fileService.CleanUpOldFilesAsync();


                //Wykonuj co 24 godziny
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
                //Spróbuj ponownie za godzinę
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("[BackgroundService] Service STOPPED");
    }
}