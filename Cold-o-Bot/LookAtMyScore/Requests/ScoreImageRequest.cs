using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ColdOBot.Net;
using ColdOBot.Osu;
using Newtonsoft.Json;

namespace ColdOBot.LookAtMyScore.Requests
{
    public class ScoreImageRequest : ApiRequest
    {
        protected override string BaseUri => "https://lookatmysco.re/api/";
        protected override string Target => "submit";
        protected override RequestMethod RequestMethod => RequestMethod.Post;

        public ScoreImageRequest(string username, int beatmapID, Mode mode)
        {
            Parameters.Add("username", username);
            Parameters.Add("beatmap_id", "" + beatmapID);
            Parameters.Add("mode", "" + (int)mode);
        }

        public Image Image;

        public MultiScore Scores;

        public new async Task<ImageResult> Perform()
        {
            ImageResult r;
            string s = await base.Perform();
            if (new Regex("\\Gresult").Match(s, 2).Success)
            {
                if (new Regex("\\Gimage").Match(s, 11).Success)
                {
                    Image = JsonConvert.DeserializeObject<Image>(s);
                    r = ImageResult.Image;
                }
                else if (new Regex("\\Gmultiple-scores").Match(s, 11).Success)
                {
                    Scores = JsonConvert.DeserializeObject<MultiScore>(s);
                    r = ImageResult.MultipleScores;
                }
                else
                    r = ImageResult.Error;
            }
            else
                r = ImageResult.Error;
            return r;
        }
    }

    public enum ImageResult
    {
        /// <summary>
        /// An image to the score was generated
        /// </summary>
        Image,
        /// <summary>
        /// There are multiple scores for the same beatmap and user (different mods)
        /// </summary>
        MultipleScores,
        /// <summary>
        /// Uhm... something was wrong
        /// </summary>
        Error
    }
}