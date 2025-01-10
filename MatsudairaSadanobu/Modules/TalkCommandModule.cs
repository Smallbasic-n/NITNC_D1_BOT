using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
    [SlashCommand("talk", "松平定信がずんだもんの声でしゃべります．",runMode: RunMode.Async)]
    public async Task TalkCommand([Summary("content","内容")]string content,[Summary("channel","ボイスチャネル")] IVoiceChannel channel)
    {
        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:50021")
        };
        var quary=await httpClient.PostAsync($"/audio_query?text={content}&speaker=3",JsonContent.Create(""));
        var quaryData=await quary.Content.ReadAsStringAsync();
        var audio= await (await httpClient.PostAsync("/synthesis?speaker=1&enable_interrogative_upspeak=true",new StringContent(quaryData,Encoding.UTF8,"application/json"))).Content.ReadAsByteArrayAsync();

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
