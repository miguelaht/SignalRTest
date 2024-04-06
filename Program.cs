using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHub<MyHub>("hub");

app.Run();

public class MyHub : Hub<IMyHubInterface>
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private static CancellationToken s_cancellationToken;
    private static bool processing;

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public async Task JoinProcess(int steps = 5, int stages = 1)
    {
        await Clients.All.JoinConfirmation(new { Success = true });
        if (processing) return;

        try
        {
            Console.WriteLine("Starting process");
            processing = true;
            s_cancellationToken = _cancellationTokenSource.Token;
            int counter = 0;
            await Clients.All.StartProcess(new { Message = "Starting", });

            while (counter++ < steps)
            {
                s_cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(TimeSpan.FromSeconds(5));
                var stage = 0;
                while (stage++ < stages)
                {
                    await Clients.All.UpdateProcessStatus(new { Step = counter, Progress = stage, Message = $"Test message {counter}", });
                    Thread.Sleep(TimeSpan.FromSeconds(4));
                }
            }

            await Clients.All.EndProcess(new { Message = "Ending" });
            processing = false;
        }
        catch (Exception)
        {
            await Clients.All.EndProcess(new { Message = "Abort" });
            processing = false;
        }
        finally
        {
            Console.WriteLine("Ending process");
        }
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine("New connection");
        if (!processing)
        {
            _ = _cancellationTokenSource.TryReset();
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Client closed connection");
        _cancellationTokenSource.Cancel();

        return base.OnDisconnectedAsync(exception);
    }

    public override string? ToString()
    {
        return base.ToString();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public interface IMyHubInterface
{
    public Task JoinConfirmation(object message);
    public Task StartProcess(object message);
    public Task UpdateProcessStatus(object message);
    public Task EndProcess(object message);
}