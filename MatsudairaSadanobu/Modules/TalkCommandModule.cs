using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Discord;
using Discord.Audio;
using Discord.Audio.Streams;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NITNC_D1_Server.Data;

namespace MatsudairaSadanobu.Modules;

public class TalkCommandModule(IDbContextFactory<ApplicationDbContext> context, IConfiguration configuration) : DiscordBotBasic.Module(context, configuration)
{
    private HttpClient _httpClient = new HttpClient()
    {
        BaseAddress = new Uri("http://localhost:50021")
    };

    [SlashCommand("speaker", "キャラクタを選択します。")]
    public async Task SetSpeakerCommand()
    {
        var input = new SelectMenuBuilder()
            .WithPlaceholder("VOICEBOXキャラクタ").WithCustomId("selector-"+Context.User.Id);
        var json = JsonNode.Parse(await _httpClient.GetStringAsync("/speakers"))?.AsArray();
        if (json == null) return;
        foreach (var item in json)
        {
            if (input.Options.Count>=25) break;
            input.AddOption(item?["name"]?.ToString() ?? "", (item?["speaker_uuid"]?.ToString() ?? ""));
        }

        await RespondAsync(ephemeral:true,text: "あなたのメッセージを担当するキャラクタを選択してください。", components: new ComponentBuilder().WithSelectMenu(input).Build());
    }

    [ComponentInteraction("selector-*")]
    public async Task SetType(string id,string[] selectedSpeakers)
    {
        var input = new SelectMenuBuilder()
            .WithPlaceholder("声のタイプ").WithCustomId("typesection-"+Context.User.Id);
        var json = JsonNode.Parse(await _httpClient.GetStringAsync("/speakers"))?.AsArray();
        if (json == null) return;
        foreach (var item in json.Where(a=>a["speaker_uuid"].ToString()==selectedSpeakers[0])?.FirstOrDefault()?["styles"]?.AsArray())
        {
            input.AddOption(item?["name"]?.ToString() ?? "", (item?["id"]?.ToString() ?? ""));
        }

        await RespondAsync(ephemeral:true,text: "あなたのメッセージを担当するキャラクタの声のタイプを選択してください。", components: new ComponentBuilder().WithSelectMenu(input).Build());
        
    }

    [ComponentInteraction("typesection-*")]
    public async Task SelectedType(string id, string[] selectedTypes)
    {
        if (Program.Speakers.Any(x=>x.Item1==Context.User.Id))
        {
            Program.Speakers.Remove(Program.Speakers.First(x=>x.Item1==Context.User.Id));
        }
        Program.Speakers.Add((Convert.ToUInt64(id),Convert.ToInt32(selectedTypes[0])));
        await RespondAsync(ephemeral:true,text:"設定しました。");
    }

    [SlashCommand("talk", "松平定信がずんだもんの声でしゃべります．",runMode: RunMode.Async)]
    public async Task TalkCommand([Summary("content","内容")]string content,[Summary("channel","ボイスチャネル")] IVoiceChannel channel)
    {
        if (!Program.Speakers.Any(x=>x.Item1==Context.User.Id))
        {
            await RespondAsync(ephemeral:true,text:"キャラクタを選択してください。");
            return;
        }
        var speaker=Program.Speakers.First(x=>x.Item1==Context.User.Id).Item2;
        var quary=await _httpClient.PostAsync($"/audio_query?text={content}&speaker="+speaker,JsonContent.Create(""));
        var quaryData=await quary.Content.ReadAsStringAsync();
        var audio= await (await _httpClient.PostAsync("/synthesis?speaker="+speaker+"&enable_interrogative_upspeak=true",new StringContent(quaryData,Encoding.UTF8,"application/json"))).Content.ReadAsByteArrayAsync();

        var file = Path.GetTempFileName()+".wav";
        File.WriteAllBytes(file,audio);
        
        var audioClient = await channel.ConnectAsync(disconnect:false);
        while (audioClient.ConnectionState != ConnectionState.Connected) { }
        var discord = audioClient.CreatePCMStream(AudioApplication.Voice, 128 * 1024,200);
        WriteAudioStream(discord, file);
        await channel.DisconnectAsync();
        File.Delete(file);
    }
    
    
        private static readonly WaveFormat DiscordOutputFormat = new WaveFormat(48000, 16, 2);

        public void WriteAudioStream(
            Stream outputStream,
            string audioFile)
        {
            using (var audio = new AudioFileReader(audioFile))
            {
                
                var resampler = new WdlResamplingSampleProvider(audio, 48000).ToStereo().ToWaveProvider16();
                
                var blockSize = DiscordOutputFormat.AverageBytesPerSecond / 50;
                var buffer = new byte[blockSize];
                var byteCount = 0;

                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                {
                    if (byteCount < blockSize)
                    {
                        for (int i = byteCount; i < blockSize; i++)
                        {
                            buffer[i] = 0;
                        }
                    }

                    outputStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    
}
