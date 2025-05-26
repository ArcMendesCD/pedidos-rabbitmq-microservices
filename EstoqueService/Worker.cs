using EstoqueService.Services;

public class Worker : BackgroundService
{
    private readonly EstoqueConsumer _consumer;

    public Worker(EstoqueConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Start();
        return Task.CompletedTask;
    }
}