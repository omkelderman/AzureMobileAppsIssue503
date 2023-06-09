using Client;
using Microsoft.Datasync.Client;

var client = new DatasyncClient("http://localhost:8085");

// im too lazy to setup offline table stuff, so lets just use remote table
// I can still demonstrate the issue with the remote table by doing the exact same request with this remote table that the offline table would do on a PullItemsAsync() call
var remoteTodoTable = client.GetRemoteTable<TodoItem>();

switch (args.FirstOrDefault())
{
    default:
        Console.WriteLine("run with one of the following command line arguments:");
        Console.WriteLine(" * fill: create 5 new todo items and push them to the server");
        Console.WriteLine(" * fetch: fetch todo items from the server");
        Console.WriteLine(" * fix-index: call the special created fix-index endpoint on the server");
        return;
    case "fill":
        await RunFill();
        return;
    case "fetch":
        await RunFetch();
        return;
    case "fix-index":
        await RunFixIndex();
        return;
}

async Task RunFill()
{
    Console.WriteLine("Lets first push 5 todo items to the server so we have something in its database");
    for (var i = 0; i < 5; i++)
    {
        await remoteTodoTable.InsertItemAsync(new TodoItem
        {
            Title = $"Todo Item #{i}"
        });
    }
}

async Task RunFetch()
{
    Console.WriteLine("Lets try to fetch a list of todo items, ordered by UpdatedAt, which is what the offline table PullItemsAsync would do");
    Console.WriteLine("If we haven't run the fix-index task yet, this will appear to hang forever.");
    Console.WriteLine("If you look at the server app console, you'll see it spams requests and cosmos errors. The only way to stop it is to kill this process.");
    var result = await remoteTodoTable.CreateQuery().IncludeTotalCount().OrderBy(x => x.UpdatedAt).ToListAsync();
    Console.WriteLine($"Remote Todo items found: [{string.Join(',', result.Select(x => x.Title))}]");
}

async Task RunFixIndex()
{
    using var httpClient = new HttpClient();
    Console.WriteLine("Executing fix-index request");
    var resp = await httpClient.PostAsync(new Uri(client.Endpoint, "/fix-index"), null);
    Console.WriteLine($"request result: {resp.StatusCode}");
}
