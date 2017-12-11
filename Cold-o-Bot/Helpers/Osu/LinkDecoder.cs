using System.Text.RegularExpressions;

namespace ColdOBot.Helpers.Osu
{
    public class LinkDecoder
    {
        public static bool IsValidBeatmapForImage(string url, out BeatmapLinkType type)
        {
            if (new Regex(
                "^[\\d]*[\\d]$|^[\\d]*s$|https:\\/\\/osu.ppy.sh\\/(s\\/[\\d][\\d]*$|b(eatmapsets\\/[\\d][\\d]*(#(osu|fruits|taiko|mania)\\/[\\d][\\d]*)?$|\\/[\\d][\\d]*((\\?|&)m=[0123])?$))",
                RegexOptions.IgnoreCase).IsMatch(url))
            {
                type = new BeatmapLinkType
                {
                    IsOldSite = new Regex(
                        "https:\\/\\/osu.ppy.sh\\/(s\\/[\\d][\\d]*$|b\\/[\\d][\\d]*((\\?| &)m=[0123])?$)",
                        RegexOptions.IgnoreCase).IsMatch(url),
                    IsNewSite = new Regex(
                        "https:\\/\\/osu.ppy.sh\\/beatmapsets\\/[\\d][\\d]*(#(osu|fruits|taiko|mania)\\/[\\d][\\d]*)?$",
                        RegexOptions.IgnoreCase).IsMatch(url),
                    IsBeatmapset =
                        new Regex("^[\\d]*s$|https:\\/\\/osu.ppy.sh\\/(s\\/[\\d][\\d]*$|beatmapsets\\/[\\d][\\d]*$)",
                            RegexOptions.IgnoreCase).IsMatch(url)
                };
                return true;
            }
            type = new BeatmapLinkType();
            return false;
        }
    }

    public struct BeatmapLinkType
    {
        public bool IsBeatmapset;

        public bool IsOldSite;

        public bool IsNewSite;
    }
}