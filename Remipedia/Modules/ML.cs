using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
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

            var layer = 16;

            var tmpPath = DateTime.Now.ToString("HHmmssff") + Context.User.Id;
            var inPath = tmpPath + extension;
            var outPath = tmpPath + $"_vgg-conv_{layer}_000000.jpg";
            File.WriteAllBytes(inPath, await StaticObjects.HttpClient.GetByteArrayAsync(url));

            var args = $"nightmare cfg/vgg-conv.cfg vgg-conv.weights {inPath} {layer} -iters 4";
            Console.WriteLine(new LogMessage(LogSeverity.Info, "Dream", "darnet " + args));
            Process.Start("darknet", args).WaitForExit();

            await Context.Channel.SendFileAsync(outPath);

            File.Delete(inPath);
            File.Delete(outPath);
        }
    }
}
