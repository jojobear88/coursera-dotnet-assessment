using UserManagementAPI.Models;
using UserManagementAPI.Repositories;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register repository (in-memory for now)
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

// Register custom logging middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// ✅ GET: Retrieve all users
app.MapGet("/users", (IUserRepository repo) =>
{
    return Results.Ok(repo.GetAll());
});

// ✅ GET: Retrieve a specific user by ID
app.MapGet("/users/{id}", (int id, IUserRepository repo) =>
{
    var user = repo.GetById(id);
    return user is not null ? Results.Ok(user) : Results.NotFound($"User with ID {id} not found.");
});

// ✅ POST: Add a new user
app.MapPost("/users", (User user, IUserRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(user.FirstName) ||
        string.IsNullOrWhiteSpace(user.LastName) ||
        string.IsNullOrWhiteSpace(user.Email))
    {
        return Results.BadRequest("FirstName, LastName, and Email are required.");
    }

    repo.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

// ✅ PUT: Update an existing user
app.MapPut("/users/{id}", (int id, User updatedUser, IUserRepository repo) =>
{
    var existing = repo.GetById(id);
    if (existing is null) return Results.NotFound();

    updatedUser.Id = id;
    repo.Update(updatedUser);
    return Results.Ok(updatedUser);
});

// ✅ DELETE: Remove a user by ID
app.MapDelete("/users/{id}", (int id, IUserRepository repo) =>
{
    var user = repo.GetById(id);
    if (user is null) return Results.NotFound();

    repo.Delete(id);
    return Results.NoContent();
});

app.Run();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (errorFeature != null)
        {
            var errorResponse = new
            {
                Message = "An unexpected error occurred.",
                Detail = errorFeature.Error.Message
            };
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    });
});