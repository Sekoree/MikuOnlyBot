// See https://aka.ms/new-console-template for more information
using DSharpPlus;

Console.WriteLine("Hello, World!");
var channel = ulong.Parse(args[0]);
var client = new DiscordClient(new DiscordConfiguration()
{
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
    Token = args[1],
    TokenType = TokenType.Bot,
});

client.MessageCreated += async (sender, args) =>
{
    if (args.Message.Content.ToLower() != "miku" && args.Message.ChannelId == channel) 
        await args.Message.DeleteAsync();
};

client.Ready += (sender, args) =>
{
    Console.WriteLine("Ready");
    return Task.CompletedTask;
};

await client.ConnectAsync();
Console.WriteLine("Connected");
Console.ReadLine();