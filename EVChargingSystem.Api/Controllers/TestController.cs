using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EVChargingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IMongoDatabase _database;

    public TestController(IMongoDatabase database)
    {
        _database = database;
    }

    [HttpGet("mongodb-status")]
    public async Task<IActionResult> GetMongoDbStatus()
    {
        try
        {
            // Try to ping the database
            await _database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
            return Ok(new
            {
                status = "Connected",
                database = _database.DatabaseNamespace.DatabaseName,
                message = "MongoDB is connected successfully!"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "Disconnected",
                message = $"MongoDB connection failed: {ex.Message}"
            });
        }
    }
}