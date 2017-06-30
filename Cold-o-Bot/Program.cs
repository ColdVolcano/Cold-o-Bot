using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using System.Net.Sockets;

namespace ColdOBot
{
    class Program
    {
        private static byte[] data;

        private static DiscordClient discord;

        static void Main(string[] args)
        {
            Console.WriteLine("Discord token pls");
            string token = Console.ReadLine();
            discord = new DiscordClient(new DiscordConfig
            {
                AutoReconnect = true,
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false
            });
            Console.WriteLine("Twitch oauth");
            string oauth = Console.ReadLine();
            Console.WriteLine("Twitch nick");
            string nick = Console.ReadLine();
            Console.WriteLine("Twitch channel to connect:");
            string channel = Console.ReadLine();
            Run(oauth, nick, channel).GetAwaiter().GetResult();
        }

        public static async Task Run(string twitchOauth, string twitchNick, string twitchTargetChannel)
        {
            discord.DebugLogger.LogMessageReceived += (o, e) =>
            {
                switch (e.Level)
                {
                    case LogLevel.Unnecessary:
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        break;
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
                discord.DebugLogger.LogMessage(LogLevel.Debug, "Cold-o-Bot", e.Message.Content, DateTime.Now);
                #region PING COMMAND
                if (e.Message.Content.StartsWith($"!ping"))
                {
                    e.Message.RespondAsync(e.Message.CreationDate.LocalDateTime.Subtract(DateTime.Now).ToString("ss'.'ffffff"));
                }
                #endregion
                else if(e.Message.Content.StartsWith($"!twownk"))
                {
                    e.Message.CreateReactionAsync("twownking:320774047267029002");
                }
                #region ROLL COMMAND
                else if (e.Message.Content.StartsWith($"!roll"))
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
                                    e.Message.RespondAsync("Could not parse values");
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
}