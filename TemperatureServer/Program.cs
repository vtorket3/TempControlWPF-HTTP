using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TemperatureStorage>();
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<TemperatureHub>("/temperatureHub");

app.Run();

public class TemperatureData
{
    public double Temperature { get; set; }
    public DateTime Timestamp { get; set; }
}

public class TemperatureStorage
{
    private readonly Queue<TemperatureData> _data = new();

    public void Add(TemperatureData temp)
    {
        if (_data.Count >= 10)
            _data.Dequeue();

        _data.Enqueue(temp);
    }

    public IEnumerable<TemperatureData> GetAll() => _data;
}

public class TemperatureHub : Hub { }


[ApiController]
[Route("api/temperature")]
public class TemperatureController : ControllerBase
{
    private readonly TemperatureStorage _storage;
    private readonly IHubContext<TemperatureHub> _hub;

    public TemperatureController(TemperatureStorage storage, IHubContext<TemperatureHub> hub)
    {
        _storage = storage;
        _hub = hub;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TemperatureData data)
    {
        _storage.Add(data);
        await _hub.Clients.All.SendAsync("ReceiveData", _storage.GetAll());
        return Ok();
    }
}
