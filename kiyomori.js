// Require the necessary discord.js classes
const { Client, Events, GatewayIntentBits,Partials, Collection, REST, SlashCommandBuilder, Routes, EmbedBuilder, ChatInputCommandInteraction, Integration, Guild } = require('discord.js');
const request=require('request')
const { token, guildId, scheduleId, passphrase, salt, clientId } = require('./kiyomori.json');

/*const token=process.env.DISCORD_TOKEN;
const guildId=process.env.DISCORD_GUILDID;
const chankId=process.env.DISCORD_CHANK_CHID;
const passphrase=process.env.PASSPHRASE;
const salt=process.env.SALT;
const clientId=process.env.DISCORD_CLIENTID;
const factId=process.env.DISCORD_FACTBOOK_CHID;
const dataImportId=process.env.CSV_CHID;
const chankStepId=process.env.CHANK_RANGE_ID;
const factRangeId=process.env.FACT_RANGE_ID;
const interval=parseInt(process.env.INTERVAL);*/

const encryptFile="./kiyomori.data"
const iv= Buffer.from('00000000000000000000000000000000', 'hex');
const fs = require('node:fs')
const crypto = require('crypto');
const algorithm = 'aes-256-ctr';

var write = (text) =>{ //encrypt
  const key = crypto.scryptSync(passphrase, salt, 32)
  var cipher = crypto.createCipheriv(algorithm,key,iv)
  var crypted = cipher.update(text,'utf8','base64')
  crypted += cipher.final('base64');
  fs.writeFileSync(encryptFile,crypted);
  return crypted;
}

var read = () => {//decrypt
  const key = crypto.scryptSync(passphrase, salt, 32)
  const text=fs.readFileSync(encryptFile).toString();
  var decipher = crypto.createDecipheriv(algorithm,key,iv)
  var dec = decipher.update(text,'base64','utf8')
  dec += decipher.final('utf8');
  return dec;
}

var userData={schedule:[{subjectName: "",year:2024,month:11,day: 1,start: 0,end:0,complete_yester: true,complete_day: true}],assignment:[{subjectName: "",assignments:[{name: "",start:"",end:"",unit:"P.",prefix:true}]}]}
if (!fs.existsSync(encryptFile)) {write(JSON.stringify({schedule:[],assignment:[]}))}
userData=JSON.parse(read());

const client = new Client({
	intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages, GatewayIntentBits.GuildMessageReactions,GatewayIntentBits.MessageContent],
	partials: [Partials.Message, Partials.Channel, Partials.Reaction],
    disableEveryone: false
});

const commands = [];
client.commands = new Collection();
const add_schedule={
    data: new SlashCommandBuilder()
        .setName('schedule')
        .setDescription('平清盛公に授業変更を通達します．')
        .addStringOption(option=>option.setName('subjectname').setDescription("教科名(ex: 基礎数学I)").setRequired(true))
        .addIntegerOption(option=>option.setName('year').setDescription("変更後の実施日(年)(ex: 2024)").setRequired(true))
        .addIntegerOption(option=>option.setName('month').setDescription("変更後の実施日(月)(ex: 11)").setRequired(true))
        .addIntegerOption(option=>option.setName('day').setDescription("変更後の実施日(日)(ex: 25)").setRequired(true))
        .addIntegerOption(option=>option.setName('start').setDescription("変更後の授業開始時数(ex: 1,2限ならばここでは1)").setRequired(true))
        .addIntegerOption(option=>option.setName('end').setDescription("変更後の授業終了時数(ex: 1,2限ならばここでは2)").setRequired(true))
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const subjectName=interaction.options.getString("subjectname");
        const year=interaction.options.getInteger("year");
        const month=interaction.options.getInteger("month");
        const day=interaction.options.getInteger("day");
        const start=interaction.options.getInteger("start");
        const end=interaction.options.getInteger("end");
        console.log("called "+ year +"/"+month+"/"+day+" "+start+"~"+end);
        if (subjectName == undefined||year == undefined||month == undefined||day == undefined||start == undefined||end == undefined){
            console.log("invalid");
            await interaction.reply({ content: "入力された内容が不正です．もう一度確認してください．",ephemeral: true});
            return;
        }
        userData.schedule.push({subjectName: subjectName,year: year, month:month,day:day, start:start, end:end, complete_day: false,complete_yester: false});
        console.log("pushed");
        write(JSON.stringify(userData));
        console.log("writed");
        const embd=new EmbedBuilder()
            .setColor(0x87beec)
            .setTitle(subjectName+'の授業変更について')
            .setAuthor({name: '平清盛'})
            .setDescription(subjectName+'で授業変更がありました．皆さんお気をつけ下さい．')
            .addFields(
                { name: '教科名', value: subjectName},
                { name: '変更後の実施日時，時限', value: year+'年'+month+'月'+day+'日　'+start+'限～'+end+'限'}
            )
            .setTimestamp()
            .setFooter({ text: '太政大臣 平清盛 https://github.com/Smallbasic-n/NITNC_D1_BOT'})
        await interaction.reply({ content: '@everyone',embeds: [embd]});
    },
};
commands.push(add_schedule.data.toJSON());
const rest = new REST().setToken(token);

(async () => {
	try {
		console.log(`Started refreshing ${commands.length} application (/) commands.`);

		// The put method is used to fully refresh all commands in the guild with the current set
		const data = await rest.put(
			Routes.applicationGuildCommands(clientId, guildId),
			{ body: commands },
		);

		console.log(`Successfully reloaded ${data.length} application (/) commands.`);
	} catch (error) {
		// And of course, make sure you catch and log any errors!
		console.error(error);
	}
})();

client.commands.set(add_schedule.data.name, add_schedule);

client.on(Events.InteractionCreate, async interaction => {
	if (!interaction.isChatInputCommand()) return;

	const command = interaction.client.commands.get(interaction.commandName);

	if (!command) {
		console.error(`No command matching ${interaction.commandName} was found.`);
		return;
	}

	try {
		await command.execute(interaction);
	} catch (error) {
		console.error(error);
		if (interaction.replied || interaction.deferred) {
			await interaction.followUp({ content: 'There was an error while executing this command!', ephemeral: true });
		} else {
			await interaction.reply({ content: 'There was an error while executing this command!', ephemeral: true });
		}
	}
})
client.once(Events.ClientReady,async readyClient => {
    const guild=client.guilds.cache.get(guildId);
    const member = guild.members.cache.get(client.user.id);
    member.setNickname('平清盛').catch(console.error);
	console.log(`Ready! Logged in as ${readyClient.user.tag}`);
    foundAleart(guild);
    setInterval(()=>foundAleart(guild), 3600*1000);
});
/**
 * 
 * @param {Guild} guild 
 */
function foundAleart(guild){
    var i=0
    userData.schedule.forEach((data)=>{
        var dates=new Date();
        dates.setUTCHours(dates.getUTCHours()+9)
        var year=dates.getUTCFullYear()
        var month = dates.getUTCMonth()+1
        var day= dates.getUTCDate()
        if (data.year==year&&data.month==month&&data.day==(day+1)&&!data.complete_yester){
            guild.channels.cache.get(scheduleId).send("@everyone \n# 重要　明日の授業変更について \n明日"+data.year+"年"+data.month+"月"+data.day+"日"+"の"+data.start+"限～"+data.end+"限は**"+data.subjectName+"**が実施されます．お忘れ物をないさいませぬよう，お気をつけ下さい．\n 太政大臣 平清盛")
            userData.schedule[i].complete_yester=true;
            userData.schedule[i].complete_day=false;
            write(JSON.stringify(userData));
        }else if(data.year==year&&data.month==month&&data.day==day&&!data.complete_day){
            guild.channels.cache.get(scheduleId).send("@everyone \n# 重要　本日の授業変更について \n昨日お伝えした通り，本日"+data.year+"年"+data.month+"月"+data.day+"日"+"の"+data.start+"限～"+data.end+"限は**"+data.subjectName+"**が実施されます．お忘れ物をないさいませぬよう，お気をつけ下さい．\n 太政大臣 平清盛")
            userData.schedule[i].complete_yester=true;
            userData.schedule[i].complete_day=true;
            write(JSON.stringify(userData));
        }
        i +=1;
    })
}
client.login(token);
