using Discord;

namespace DiscordBotBasic;

public static class Supports
{
    /// <summary>
    /// アプリケーションの名前を指定します．例：今川義元
    /// </summary>
    public static string ApplicationName { get; set; } = "";
    /// <summary>
    /// アプリケーション名の接頭辞を指定します．例：駿河国
    /// </summary>
    public static string ApplicationPrefix { get; set; } = "";
    /// <summary>
    /// 指定された情報をもとにEmbedメッセージを作成します．
    /// </summary>
    /// <param name="title">Embedメッセージのタイトル</param>
    /// <param name="description">Embedメッセージの詳細</param>
    /// <param name="fields">載せる情報．任意</param>
    /// <returns>作成されたEmbed</returns>
    public static Embed EmbedInstanceCreator(string title, string description, params EmbedFieldBuilder[] fields)
    {
        return new EmbedBuilder()
            .WithAuthor(ApplicationName)
            .WithDescription(description)
            .WithTitle(title)
            .WithFields(fields)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter(
                new EmbedFooterBuilder()
                    .WithText(ApplicationPrefix+" "+ApplicationName+" https://github.com/Smallbasic-n/NITNC_D1_BOT")
            ).Build();
    }
}