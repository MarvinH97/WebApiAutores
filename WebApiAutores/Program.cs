using System.IdentityModel.Tokens.Jwt;
using WebApiAutores;

var builder = WebApplication.CreateBuilder(args);

/** Hace que no se mapeen los type de los claims */
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
/** *********************************************************/

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

var servicioLogger = (ILogger<Startup>)app.Services.GetService(typeof(ILogger<Startup>));

startup.Configure(app, app.Environment, servicioLogger);

app.Run();
