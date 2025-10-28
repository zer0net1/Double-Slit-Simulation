using DoubleSlitSimulation.Computing;
using Microsoft.AspNetCore.Mvc;

namespace DoubleSlitSimulation.Controllers;

[ApiController]
[Route("[controller]")]
public class ErrorHandler : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromBody] SimulationParameters parameters)
    {
        var simulator = new DoubleSlitSimulator(parameters);
        var result = simulator.RunError();
        
        return Ok(result);
    }
}