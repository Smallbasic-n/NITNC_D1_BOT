// Require the necessary discord.js classes
const { Client, Events, GatewayIntentBits,Partials, Collection, REST, SlashCommandBuilder, Routes, EmbedBuilder } = require('discord.js');
const path=require('node:path')
const request=require('request')
const { token, guildId, chankId, passphrase, salt, clientId, factId, dataImportId, chankStepId,factRangeId } = require('./lincoln.json');
const chankData="./lincoln.csv"
const factbookData="./factbook.csv"
const iv= Buffer.from('00000000000000000000000000000000', 'hex');
const fs = require('node:fs')
const crypto = require('crypto');
const algorithm = 'aes-256-ctr';

var write = (text) =>{ //encrypt
  const key = crypto.scryptSync(passphrase, salt, 32)
  var cipher = crypto.createCipheriv(algorithm,key,iv)
  var crypted = cipher.update(text,'utf8','base64')
  crypted += cipher.final('base64');
  fs.writeFileSync('./lincoln.data',crypted);
  return crypted;
}

var read = () => {//decrypt
  const key = crypto.scryptSync(passphrase, salt, 32)
  const text=fs.readFileSync('./lincoln.data').toString();
  var decipher = crypto.createDecipheriv(algorithm,key,iv)
  var dec = decipher.update(text,'base64','utf8')
  dec += decipher.final('utf8');
  return dec;
}


var userData={user:[{accountId: "",chank: 0,factbook: 0}],chankStep:1,factbookRange:[1,2]}
//if (!fs.existsSync('./user.iv')) {fs.writeFileSync("./user.iv",crypto.randomBytes(16))}
if (!fs.existsSync('./lincoln.data')) {write(JSON.stringify({user:[]}))}
userData=JSON.parse(read());

//const token=process.env.DISCORD_TOKEN;
//const guildId=process.env.DISCORD_GUILDID;
//const clientId=process.env.DISCORD_CLIENTID;

const client = new Client({
	intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages, GatewayIntentBits.GuildMessageReactions,GatewayIntentBits.MessageContent],
	partials: [Partials.Message, Partials.Channel, Partials.Reaction],
    disableEveryone: false
});
const commands = [];
client.commands = new Collection();
const chanp_command={
    data: new SlashCommandBuilder()
        .setName('champ')
        .setDescription('チャンクで英単語、FACTBOOK これからの英文法 暗唱例文集のクイズでの成績を表示します．')
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        var chankId=[];
        var chankCt=0;
        var factId=[];
        var factCt=0;
        userData.user.forEach(data=>{
            if (data.chank==chankCt){
                chankId.push(data.accountId);
            }else if (data.chank>chankCt){
                chankId=[data.accountId]
                chankCt=data.chank;
            }
            if (data.factbook==factCt){
                factId.push(data.accountId);
            }else if (data.factbook>factCt){
                factId=[data.accountId]
                factCt=data.factbook;
            }
        })
        var chank_all='';
        var fact_all='';
        chankId.forEach(data=>{
            chank_all+="<@"+data+"> "
        })
        factId.forEach(data=>{
            fact_all+="<@"+data+"> "
        })
        const embd=new EmbedBuilder()
            .setColor(0x87ebec)
            .setTitle('リンカン調べ！チャンクで英単語, 暗唱例文チャンプ！')
            .setAuthor({name: 'エイブラハム・リンカン'})
            .setDescription('チャンク・暗唱例文クイズの結果を表示します．')
            .addFields(
                { name: 'チャンクチャンプ', value: chank_all},
                { name: 'チャンク正当数', value: chankCt.toString()},
                { name: '暗唱例文チャンプ', value: fact_all },
                { name: '暗唱例文正当数', value: factCt.toString()},
            )
            .setTimestamp()
            .setFooter({ text: 'アメリカ合衆国 第16第 大統領 エイブラハム・リンカン https://github.com/Smallbasic-n/NITNC_D1_BOT'})
        await interaction.reply({ embeds: [embd]});
    },
};

commands.push(chanp_command.data.toJSON());

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

client.commands.set(chanp_command.data.name, chanp_command);

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

var chank = fs.readFileSync(chankData).toString().split('\n').map(row => row.split(','));
var factbook = fs.readFileSync(factbookData).toString().split('\n').map(row => row.split(','));

var rangeChank=[[]]
chank.forEach((data)=>{
    if (data[0]==userData.chankStep) rangeChank.push(data)
})
var iiii=0
factbook.forEach((data)=>{
    if (iiii>=userData.factbookRange[0]&&iiii<=userData.factbookRange[1]) rangeChank.push(data);
    iiii+=1
})
var chankAns="";
var factAns="";
var chankStepAns="";
var factRangeAns="";
var beforeRange=[]
var factbookBeforeRange=[]
var before=[]
var factbookBefore=[]
client.once(Events.ClientReady,async readyClient => {
    const guild=client.guilds.cache.get(guildId);
    const member = guild.members.cache.get(client.user.id);
    member.setNickname('エイブラハム・リンカン').catch(console.error);
	console.log(`Ready! Logged in as ${readyClient.user.tag}`);
    setInterval(async () => {
        var ok=false;
        var i=-1;
        while (!ok){
            i = Math.floor(Math.random() * (chank.length - 0)) + 0
            ok=true
            if (before.find(id=>id==i) != undefined){
                if (before.length==chank.length) before=[];
                else ok=false;
            }
            if (chank[i][0]==undefined||chank[i][1]==undefined||chank[i][2]==undefined||chank[i][0]==''||chank[i][1]==''||chank[i][2]=='') ok=false
        }
        before.push(i)
        var index=chank[i]
        chankAns=index[2].replaceAll(" ","").replaceAll("　","").replaceAll(",","").replaceAll("、","").toUpperCase();
        guild.channels.cache.get(chankId).send("日本語："+index[1]+"\n英語："+index[2]);
        
        ok=false;
        i=-1;
        while (!ok){
            i = Math.floor(Math.random() * (factbook.length - 0)) + 0
            ok=true
            if (factbookBefore.find(id=>id==i) != undefined){
                if (factbookBefore.length==factbook.length) factbookBefore=[];
                else ok=false;
            }
            if (factbook[i][0]==undefined||factbook[i][1]==undefined||factbook[i][0]==''||factbook[i][1]=='') ok=false
        }
        factbookBefore.push(i)
        var index=factbook[i]
        factAns=index[1].replaceAll("　"," ").replaceAll("、",",");
        guild.channels.cache.get(factId).send("日本語："+index[0]);



        var ok=false;
        var i=-1;
        while (!ok){
            i = Math.floor(Math.random() * (chank.length - 0)) + 0
            ok=true
            if (beforeRange.find(id=>id==i) != undefined){
                if (beforeRange.length==chankStep.length) beforeRange=[];
                else ok=false;
            }
            if (rangeChank[i][0]==undefined||rangeChank[i][1]==undefined||rangeChank[i][2]==undefined||rangeChank[i][0]==''||rangeChank[i][1]==''||rangeChank[i][2]=='') ok=false
        }
        beforeRange.push(i)
        var index=rangeChank[i]
        chankStepAns=index[2].replaceAll(" ","").replaceAll("　","").replaceAll(",","").replaceAll("、","").toUpperCase();
        guild.channels.cache.get(chankStepId).send("日本語："+index[1]+"\n英語："+index[2]);
        
        ok=false;
        i=-1;
        while (!ok){
            i = Math.floor(Math.random() * (factbook.length - 0)) + 0
            ok=true
            if (factbookBeforeRange.find(id=>id==i) != undefined){
                if (factbookBeforeRange.length==factbook.length) factbookBeforeRange=[];
                else ok=false;
            }
            if (factbookBeforeRange[i][0]==undefined||factbookBeforeRange[i][1]==undefined||factbookBeforeRange[i][0]==''||factbookBeforeRange[i][1]=='') ok=false
        }
        factbookBeforeRange.push(i)
        var index=factbookBeforeRange[i]
        factRangeAns=index[1].replaceAll("　"," ").replaceAll("、",",");
        guild.channels.cache.get(factRangeId).send("日本語："+index[0]);
    },10*1000)
});

client.on(Events.MessageCreate, async (message)=>{
    if (message.author.id==client.user.id) return;
    if (message.channelId==chankId){
        const content=message.content
        if (content.replaceAll(" ","").replaceAll("　","").replaceAll(",","").replaceAll("、","").toUpperCase()==chankAns){
            message.react('💯')
            var index=userData.user.findIndex((usr)=>usr.accountId==message.author.id)
            if (index==-1){userData.user.push({accountId: message.author.id, chank: 0, factbook: 0});index=userData.user.findIndex((usr)=>usr.accountId==message.author.id);}
            userData.user[index].chank+=1;
            write(JSON.stringify(userData));
        }else{
            message.react('🤔')
        }
    }else if (message.channelId==factId){
        const content=message.content
        if (content.replaceAll("　"," ").replaceAll("、",",")==factAns){
            message.react('💯')
            var index=userData.user.findIndex((usr)=>usr.accountId==message.author.id)
            if (index==-1){userData.user.push({accountId: message.author.id, chank: 0, factbook: 0});index=userData.user.findIndex((usr)=>usr.accountId==message.author.id);}
            userData.user[index].factbook+=1;
            write(JSON.stringify(userData));
        }else{
            message.react('🤔')
        }        
    }else if (message.channelId==dataImportId){
        const attach=message.attachments.map(attachment=>attachment.url);
        if (attach.length==0){
            message.reply("csvファイルを添付してください．")
        }else{
            request({url: attach[0],method: "GET"},(er,rp,bd)=>{
                if (message.content.includes("chank")){
                    fs.writeFileSync(chankData,fs.readFileSync(chankData).toString()+'\n'+bd)
                    chank = fs.readFileSync(chankData).toString().split('\n').map(row => row.split(','));                    
                    message.reply('チャンクで英単語のデータを追加しました．(STEP:'+chank[chank.length-1][0]+')')
                }else if(message.content.includes("factbook")){
                    fs.writeFileSync(factbookData,fs.readFileSync(factbookData).toString()+'\n'+bd)
                    const lasti=factbook.length;
                    factbook = fs.readFileSync(factbookData).toString().split('\n').map(row => row.split(','));                    
                    message.reply('FACTBOOK 暗唱例文集のデータを追加しました．('+lasti+"~"+factbook.length+')')
                }
            })
        }
    }
})
client.login(token);
