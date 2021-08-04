using Discord.Commands;
using System;
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
            var url = Context.Message.Attachments.ElementAt(0).Url;
            var extension = Path.GetExtension(url);
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            {
                await ReplyAsync("Invalid file type");
                return;
            }

            var path = DateTime.Now.ToString("HHmmssff") + Context.User.Id + extension;
            File.WriteAllBytes(path, await StaticObjects.HttpClient.GetByteArrayAsync(url));

            await Context.Channel.SendFileAsync(path);

            File.Delete(path);
        }
    }
}
