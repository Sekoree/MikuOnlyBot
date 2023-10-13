// See https://aka.ms/new-console-template for more information

using DSharpPlus;
using DSharpPlus.Entities;

Console.WriteLine("Hello, World!");
var channel = ulong.Parse(args[0]);
var client = new DiscordClient(new DiscordConfiguration()
{
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMembers,
    Token = args[1],
    TokenType = TokenType.Bot,
});

DiscordEmoji? emoji = default;

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

client.GuildAvailable += (sender, args) =>
{
    Console.WriteLine($"Guild available: {args.Guild.Name}");
    _ = args.Guild.Emojis.TryGetValue(532326560967950337, out emoji);
    if (args.Guild.Id == 483279257431441410)
        _ = Task.Run(() => HandlePastReactions(args.Guild, emoji!));
    return Task.CompletedTask;
};

client.MessageReactionAdded += async (sender, args) =>
{
    if (emoji is null)
    {
        Console.WriteLine("Emoji is null");
        return;
    }

    if (args.Channel.Id == 483282509035995156 // rule channel
        && args.Emoji.Id == 532326560967950337 // plush emoji
        && args.Message.Id == 1159957838358122506) // rule event react message
    {
        var guild = args.Guild;
        var role = guild.GetRole(1159956236733780049);
        var member = await guild.GetMemberAsync(args.User.Id);
        await member.GrantRoleAsync(role);
        Console.WriteLine($"Granted {member.DisplayName} the role");
    }
    else if (args.Channel.Id == 483290389047017482 // announcement channel
             && args.Emoji.Id == 532326560967950337 // plush emoji
             && args.Message.Id == 1159960482342506517) // announcement event react message
    {
        var guild = args.Guild;
        var role = guild.GetRole(1159956236733780049);
        var member = await guild.GetMemberAsync(args.User.Id);
        await member.GrantRoleAsync(role);
        Console.WriteLine($"Granted {member.DisplayName} the role");
    }
};

client.MessageReactionRemoved += async (sender, args) =>
{
    if (emoji is null)
    {
        Console.WriteLine("Emoji is null");
        return;
    }

    if (args.Channel.Id == 483282509035995156 // rule channel 
        && args.Emoji.Id == 532326560967950337 // plush emoji
        && args.Message.Id == 1159957838358122506) // rule event react message
    {
        var guild = args.Guild;
        var role = guild.GetRole(1159956236733780049);
        var otherChannel = guild.GetChannel(483290389047017482);
        var otherMsg = await otherChannel.GetMessageAsync(1159960482342506517);
        var otherMsgReacts = await otherMsg.GetReactionsAsync(emoji!, 100);
        if (otherMsgReacts.Any(u => u.Id == args.User.Id))
            return;
        var member = await guild.GetMemberAsync(args.User.Id);
        await member.RevokeRoleAsync(role);
        Console.WriteLine($"Revoked {member.DisplayName} the role");
    }
    else if (args.Channel.Id == 483290389047017482 // announcement channel 
             && args.Emoji.Id == 532326560967950337 // plush emoji
             && args.Message.Id == 1159960482342506517) // announcement event react message
    {
        var guild = args.Guild;
        var role = guild.GetRole(1159956236733780049);
        var otherChannel = guild.GetChannel(483282509035995156);
        var otherMsg = await otherChannel.GetMessageAsync(1159957838358122506);
        var otherMsgReacts = await otherMsg.GetReactionsAsync(emoji!, 100);
        if (otherMsgReacts.Any(u => u.Id == args.User.Id))
            return;
        var member = await guild.GetMemberAsync(args.User.Id);
        await member.RevokeRoleAsync(role);
        Console.WriteLine($"Revoked {member.DisplayName} the role");
    }
};

await client.ConnectAsync();
Console.WriteLine("Connected");
Console.ReadLine();

static async Task HandlePastReactions(DiscordGuild guild, DiscordEmoji emoji)
{
    try
    {
        Console.WriteLine("Handling past reactions in guild: " + guild.Name);
        var ruleChannel = guild.GetChannel(483282509035995156);
        Console.WriteLine("Rule channel: " + ruleChannel.Name);
        var announcementChannel = guild.GetChannel(483290389047017482);
        Console.WriteLine("Announcement channel: " + announcementChannel.Name);
        var ruleMsg = await ruleChannel.GetMessageAsync(1159957838358122506);
        var ruleMsgReacts = await ruleMsg.GetReactionsAsync(emoji, 100);
        Console.WriteLine("Rule message reactions: " + ruleMsgReacts.Count);
        var announcementMsg = await announcementChannel.GetMessageAsync(1159960482342506517);
        var announcementMsgReacts = await announcementMsg.GetReactionsAsync(emoji, 100);
        Console.WriteLine("Announcement message reactions: " + announcementMsgReacts.Count);

        var reactedUsers = new List<DiscordUser>();
        foreach (var discordUser in ruleMsgReacts)
            if (discordUser is not null)
                reactedUsers.Add(discordUser);

        foreach (var discordUser in announcementMsgReacts)
            if (discordUser is not null && !reactedUsers.Contains(discordUser))
                reactedUsers.Add(discordUser);

        Console.WriteLine("Total reacted users: " + reactedUsers.Count);

        var role = guild.GetRole(1159956236733780049);
        var members = await guild.GetAllMembersAsync();
        var usersWithRole = new List<DiscordMember>();
        foreach (var member in members)
            if (member.Roles.Any(r => r.Id == role.Id))
                usersWithRole.Add(member);

        Console.WriteLine("Total users with role: " + usersWithRole.Count);

        var usersMissingRole = reactedUsers.Where(u => !usersWithRole.Contains(u));
        foreach (var user in usersMissingRole)
        {
            var member = members.First(m => m.Id == user.Id);
            await member.GrantRoleAsync(role);
            Console.WriteLine($"Granted {member.DisplayName} the role");
        }

        var usersWithRoleMissingReaction = usersWithRole.Where(u => !reactedUsers.Contains(u));
        foreach (var user in usersWithRoleMissingReaction)
        {
            var member = members.First(m => m.Id == user.Id);
            await member.RevokeRoleAsync(role);
            Console.WriteLine($"Revoked {member.DisplayName} the role");
        }

        Console.WriteLine("Done");
    }
    catch (Exception e)
    {
        Console.WriteLine("Error: ");
        Console.WriteLine(e);
    }
}