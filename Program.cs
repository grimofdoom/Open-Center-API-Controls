using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environments.Staging,
    WebRootPath = "wwwroot"
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(builder => {
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions() {
    OnPrepareResponse = context => {
        context.Context.Response.Headers.Add("cache-Control", "No-cache, no-store");
        context.Context.Response.Headers.Add("Expires", "-1");
    }
});
app.UseRouting();

#region Setup Packages
CPM.Core core = new();

List<PackageDetails> installedPackageDetails = new List<PackageDetails>();
int packageCount = 0;
int commandCount = 0;

//Build up a premade cache of all package/command details
foreach(CPM.Package package in core.packages) {
    packageCount++;
    PackageDetails pd = new() {
        name = package.name,
        callName = package.callName,
        description = package.description,
        author = package.author,
        commands = new List<CommandDetails>()
    };

    foreach (CPM.Command cmd in package.commands) {
        commandCount++;
        pd.commands.Add(new() {
            name = cmd.name,
            callName = cmd.callName,
            description = cmd.description,
            expectedValues = cmd.expectedValues,
            availableResources = cmd.availableResources
        });
    }

    installedPackageDetails.Add(pd);
}

Console.WriteLine($"[{packageCount}] packages and [{commandCount}] commands installed");

#endregion





#region map addresses
//This should be the core page where web browsers can control the server
app.MapGet("/", () => { 
    return Results.Content(File.ReadAllText("Public/Main.html"), "Text/HTML");
});

//This is where all commands will be sent, using JSON to lay out how the commands should be performed
app.MapPost("/Command", (CPM.RecievedCommand? command) => {
    Console.WriteLine("Performing some command");

    //Double check that nothing went bad
    if (command == null) {
        Console.WriteLine(command);
        return Results.BadRequest();
    }

    //Call the command, collect any data to send back to client
    object cmdResults = core.Perform(command);

    //Process results from command
    if (cmdResults == null) {
        //Either creator f'd up or there is an unknown problem with command
        return Results.Problem("Unknown error from command: " + command.command);
    } else if (cmdResults.GetType() == typeof(CPM.OK)) {
        //Command ran ok, and nothing needs to be sent back
        return Results.Ok();
    } else {
        //Command ran ok, and something needs to be sent back
        return cmdResults;
    }
});

//This is where alternative frontends can get all the data in one giant package from commands to image url's
//Frontend should cache data if possible, reduce local server calls
app.MapGet("/CommandData", () =>  installedPackageDetails);

//This is where frontends can send data back to control the server
app.MapPost("/ServerSettings", () => "Unset Server Settings/Control");
#endregion


//Setup local IP\
string localIP = LocalIPAddress();

app.Urls.Add("http://" + localIP + ":5072");
app.Urls.Add("https://" + localIP + ":7072");

app.Run();


static string LocalIPAddress() {
    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
        socket.Connect("8.8.8.8", 65530);
        IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
        if (endPoint != null) {
            return endPoint.Address.ToString();
        } else {
            return "127.0.0.1";
        }
    }
}


#region extra records for sending package details to client
record PackageDetails {
    public string? name { get; set; }
    public string? callName { get; set; }
    public string? description { get; set; }
    public string? author { get; set; }
    public List<CommandDetails>? commands { get; set; }
}

record CommandDetails {
    public string? name { get; set; }
    public string? callName { get; set; }
    public string? description { get; set; }
    public List<CPM.ExpectedValues>? expectedValues { get; set; }
    public List<CPM.AvailableResources>? availableResources { get; set; }
}
#endregion