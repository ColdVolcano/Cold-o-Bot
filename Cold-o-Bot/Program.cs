using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using System.Net.Sockets;
using System.IO;
using System.Linq;

namespace ColdOBot
{
    class Program
    {
        private static byte[] data;

        private static Dictionary<string, string> keys = new Dictionary<string, string>();
        private static string prefix = "!!";
        
        private static DiscordClient discord;

        static private string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ColdOBot");
        static private string configPath = Path.Combine(path, "settings.txt");

        static void Main(string[] args)
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

            discord = new DiscordClient(new DiscordConfig
            {
                AutoReconnect = true,
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = keys["token"],
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false
            });
            Run(keys["oauth"], keys["nick"], keys["channel"]).GetAwaiter().GetResult();
        }

        static private void InitializeConfig()
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
                switch (e.Level)
                {
                    case LogLevel.Debug:
                        Console.BackgroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        break;
                    case LogLevel.Warning:
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        break;
                    case LogLevel.Error:
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        break;
                    case LogLevel.Critical:
                        Console.BackgroundColor = ConsoleColor.Red;
                        break;
                }
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"[{e.Level}]");
                Console.ResetColor();
                Console.WriteLine($" [{e.Timestamp}] [{e.Application}] {e.Message}");
            };

            discord.GuildAvailable += e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, "Cold-o-Bot", $"Guild available: {e.Guild.Name}", DateTime.Now);
                return Task.Delay(0);
            };

            discord.MessageCreated += e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Debug, "Cold-o-Bot", e.Message.Author.Username + ": " + e.Message.Content, DateTime.Now);
                #region PING COMMAND
                if (e.Message.Content.StartsWith($"{prefix}ping"))
                {
                    e.Message.RespondAsync(e.Message.CreationDate.LocalDateTime.Subtract(DateTime.Now).ToString("ss's'ffffff'u'"));
                }
                #endregion
                else if (e.Message.Content.StartsWith($"{prefix}twownk"))
                {
                    e.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(discord, 320774047267029002));
                }
                else if (e.Message.Content.StartsWith($"{prefix}stats"))
                {

                }
                else if (e.Message.Content.StartsWith($"{prefix}deleteMessages") && e.Message.Author.Id == 120196252775350273)
                {
                    string[] split = e.Message.Content.Split(' ');
                    int count = 0;
                    if (split.Length == 2 && int.TryParse(split[1], out count) && count > 0 && count < 5)
                    {
                        e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":put_litter_in_its_place:").ToString());
                        e.Channel.DeleteMessagesAsync(e.Channel.GetMessagesAsync(count, e.Message.Id).Result);
                    }
                    else
                        e.Message.RespondAsync("Parameters do not match");
                }
                else if (e.Message.Content.StartsWith($"{prefix}profile") && e.Message.Author.Id == 120196252775350273)
                {
                    string[] split = e.Message.Content.Split(' ');
                    if (split.Length >= 3)
                        switch (split[1])
                        {
                            case "status":
                                UserStatus status;
                                bool canParse = Enum.TryParse(split[2], out status);
                                if (canParse)
                                {
                                    discord.UpdateStatusAsync(user_status: status);
                                    e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":ok:").ToString() + " Succesfully updated online status");
                                }
                                else
                                    e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":octagonal_sign:").ToString() + " Could not parse values");
                                break;
                            case "game":
                                discord.UpdateStatusAsync(new Game(string.Join(" ", split.Skip(2).ToArray())));
                                e.Message.RespondAsync(DiscordEmoji.FromName(discord, ":ok:").ToString() + " Succesfully updated online status");
                                break;
                            default:
                                break;
                        }
                    else
                        e.Message.RespondAsync("Not enough arguments");
                }
                #region ROLL COMMAND
                else if (e.Message.Content.StartsWith($"{prefix}roll"))
                {
                    string[] split = e.Message.Content.Split(' ');
                    uint maxValue = 100;
                    if (split.Length >= 2)
                        switch (split[1].Contains("d"))
                        {
                            case true:
                                uint dices = 0;
                                string[] dicesAndMaxes = split[1].Split('d');
                                if (uint.TryParse(dicesAndMaxes[0], out dices) && uint.TryParse(dicesAndMaxes[1], out maxValue))
                                {
                                    if (dices == 1)
                                        break;
                                    if (dices <= 10)
                                    {
                                        if (maxValue == 0)
                                            maxValue = 6;
                                        string response = $"<@{e.Message.Author.Id}> rolled {dices} dices with {maxValue} sides:\n";
                                        int diceResult;
                                        long totalResult = 0;
                                        Random ran = new Random();
                                        for (int i = 0; i < dices; i++)
                                        {
                                            diceResult = ran.Next(1, (int)maxValue);
                                            totalResult += diceResult;
                                            response += $"Dice {i + 1} got a {diceResult}\n";
                                        }
                                        response += $"A total of {totalResult} points!";
                                        e.Message.RespondAsync(response);
                                    }
                                    else
                                        e.Message.RespondAsync($"{e.Author.Mention}: You can only roll up to 10 dices");
                                }
                                else
                                    break;
                                return Task.Delay(0);
                            case false:
                                if (!uint.TryParse(split[1], out maxValue) || maxValue == 0)
                                    maxValue = 100;
                                break;
                        }
                    e.Message.RespondAsync($"<@{e.Message.Author.Id}> rolled {new Random().Next(1, (int)maxValue)}");
                }
                #endregion
                return Task.Delay(0);
            };

            discord.Ready += e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, "Cold-o-Bot", $"Cold-o-Bot is now running!", DateTime.Now);
                return Task.Delay(0);
            };

            await discord.ConnectAsync();

            RunTwitchBot(twitchOauth, twitchNick, twitchTargetChannel);

        }

        public static async Task RunTwitchBot(string oauth, string nick, string channel)
        {
            TcpClient client = new TcpClient("irc.twitch.tv", 6667);

            NetworkStream stream = client.GetStream();

            string loginstring = $"PASS oauth:{oauth}\r\nNICK {nick}\r\n";
            byte[] login = Encoding.ASCII.GetBytes(loginstring);
            stream.Write(login, 0, login.Length);
            discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", "Sent login", DateTime.Now);
            discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", loginstring, DateTime.Now);

            data = new byte[512];

            string responseData = string.Empty;

            int bytes = stream.Read(data, 0, data.Length);
            responseData = Encoding.ASCII.GetString(data, 0, bytes);
            discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", $"Received WELCOME: \r\n{responseData}", DateTime.Now);

            string joinstring = "JOIN " + "#" + channel + "\r\n";
            byte[] join = Encoding.ASCII.GetBytes(joinstring);
            stream.Write(join, 0, join.Length);
            discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", $"Sent channel join.\r\n{joinstring}", DateTime.Now);

            discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", "TWITCH CHAT HAS BEGUN. BE CAREFUL", DateTime.Now);

            while (true)
            {
                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;
                do
                {
                    try { numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length); }
                    catch (Exception e) { discord.DebugLogger.LogMessage(LogLevel.Critical, "Twitch-o-Bot", $"OH SHIT SOMETHING WENT WRONG {e}", DateTime.Now); }

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                } while (stream.DataAvailable);
                discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", myCompleteMessage.ToString(), DateTime.Now);

                switch (myCompleteMessage.ToString())
                {
                    case "PING :tmi.twitch.tv\r\n":
                        try
                        {
                            byte[] say = Encoding.ASCII.GetBytes("PONG :tmi.twitch.tv\r\n");
                            stream.Write(say, 0, say.Length);
                            discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", "Received PING, sent PONG", DateTime.Now);
                        }
                        catch (Exception e) { discord.DebugLogger.LogMessage(LogLevel.Critical, "Twitch-o-Bot", $"OH SHIT SOMETHING WENT WRONG {e}", DateTime.Now); }
                        break;
                    default:
                        string[] message;
                        string[] preamble;
                        try
                        {
                            string messageParser = myCompleteMessage.ToString();
                            message = messageParser.Split(':');
                            preamble = message[1].Split(' ');
                            string tochat;
                            string[] sendingUser = preamble[0].Split('!');

                            if (preamble[1] == "PRIVMSG")
                            {
                                tochat = sendingUser[0] + ": " + message[2];

                                if (!tochat.Contains("\n"))
                                {
                                    tochat = tochat + "\n";
                                }

                                if (sendingUser[0] != "moobot" && sendingUser[0] != "whale_bot")
                                {
                                    discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", tochat.TrimEnd('\n'), DateTime.Now);
                                }
                            }
                            else if (preamble[1] == "JOIN")
                            {
                                tochat = "JOINED: " + sendingUser[0];
                                discord.DebugLogger.LogMessage(LogLevel.Info, "Twitch-o-Bot", tochat.TrimEnd('\n'), DateTime.Now);
                            }
                        }
                        catch (Exception e) { discord.DebugLogger.LogMessage(LogLevel.Critical, "Twitch-o-Bot", $"OH SHIT SOMETHING WENT WRONG {e}", DateTime.Now); }
                        break;
                }
            }
        }
    }

    public static class OsuApi
    {
        private static Uri BaseUri = new Uri("https://osu.ppy.sh/api/");
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
        keyMod = Key4 | Key5 | Key6 | Key7 | Key8,
        FadeIn = 1048576,
        Random = 2097152,
        LastMod = 4194304,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | keyMod,
        Key9 = 16777216,
        Key10 = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456
    }
}