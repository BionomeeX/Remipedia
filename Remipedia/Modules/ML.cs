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
        [Command("Help")]
        public async Task HelpAsync() {
            await ReplyAsync(embed: new EmbedBuilder{
                Color = new Color(0x067FBD),
                Title = "Help",
                Description = "**Dream [layer] [iters] [range]**:\nDream use VGG-16 as a backbone so only 18 layers are usable.\n\t[layer] optional (1 - 19)(default to 10) choose the layer you want to use for gradient maximization. Low level (near 0) will show basics changes (colors, straight lines, etc...), whereas high level (near 16) will show more animal structures.\n\t[iters] optional (1 - 100)(default to 10). Number of iterations to apply to the image, the more iterations, th more feverish the DREAM will be!\n\t[range] optional (1 - 19-layer)(default to 1). Number of layers after the selected layer (with layer argument) that will be choosen at random for each iteration. Large number will makes the DREAM more diverse in the range of possibles transformations."
            }.Build());
        }

        [Command("Dream", RunMode = RunMode.Async)]
        public async Task DreamAsync(string url, int layer = 10, int iters = 10, int range = 1)
        {
            if (layer > 18 || layer < 1)
            {
                await ReplyAsync("Invalid layer argument (must be between 1 and 18)");
                return;
            }

            if (iters > 100 || iters < 0)
            {
                await ReplyAsync("Invalid iters argument (must be between 0 and 100)");
                return;
            }

            if (range > 19 - layer || range < 0)
            {
                await ReplyAsync("Invalid range argument (must be between 1 and 19 - layer)");
                return;
            }

            var extension = Path.GetExtension(url).Split('?')[0];
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            {
                await ReplyAsync("Invalid file type " + extension);
                return;
            }


            var tmpPath = DateTime.Now.ToString("HHmmssff") + Context.User.Id;
            var inPath = "Inputs/" + tmpPath + extension;
            var outPath = tmpPath + $"_vgg-conv_{layer}_000000.jpg";
            File.WriteAllBytes(inPath, await StaticObjects.HttpClient.GetByteArrayAsync(url));

            var msg = await ReplyAsync("Your image is processed, this can take up to a few minutes");

            var args = $"nightmare cfg/vgg-conv.cfg vgg-conv.weights {inPath} {layer} -iters {iters} - range {range}";
            Console.WriteLine(new LogMessage(LogSeverity.Info, "Dream", "darknet " + args));
            Process.Start("darknet", args).WaitForExit();

            await Context.Channel.SendFileAsync(outPath);
            await msg.DeleteAsync();

            File.Delete(inPath);
            File.Delete(outPath);
        }

        [Command("Dream", RunMode = RunMode.Async), Priority(1)]
        public async Task DreamAsync(int layer = 10, int iters = 10, int range = 1)
        {
            if (Context.Message.Attachments.Count == 0)
            {
                await ReplyAsync("You need to provide an image");
                return;
            }
            var url = Context.Message.Attachments.ElementAt(0).Url;
            await DreamAsync(url, layer, iters, range);
        }
    }
}
