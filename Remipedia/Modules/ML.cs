using Discord.Commands;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Remipedia.Modules
{
    public class ML : ModuleBase
    {
        [Command("Dream")]
        public async Task DreamAsync()
        {
            if (Context.Message.Attachments.Count == 0)
            {
                await ReplyAsync("You need to provide an image");
                return;
            }
            var fi = new FileInfo(Context.Message.Attachments.ElementAt(0).Url);
            if (fi.Extension != ".png" && fi.Extension != ".jpg" && fi.Extension != ".jpeg")
            {
                await ReplyAsync("Invalid file type");
                return;
            }
        }
    }
}
