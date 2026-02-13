using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly UserService _userService;

    public UsersController(ILogger<UsersController> logger, UserService userService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A list of users.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = _userService.GetAllUsers();
        _logger.LogInformation("Returning {Count} users", users.Count());
        return Ok(users);
    }

    /// <summary>
    /// Retrieves a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user if found.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = _userService.GetUserById(id);

        if (user == null)
        {
            _logger.LogWarning("User with ID {Id} not found", id);
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user data.</param>
    /// <returns>The created user.</returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        var createdUser = _userService.CreateUser(user);

        _logger.LogInformation("Created user {Id} - {Name} ({Email})", createdUser.Id, createdUser.Name, createdUser.Email);

        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="updatedUser">The updated user data.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
    {
        if (!_userService.UpdateUser(id, updatedUser))
        {
            _logger.LogWarning("Update failed - user {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Updated user {Id} - new values: {Name}, {Email}, {Department}",
            id, updatedUser.Name, updatedUser.Email, updatedUser.Department);

        return NoContent();
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = _userService.GetUserById(id);
        if (user == null)
        {
            _logger.LogWarning("Delete failed - user {Id} not found", id);
            return NotFound();
        }

        _userService.DeleteUser(id);
        _logger.LogInformation("Deleted user {Id} ({Name})", id, user.Name);

        return NoContent();
    }

    /// <summary>
    /// Test endpoint to trigger an exception (for middleware testing).
    /// </summary>
    [HttpGet("test-exception")]
    public async Task<IActionResult> TestException()
    {
        throw new InvalidOperationException("Test exception for middleware.");
    }
}