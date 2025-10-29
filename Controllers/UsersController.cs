using DMServer.db;
using DMServer.Utils;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly Database _db;
    public UsersController(Database db) => _db = db;

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (_db.Users.Any(u => u.Login == request.Login))
            return BadRequest(new { success = false, message = "Login already exists" });

        var user = new User
        {
            Login = request.Login,
            Email = request.Email, // сохраняем как есть
            Password = SHA.SHA512Hash(request.Password), // хешируем только пароль
            DiscordData = request.DiscordId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return Ok(new { success = true, account_id = user.AccountId, message = "User registered successfully" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Login == request.Login);
        if (user == null)
            return BadRequest(new { success = false, message = "Invalid login or password" });

        // проверяем хеш пароля
        if (user.Password != SHA.SHA512Hash(request.Password))
            return BadRequest(new { success = false, message = "Invalid login or password" });

        // добавляем сессию
        var session = new Session
        {
            AccountId = user.AccountId,
            UserId = user.AccountId,
            LoginDate = DateTime.UtcNow
        };
        _db.Sessions.Add(session);

        // обновляем дату последнего онлайн
        user.LastOnline = DateTime.UtcNow;

        _db.SaveChanges();

        return Ok(new
        {
            success = true,
            account_id = user.AccountId,
            session_id = session.SessionId,
            message = "Login successful"
        });
    }
}

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly Database _db;
    public SubscriptionsController(Database db) => _db = db;

    [HttpPost("add")]
    public IActionResult Add([FromBody] Subscription sub)
    {
        sub.PurchaseDate = DateTime.UtcNow;
        _db.Subscriptions.Add(sub);
        _db.SaveChanges();
        return Ok(new { success = true, subscription_id = sub.SubscriptionId, message = "Subscription added" });
    }
}

public class RegisterRequest
{
    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? DiscordId { get; set; }
}

public class LoginRequest
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}
