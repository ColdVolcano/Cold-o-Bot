using DSharpPlus;

namespace ColdOBot
{
    public static class DiscordParser
    {
        public static bool ParseDiscordCommand(DiscordMessage message, out DiscordTextResponse response)
        {
            response = new DiscordTextResponse();
            return true;
        }

        public static bool ParseDiscordCommand(DiscordMessage message, out DiscordReactionResponse response)
        {
            response = new DiscordReactionResponse();
            return true;
        }
    }

    public struct DiscordTextResponse
    {
        public DiscordEmbed Embed;
        public bool TTS;
        public string Content;
    }

    public struct DiscordReactionResponse
    {
        public string Emoji;
    }

    public enum DiscordResponseType
    {
        Message,
        Reaction,
    }
}
