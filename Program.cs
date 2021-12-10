var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();





//This should be the core page where web browsers can control the server
app.MapGet("/", () => "Welcome to OCAC; Open Center API Controls");

//This will be a web based system where the user can explore in more detail how the system works
//Pretty much another view of all the commands and what they can do & how they work
app.MapGet("/Explorer", () => "Unset System Explorer");

//This is where all commands will be sent, using JSON to lay out how the commands should be performed
app.MapPost("/Command", () => "Unset Commands System");

//This is where alternative frontends can get all the data in one giant package from commands to image url's
//Frontend should cache data if possible, reduce local server calls
app.MapGet("/InitFrontend", () => "Unset Frontend Initialize");

//This is where frontends can send data back to control the server
app.MapPost("/ServerSettings", () => "Unset Server Settings/Control");





app.Run();
