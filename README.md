# pixeldrain-dotnet
A simple C# api wrapper for pixeldrain.

### Usage
Copy the .cs file to your project, then do something like this:

Create a new instance of pixeldrain client:
```cs
var pd = new Pixeldrain.PixeldrainClient();
```
You can also use usings because why not:
```cs
using (var pd = new Pixeldrain.PixeldrainClient()) { /*do the dirty*/ }
```

To upload a file and get it's ID:
```cs
var task = pd.UploadFile(@"C:\Path\Thing.scr");
task.Wait();
Console.WriteLine(task.Result); // out the id
```

To download a file and either get it's byte array and save it to disk (if you specify the path)
```cs
var task2 = pd.DownloadFile(task.Result, @"C:\Path\Thing2.jpeg");
task2.Wait();
Console.WriteLine("SUCCESSFUL SUCCESS!!!");
```

### Bonus
You can attack an API key, but it doesn't do anything for now, if I will ever update this repo...
```cs
var pdkey = new Pixeldrain.PixeldrainClient("apikeyreal");
```

You can also attach a logger from `Microsoft.Extensions.Logging` and it will actually work, but would you really need a logger?
```cs
var pd = new Pixeldrain.PixeldrainClient(new ILogger() /*whatever*/, "apikey if necessary");
```

Also you can always remove these things because the file is really small. And simple.
