using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Remipedia.Modules
{
    public class ML : ModuleBase
    {
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

            await LaunchMlCommand("dream", new[] { url },
                "darknet", $"nightmare cfg/vgg-conv.cfg vgg-conv.weights [INPATH 0] {layer} -iters {iters} -range {range}",
                $"_vgg-conv_{layer}_000000.jpg");
        }

        [Command("Dream", RunMode = RunMode.Async), Priority(1)]
        public async Task DreamAsync(int layer = 10, int iters = 10, int range = 1)
        {
            await DreamAsync(GetAttachmentImage(1).ElementAt(0), layer, iters, range);
        }

        [Command("Edge", RunMode = RunMode.Async)]
        public async Task EdgeAsync(string url, double percentile = 99.7)
        {
            if (percentile >= 100 || percentile <= 0)
            {
                await ReplyAsync("Invalid range argument (must be between 0 and 100)");
                return;
            }
            await LaunchMlCommand("edge", new[] { url }, "python", $"sobel.py -I [INPATH 0] -p {percentile}", ".jpg");
        }

        [Command("Edge", RunMode = RunMode.Async), Priority(1)]
        public async Task EdgeAsync(double percentile = 99.7)
        {
            await EdgeAsync(GetAttachmentImage(1).ElementAt(0), percentile);
        }

        [Command("Transfer", RunMode = RunMode.Async)]
        public async Task TransferAsync(string url1, string url2)
        {
            await LaunchMlCommand("transfer", new[] { url1, url2 }, "python", "styletransfer.py -I [INPATH 0] -J [INPATH 1]", ".jpg");
        }

        [Command("Transfer", RunMode = RunMode.Async), Priority(1)]
        public async Task TransferAsync()
        {
            var images = GetAttachmentImage(2);
            await TransferAsync(images.ElementAt(0), images.ElementAt(1));
        }

        [Command("PCA", RunMode = RunMode.Async)]
        public async Task PCAAsync(string url, ColorString color = null)
        {
            if (color == null)
            {
                color = ColorString.Default;
            }
            await LaunchMlCommand("pca", new[] { url }, "python", $"pca.py -I [INPATH 0] -c {color}", ".jpg");
        }

        [Command("PCA", RunMode = RunMode.Async), Priority(1)]
        public async Task PCAAsync(ColorString color = null)
        {
            await PCAAsync(GetAttachmentImage(1).ElementAt(0), color);
        }

        /// <summary>
        /// Start a machine learning process
        /// </summary>
        /// <param name="discordCmdName">Name of the current Discord command</param>
        /// <param name="url">URL to the image to treat</param>
        /// <param name="command">Command name</param>
        /// <param name="arguments">Command arguments</param>
        /// <param name="outpathEnd">End of the path for the output file, will be append to the current generated file name</param>
        private async Task LaunchMlCommand(string discordCmdName, string[] url, string command, string arguments, string outpathEnd)
        {
            // In and out paths the image will have
            string[] inPaths = new string[url.Length];
            string[] outPaths = new string[url.Length];
            var tmpPath = DateTime.Now.ToString("HHmmssff") + Context.User.Id + StaticObjects.Random.Next(0, 99999) + "_" + discordCmdName.ToLowerInvariant();

            for (int i = 0; i < url.Length; i++)
            {

                // Check if URL is a valid image
                var extension = Path.GetExtension(url[i]).Split('?')[0];
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
                {
                    throw new ArgumentException("Invalid file type " + extension);
                }

                inPaths[i] = $"Inputs/{tmpPath}_{i}{extension}";
                outPaths[i] = tmpPath + "_" + i + outpathEnd;

                // Replace [INPATH] string in the command by the actual path
                arguments = arguments.Replace($"[INPATH {i}]", inPaths[i]);

                // Download the image
                var bytes = await StaticObjects.HttpClient.GetByteArrayAsync(url[i]);
                if (bytes.Length > 8_000_000)
                {
                    throw new ArgumentException("Your image must be less than 8MB");
                }
                File.WriteAllBytes(inPaths[i], bytes);
            }
            try
            {
                var msg = await ReplyAsync("Your image is processed, this can take up to a few minutes");
                Console.WriteLine(new LogMessage(LogSeverity.Info, discordCmdName, command + " " + arguments));
                Process.Start(command, arguments).WaitForExit();

                // When we are done, we delete the waiting message and post the modified image
                foreach (var oPath in outPaths)
                {
                    await Context.Channel.SendFileAsync(oPath);
                }
                await msg.DeleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                DeleteFiles(inPaths);
                DeleteFiles(outPaths);
            }
        }

        public void DeleteFiles(string[] path)
        {
            foreach (var p in path)
            {
                File.Delete(p);
            }
        }

        /// <summary>
        /// Get the file in attachment or throw an error if there are none
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetAttachmentImage(int count)
        {
            if (Context.Message.Attachments.Count < count)
            {
                throw new ArgumentException($"You must provide at least {count} image");
            }
            return Enumerable.Range(0, count)
                .Select(x => Context.Message.Attachments.ElementAt(x).Url);
        }
    }
}
