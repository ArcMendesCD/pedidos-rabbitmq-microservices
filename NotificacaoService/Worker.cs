using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificacaoService.Services;

namespace NotificacaoService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly NotificacaoConsumer _consumer;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _consumer = new NotificacaoConsumer();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Start();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}