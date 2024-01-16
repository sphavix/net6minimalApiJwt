using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using net6minimalApiJwt.Models;
using net6minimalApiJwt.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = " Authorization",
        Description = "Bearer Authentication with JWT token",
        Type = SecuritySchemeType.Http
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

//Add JWT Authentication Service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IMovieService, MovieService>();

var app = builder.Build();

app.UseSwagger();
app.UseAuthorization();
app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Hello World").ExcludeFromDescription();

app.MapPost("/login", (UserLogin user, IUserService service) => Login(user, service))
    .Accepts<UserLogin>("application/json")
    .Produces<string>();

app.MapPost("/create",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    (Movie movie, IMovieService service) => Create(movie, service))
    .Accepts<Movie>("application/json")
    .Produces<Movie>(statusCode: 200, contentType: "application/json");

app.MapGet("/get",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Standard, Administrator")]
    (int id, IMovieService service) => Get(id, service))
    .Produces<Movie>();

app.MapGet("/list", (IMovieService service) => List(service))
    .Produces<List<Movie>>(statusCode: 200, contentType: "application/json");

app.MapPut("/update",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    (Movie movie, IMovieService service) => Update(movie, service))
    .Accepts<Movie>("application/json")
    .Produces<Movie>(statusCode: 200, contentType: "application/json");

app.MapDelete("/delete",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    (int id, IMovieService service) => Delete(id, service));


//Methods implementation
IResult Login(UserLogin user, IUserService service)
{
    if (!String.IsNullOrEmpty(user.Username) && !String.IsNullOrEmpty(user.Password))
    {
        var loggedInUser = service.Get(user);
        if (loggedInUser is null) return Results.NotFound("Invalid user credentials");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, loggedInUser.Username),
            new Claim(ClaimTypes.Email, loggedInUser.EmailAddress),
            new Claim(ClaimTypes.GivenName, loggedInUser.FirstName),
            new Claim(ClaimTypes.Surname, loggedInUser.Surname),
            new Claim(ClaimTypes.Role, loggedInUser.Role)
        };

        var token = new JwtSecurityToken
        (
            issuer: builder.Configuration["Jwt:Issuer"],
            audience: builder.Configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            notBefore: DateTime.UtcNow,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                SecurityAlgorithms.HmacSha256)
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(tokenString);
    }
    return Results.BadRequest("Invalid user credentials");

}
IResult Create(Movie movie, IMovieService service)
{
    var result = service.Create(movie);

    return Results.Ok(result);
}

IResult Get(int id, IMovieService service)
{
    var movie = service.GetMovieById(id);

    if (movie is null) return Results.NotFound("Movie Not Found");

    return Results.Ok(movie);
}

IResult List(IMovieService service)
{
    var result = service.List();

    return Results.Ok(result);
}

IResult Update(Movie movie, IMovieService service)
{
    var result = service.Update(movie);

    if (result is null) return Results.NotFound("Movie Not Found");

    return Results.Ok(result);
}

IResult Delete(int id, IMovieService service)
{
    var result = service.Delete(id);

    if (!result) Results.BadRequest("Error, something went wrong!");

    return Results.Ok(result);
}


app.UseHttpsRedirection();
app.UseSwaggerUI();

app.Run();
