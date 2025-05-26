using PagamentoService.Services;

public class Worker : BackgroundService
{
    private readonly PagamentoConsumer _consumer;

    public Worker(PagamentoConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Start();
        return Task.CompletedTask;
    }
}