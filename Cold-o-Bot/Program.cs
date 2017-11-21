﻿using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColdOBot
{
    class Program
    {
        private static readonly Dictionary<string, string> keys = new Dictionary<string, string>();
        private static readonly string prefix = Environment.CurrentDirectory.StartsWith("D:\\") ? "??" : "!!";

        private static DiscordClient discord;

        public static DateTimeOffset TimeStarted;

        private static readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ColdOBot");
        private static readonly string configPath = Path.Combine(path, "settings.txt");

        private static void Main()
        {
            bool needsRecheck = true;
            do
            {
                if (!File.Exists(configPath))
                {
                    InitializeConfig();
                    continue;
                }
                string line;
                StreamReader reader = new StreamReader(configPath);
                while ((line = reader.ReadLine()) != null)
                {
                    string[] split = line.Split('=');
                    keys[split[0].Trim(' ')] = split[1].Trim();
                }
                reader.Close();
                if (!(keys.ContainsKey("token") && keys.ContainsKey("oauth") && keys.ContainsKey("nick") && keys.ContainsKey("channel") && keys.ContainsKey("osuKey")))
                {
                    InitializeConfig();
                    continue;
                }
                if (!keys.ContainsValue(null) && !keys.ContainsValue(""))
                {
                    needsRecheck = false;
                    continue;
                }
                bool hasDoneThat = false;
                foreach (var key in keys)
                {
                    if (string.IsNullOrWhiteSpace(key.Value) && !hasDoneThat)
                    {
                        InitializeConfig();
                        hasDoneThat = true;
                    }
                }
            } while (needsRecheck);

            OsuApi.Key = keys["osuKey"];

            discord = new DiscordClient(new DiscordConfiguration
            {
                AutoReconnect = true,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = keys["token"],
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false
            });
            Run(keys["oauth"], keys["nick"], keys["channel"]).GetAwaiter().GetResult();
        }

        private static void InitializeConfig()
        {

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string[] lines = new string[5];
            Console.WriteLine("Discord token pls");
            lines[0] = "token = " + Console.ReadLine();
            Console.WriteLine("Twitch oauth");
            lines[1] = "oauth = " + Console.ReadLine();
            Console.WriteLine("Twitch nick");
            lines[2] = "nick = " + Console.ReadLine();
            Console.WriteLine("Twitch channel to connect:");
            lines[3] = "channel = " + Console.ReadLine();
            Console.WriteLine("osu!API key");
            lines[4] = "osuKey = " + Console.ReadLine();
            File.WriteAllLines(configPath, lines);
        }

        public static async Task Run(string twitchOauth, string twitchNick, string twitchTargetChannel)
        {
            discord.DebugLogger.LogMessageReceived += (o, e) =>
            {
                bool needsFileLogging = false;
                switch (e.Level)
                {
                    case LogLevel.Debug:
                        Console.BackgroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        break;
                    case LogLevel.Warning:
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        needsFileLogging = true;
                        if (e.Level == LogLevel.Warning)
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                        if (e.Level == LogLevel.Error)
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                        if (e.Level == LogLevel.Critical)
                            Console.BackgroundColor = ConsoleColor.Red;
                        break;
                }
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"[{e.Level}]");
                Console.ResetColor();
                Console.WriteLine($" [{e.Timestamp}] [{e.Application}] {e.Message}");
                if (needsFileLogging)
                {
                    File.AppendAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChangeWatcher", "log.txt"), new[] { $"[{e.Level}] [{e.Timestamp}] [{e.Application}] {e.Message}" });
                    //Environment.Exit(-1);
                }
            };

            discord.GuildAvailable += e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, "Cold-o-Bot", $"Guild available: {e.Guild.Name}", DateTime.Now);
                return Task.Delay(0);
            };

            discord.MessageCreated += async e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Debug, "Cold-o-Bot", e.Message.Author.Username + ": " + e.Message.Content, DateTime.Now);
                #region PING COMMAND
                if (e.Message.Content.StartsWith($"{prefix}ping"))
                {
                    DiscordMessage message = null;
                    await Task.Run(async () => message = await e.Message.RespondAsync("pong!"));
                    await message.ModifyAsync($"{message.Content} `{e.Message.CreationTimestamp - message.CreationTimestamp:ss\'s\'ffffff\'u\'}`");
                }
                #endregion
                else if (e.Message.Content.StartsWith($"{prefix}about"))
                {
                    double ramUsage;
                    using (var proc = Process.GetCurrentProcess())
                        ramUsage = proc.PrivateMemorySize64 / 1024d / 1024;
                    var time = (DateTime.Now - TimeStarted);
                    var builder = new DiscordEmbedBuilder()
                        .AddField("RAM usage:", ramUsage.ToString("00.0000"), true)
                        .AddField("Active since",
                            $"{string.Join(" ", TimeStarted.ToUniversalTime().ToString().Split(new[] { ' ' }, 5).TakeWhile(s => s[0] != '-' && s[0] != '+'))} (it's been " +
                            (time.Days > 0 ? time.Days + "d" : "") + (time.Hours > 0 ? time.Hours + "h" : "") +
                            (time.Minutes > 0 ? time.Minutes + "m" : "") +
                            (time.Seconds + Math.Round(time.Milliseconds / 1000f) > 0
                                ? time.Seconds + Math.Round(time.Milliseconds / 1000f) + "s"
                                : "") + ")", true)
                        .AddField("Lib", $"DSharp+ v{discord.VersionString}");
                    await e.Message.RespondAsync(embed: builder);
                }
                else if (e.Message.Content.StartsWith($"{prefix}user") && !e.Channel.IsPrivate)
                {
                    string[] split = e.Message.Content.Split(' ');
                    DiscordMember member;
                    if (split.Length == 1)
                        member = await e.Guild.GetMemberAsync(e.Message.Author.Id);
                    else
                    {
                        int i = 0;
                        for (; !char.IsDigit(split[1], i); i++)
                        {
                        }
                        if (!ulong.TryParse(split[1].Substring(i, split[1].Length - (i + 1)), out var snowflake))
                            return;
                        member = await e.Guild.GetMemberAsync(snowflake);
                        if (member == null)
                            return;
                    }
                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    .AddField("ID", member.Id.ToString(), true)
                    .AddField("Status", member.Presence?.Status.ToString() ?? "Offline", true)
                    .AddField("Time created", string.Join(" ", member.CreationTimestamp.ToUniversalTime().ToString().Split(new[] { ' ' }, 5).TakeWhile(s => s[0] != '-' && s[0] != '+')), true)
                    .AddField("Time joined", string.Join(" ", member.JoinedAt.ToUniversalTime().ToString().Split(new[] { ' ' }, 5).TakeWhile(s => s[0] != '-' && s[0] != '+')), true)
                    .WithAuthor($"{member.Username}#{member.Discriminator}", icon_url: member.AvatarUrl)
                    .WithThumbnailUrl(member.AvatarUrl ?? member.DefaultAvatarUrl);
                    string roles = string.Empty;
                    var rol = member.Roles.ToArray();
                    for (int j = 0; j < rol.Length; j++)
                        roles += rol[j].Name + (j + 1 == rol.Length ? "" : ", ");
                    if (roles.Length > 0)
                        builder.AddField("Roles", roles, true);
                    await e.Message.RespondAsync(embed: builder);
                }
                else if (e.Message.Content.StartsWith($"{prefix}twownk"))
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(discord, 320774047267029002));
                }
                else if (e.Message.Content.StartsWith($"{prefix}restart") && e.Message.Author.Id == 120196252775350273)
                {
                    await e.Message.RespondAsync("Will start again in 4 seconds");
                    await discord.UpdateStatusAsync(userStatus: UserStatus.Invisible);
                    Task.Delay(2000).GetAwaiter().GetResult();
                    Task.Run(discord.DisconnectAsync).GetAwaiter().GetResult();
                    Process.GetCurrentProcess().CloseMainWindow();
                }
                else if (e.Message.Content.StartsWith($"{prefix}lenny"))
                {
                    await e.Message.RespondAsync("( ͡° ͜ʖ ͡°)");
                }
                else if (e.Message.Content.StartsWith($"{prefix}deletemessages ") && e.Message.Author.Id == 120196252775350273)
                {
                    string[] split = e.Message.Content.Split(' ');
                    if (split.Length == 2 && int.TryParse(split[1], out var count) && count > 0 && count < 5)
                    {
                        await e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":put_litter_in_its_place:").ToString());
                        await e.Channel.DeleteMessagesAsync(e.Channel.GetMessagesAsync(count, e.Message.Id).Result);
                    }
                    else
                        await e.Message.RespondAsync("Parameters do not match");
                }
                else if (e.Message.Content.StartsWith($"{prefix}serverinfo") && !e.Channel.IsPrivate)
                {
                    await e.Message.RespondAsync("",
                        embed: new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor(),
                            ThumbnailUrl = e.Guild.IconUrl,
                        }
                            .WithAuthor(e.Guild.Name, icon_url: e.Guild.IconUrl)
                            .AddField("Owner", $"{e.Guild.Owner.Username}#{e.Guild.Owner.Discriminator}", true)
                            .AddField("Members", $"{e.Guild.MemberCount}", true)
                            .AddField("Time Created", $"{string.Join(" ", e.Guild.CreationTimestamp.ToUniversalTime().ToString().Split(new[] { ' ' }, 5).TakeWhile(s => s[0] != '-' && s[0] != '+'))}", true)
                            .AddField("Roles", $"{e.Guild.Roles.Count}", true)
                            .AddField("Channels", $"{e.Guild.Channels.Count} ({e.Guild.Channels.Count(c => c.Type == ChannelType.Text)} text, {e.Guild.Channels.Count(c => c.Type == ChannelType.Voice)} voice)", true));
                }
                #region PROFILE COMMAND
                else if (e.Message.Content.StartsWith($"{prefix}profile") && e.Message.Author.Id == 120196252775350273)
                {
                    string[] split = e.Message.Content.Split(' ');
                    if (split.Length >= 3)
                        switch (split[1])
                        {
                            case "status":
                                bool canParse = Enum.TryParse(split[2], out UserStatus status);
                                if (canParse)
                                {
                                    await discord.UpdateStatusAsync(new DiscordActivity(discord.CurrentUser.Presence.Activity.Name, discord.CurrentUser.Presence.Activity.ActivityType), status);
                                    await e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":ok:").ToString() + " Succesfully updated online status");
                                }
                                else
                                    await e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":octagonal_sign:").ToString() + " Could not parse values");
                                break;
                            case "game":
                                await discord.UpdateStatusAsync(new DiscordActivity(string.Join(" ", split.Skip(2).ToArray())), discord.CurrentUser.Presence.Status);
                                await e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":ok:").ToString() + " Succesfully updated online status");
                                break;
                        }
                    else
                        await e.Message.RespondAsync("Not enough arguments");
                }
                #endregion
                #region ROLL COMMAND
                else if (e.Message.Content.StartsWith($"{prefix}roll"))
                {
                    string[] split = e.Message.Content.Split(' ');
                    uint maxValue = 100;
                    int value = 0;
                    if (split.Length >= 2)
                        switch (split[1].Contains("d"))
                        {
                            case true:
                                string[] dicesAndMaxes = split[1].Split('d');
                                if (uint.TryParse(dicesAndMaxes[0], out var dices) && ((uint.TryParse(dicesAndMaxes[1], out maxValue) || int.TryParse(dicesAndMaxes[1], out value) && value != 0)))
                                {
                                    if (dices == 1)
                                        break;
                                    if (dices == 0)
                                    {
                                        await e.Message.RespondAsync("Can't roll a dice I don't have");
                                        return;
                                    }
                                    if (dices <= 10)
                                    {
                                        if (maxValue == 0 && value == 0)
                                            maxValue = 6;
                                        var response = $"<@{e.Message.Author.Id}> rolled {dices} dices with {maxValue} sides:\n";
                                        long totalResult = 0;
                                        var ran = new Random();
                                        for (int i = 0; i < dices; i++)
                                        {
                                            var diceResult = value != 0 ? ran.Next(value, -1) : ran.Next(1, (int)maxValue);
                                            totalResult += diceResult;
                                            response += $"Dice {i + 1} got a {diceResult}\n";
                                        }
                                        response += $"A total of {totalResult} points!";
                                        await e.Message.RespondAsync(response);
                                    }
                                    else
                                        await e.Message.RespondAsync($"{e.Author.Mention}: You can only roll up to 10 dices");
                                }
                                else
                                    break;
                                return;
                            case false:
                                if ((!uint.TryParse(split[1], out maxValue) || maxValue == 0) && (!int.TryParse(split[1], out value) || value == 0))
                                    maxValue = 100;
                                break;
                        }
                    await e.Message.RespondAsync($"<@{e.Message.Author.Id}> rolled " + (value != 0 ? new Random().Next(value, 0) : new Random().Next(1, (int)maxValue)));
                }
                #endregion
            };

            discord.Ready += e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, "Cold-o-Bot", "Cold-o-Bot is now running!", DateTime.Now);
                TimeStarted = DateTimeOffset.Now.ToUniversalTime();
                return Task.Delay(1);
            };

            await discord.ConnectAsync();

            await Task.Delay(-1);
        }
    }

    public static class OsuApi
    {
        //private static Uri baseUri = new Uri("https://osu.ppy.sh/api/");
        public static string Key;
        public static dynamic GetBeatmaps(int beatmapSetID = 0, int beatmapID = 0, string user = "", OsuMode mode = OsuMode.All, int limit = 500)
        {
            dynamic yo = 5;
            return yo;
        }

        public static dynamic GetUser(string user, OsuMode mode = OsuMode.Osu, int eventDays = 1)
        {
            dynamic yo = 5;
            return yo;
        }

        public static dynamic GetScores(int beatmapID, string user = "", OsuMode mode = OsuMode.Osu, int mods = (int)Mods.None, int limit = 50)
        {
            dynamic yo = 5;
            return yo;
        }

        public static dynamic GetUserBest(string user, OsuMode mode = OsuMode.Osu, int limit = 10)
        {
            dynamic yo = 5;
            return yo;
        }

        public static dynamic GetUserRecent(string user, OsuMode mode = OsuMode.Osu, int limit = 10)
        {
            dynamic yo = 5;
            return yo;
        }

        public static dynamic GetMatch(long matchID)
        {
            dynamic yo = 5;
            return yo;
        }

        public static dynamic GetReplay(OsuMode mode, int beatmapID, string user)
        {
            dynamic yo = 5;
            return yo;
        }
    }

    public enum OsuMode
    {
        All = -1,
        Osu = 0,
        OsuTaiko = 1,
        OsuCatch = 2,
        OsuMania = 3,
        Vitaru = 4,
    }

    enum Mods
    {
        None = 0,
        NoFail = 1,
        Easy = 2,
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
        Flashlight = 1024,
        Autoplay = 2048,
        SpunOut = 4096,
        Relax2 = 8192,  // Autopilot?
        Perfect = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
        Key4 = 32768,
        Key5 = 65536,
        Key6 = 131072,
        Key7 = 262144,
        Key8 = 524288,
        KeyMod = Key4 | Key5 | Key6 | Key7 | Key8,
        FadeIn = 1048576,
        Random = 2097152,
        LastMod = 4194304,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod,
        Key9 = 16777216,
        Key10 = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456
    }
}