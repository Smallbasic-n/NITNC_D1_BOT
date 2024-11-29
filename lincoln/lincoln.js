// Require the necessary discord.js classes
const { Client, Events, GatewayIntentBits,Partials, Collection, REST, SlashCommandBuilder, Routes, EmbedBuilder, ChatInputCommandInteraction, Integration } = require('discord.js');
const request=require('request')
//const { token, guildId, chankId, passphrase, salt, clientId, factId, dataImportId, chankStepId,factRangeId } = require('./lincoln.json');
const token=process.env.DISCORD_TOKEN;
const guildId=process.env.DISCORD_GUILDID;
const chankId=process.env.DISCORD_CHANK_CHID;
const passphrase=process.env.PASSPHRASE;
const salt=process.env.SALT;
const clientId=process.env.DISCORD_CLIENTID;
const factId=process.env.DISCORD_FACTBOOK_CHID;
const dataImportId=process.env.CSV_CHID;
const chankStepId=process.env.CHANK_RANGE_ID;
const factRangeId=process.env.FACT_RANGE_ID;
const interval=parseInt(process.env.INTERVAL);

const chankData="/conf/chank.csv"
const factbookData="/conf/factbook.csv"
const encryptFile="/conf/lincoln.data"
const iv= Buffer.from('00000000000000000000000000000000', 'hex');
const fs = require('node:fs')
const crypto = require('crypto');
const { env } = require('node:process');
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

var userData={user:[{accountId: "",chank: 0,factbook: 0}],chankStep:1,factbookRange:[1,5]}
if (!fs.existsSync(encryptFile)) {write(JSON.stringify({user:[],chankStep:1,factbookRange:[1,5]}))}
if (!fs.existsSync(chankData)){fs.writeFileSync(chankData,"a,a,a,0")}
if (!fs.existsSync(factbookData)){fs.writeFileSync(factbookData,"a,a")}
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
            .setFooter({ text: 'アメリカ合衆国 第16代大統領 エイブラハム・リンカン https://github.com/Smallbasic-n/NITNC_D1_BOT'})
        await interaction.reply({ embeds: [embd]});
    },
};
const range_command={
    data: new SlashCommandBuilder()
        .setName('range')
        .setDescription('チャンクで英単語、FACTBOOK これからの英文法 暗唱例文集のクイズの出題範囲を表示もしくは設定します．')
        .addIntegerOption(option=>option.setName("chank").setDescription("チャンクで英単語の出題STEP番号を入力してください．").setRequired(false))
        .addIntegerOption(option=>option.setName("factsta").setDescription("暗唱例文の出題開始番号を入力してください．").setRequired(false))
        .addIntegerOption(option=>option.setName("factstp").setDescription("暗唱例文の出題終了番号を入力してください．").setRequired(false))
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const chanknum=interaction.options.getInteger("chank")
        const factsta=interaction.options.getInteger("factsta")
        const factstp=interaction.options.getInteger("factstp")
        //var edited=false;
        if (chanknum!=undefined){
            userData.chankStep=chanknum;
            write(JSON.stringify(userData))
            chankRangeAll.question=[]
            chankRangeAll.question.push(["a","a"])
            chankAll.question.forEach((data)=>{
                if (data[0]==userData.chankStep.toString()) chankRangeAll.question.push(data)
            })
            //edited=true
        }
        if (factsta!=undefined&factstp!=undefined){
            userData.factbookRange[0]=factsta;
            userData.factbookRange[1]=factstp;
            write(JSON.stringify(userData))
            factbookRangeAll.question=[]
            factbookRangeAll.question.push(["a","a"])
            for (let i = 0; i < factbookAll.question.length; i++) {
                if (userData.factbookRange[0]<=i &&i<=userData.factbookRange[1]) factbookRangeAll.question.push(factbookAll.question[i]);    
            }
            //edited=true      
        }
        console.log("Chank step:"+userData.chankStep)
        console.log("Chank step length:"+chankRangeAll.question.length)
        console.log("Factbook Range:"+userData.factbookRange[0]+"~"+userData.factbookRange[1])
        console.log("Factbook Range length:"+factbookRangeAll.question.length)
        const embd=new EmbedBuilder()
            .setColor(0x87ebec)
            .setTitle('チャンクで英単語, 暗唱例文出題範囲')
            .setAuthor({name: 'エイブラハム・リンカン'})
            .setDescription('チャンク・暗唱例文クイズの出題範囲を表示します．')
            .addFields(
                { name: 'チャンク', value: userData.chankStep.toString()},
                { name: '暗唱例文', value: userData.factbookRange[0].toString()+"~"+userData.factbookRange[1].toString()},
            )
            .setTimestamp()
            .setFooter({ text: 'アメリカ合衆国 第16代大統領 エイブラハム・リンカン https://github.com/Smallbasic-n/NITNC_D1_BOT'})
        await interaction.reply({ embeds: [embd]});
        //if (edited) process.exit(0);
    },
};

commands.push(chanp_command.data.toJSON());
commands.push(range_command.data.toJSON());

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
client.commands.set(range_command.data.name, range_command);

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

let chankAll={
    question:[],
    nowAnswer: "",
    before:[],
    isChank: true,
    disable:false
}
let factbookAll={
    question:[],
    nowAnswer: "",
    before:[],
    isChank: false,
    disable:false
}
chankAll.question = fs.readFileSync(chankData).toString().replaceAll('"','').split('\n').map(row => row.split(','));
factbookAll.question = fs.readFileSync(factbookData).toString().replaceAll('"','').split('\n').map(row => row.split(','));

let chankRangeAll={
    question:[],
    nowAnswer: "",
    before:[],
    isChank: true,
    disable:false
}
let factbookRangeAll={
    question:[],
    nowAnswer: "",
    before:[],
    isChank: false,
    disable:false
}

chankRangeAll.question.push(["a","a"])
chankAll.question.forEach((data)=>{
    if (data[3]==userData.chankStep.toString()) chankRangeAll.question.push(data)
})
console.log("Chank step:"+userData.chankStep)
console.log("Chank step length:"+chankRangeAll.question.length)
console.log("Factbook Range:"+userData.factbookRange[0]+"~"+userData.factbookRange[1])
console.log("Factbook Range length:"+factbookRangeAll.question.length)
factbookRangeAll.question.push(["a","a"])
for (let i = 0; i < factbookAll.question.length; i++) {
    //console.log("i="+i+","+(userData.factbookRange[0]<=i &&i<=userData.factbookRange[1]))
    if (userData.factbookRange[0]<=i &&i<=userData.factbookRange[1]) factbookRangeAll.question.push(factbookAll.question[i]);
}

client.once(Events.ClientReady,async readyClient => {
    const guild=client.guilds.cache.get(guildId);
    const member = guild.members.cache.get(client.user.id);
    member.setNickname('エイブラハム・リンカン').catch(console.error);
	console.log(`Ready! Logged in as ${readyClient.user.tag}`);
    
    /*sendQuestion(guild.channels.cache.get(chankId),chankAll);
    sendQuestion(guild.channels.cache.get(factId),factbookAll);
    sendQuestion(guild.channels.cache.get(chankStepId),chankRangeAll);
    sendQuestion(guild.channels.cache.get(factRangeId),factbookRangeAll);*/
    setInterval(async () => {
        sendQuestion(guild.channels.cache.get(chankId),chankAll);
        sendQuestion(guild.channels.cache.get(factId),factbookAll);
        sendQuestion(guild.channels.cache.get(chankStepId),chankRangeAll);
        sendQuestion(guild.channels.cache.get(factRangeId),factbookRangeAll);
    },interval*1000)
});
//csv format
//answer,japanese(,english,step)
function sendQuestion(channel,obj={
    question:[],
    nowAnswer: "",
    before:[],
    isChank: true,
    disable: false
}){
    var ok=false;
    var i=-1;
    var loop=0;
    while (!ok){
        if (loop>=(obj.question.length+5)){console.log("Loop detected");process.exit(1);}
        loop+=1
        i = Math.floor(Math.random() * (obj.question.length - 0)) + 0
        if (i==0){ok=false;continue;}//console.log("denied281 i="+i);
        ok=true
        if (obj.before.find(id=>id==i) != undefined){
            if (obj.before.length>=obj.question.length-1) obj.before=[];
            else {ok=false;continue;}
        }
        if (obj.question[i][0]==undefined||obj.question[i][1]==undefined||obj.question[i][0]==''||obj.question[i][1]=='') {ok=false;continue;}
        if (obj.isChank&&(obj.question[i][2]==undefined||obj.question[i][2]=='')) {ok=false;}
    }
    obj.disable=false;
    //console.log("next step")
    obj.before.push(i)
    var index=obj.question[i]
    console.log(Date(Date.now())+": Send "+(obj.isChank?"chank":"factbook")+" "+i+"/"+obj.question.length);
    obj.nowAnswer=index[0].replaceAll(" ","").replaceAll("　","").replaceAll(",","").replaceAll("、","").toUpperCase();
    var result="日本語："+index[1]
    if (obj.isChank)result+="\n英語："+index[2]
    channel.send(result);
}
/**
 * 
 * @param {Message<boolean>} message 
 */
function answerCheck(message,obj={
    question:[],
    nowAnswer: "",
    before:[],
    isChank: true
}){
    const content=message.content
    if (obj.disable) return;
    if (content.replaceAll(" ","").replaceAll("　","").replaceAll(",","").replaceAll("、","").toUpperCase()==obj.nowAnswer){
        message.react('💯')
        var index=userData.user.findIndex((usr)=>usr.accountId==message.author.id)
        if (index==-1){userData.user.push({accountId: message.author.id, chank: 0, factbook: 0});index=userData.user.findIndex((usr)=>usr.accountId==message.author.id);}
        if (obj.isChank) userData.user[index].chank+=1;
        else userData.user[index].factbook+=1;
        write(JSON.stringify(userData));
        obj.disable=true;
    }else{
        message.react('🤔')
    }

}

client.on(Events.MessageCreate, async (message)=>{
    if (message.author.id==client.user.id) return;
    if      (message.channelId==chankId)     answerCheck(message,chankAll)
    else if (message.channelId==factId)      answerCheck(message,factbookAll)
    else if (message.channelId==chankStepId) answerCheck(message,chankRangeAll)
    else if (message.channelId==factRangeId) answerCheck(message,factbookRangeAll)
    else if (message.channelId==dataImportId){
        const attach=message.attachments.map(attachment=>attachment.url);
        if (attach.length==0){
            message.reply("csvファイルを添付してください．")
        }else{
            request({url: attach[0],method: "GET"},(er,rp,bd)=>{
                console.log(message.content)
                if (message.content.includes("chank")){
                    fs.writeFileSync(chankData,fs.readFileSync(chankData).toString()+'\n'+bd)
                    chankAll.question = fs.readFileSync(chankData).toString().replaceAll('"','').split('\n').map(row => row.split(','));         
                    chankRangeAll.question=[];
                    chankRangeAll.question.push(["a","a"])
                    chankAll.question.forEach((data)=>{
                        if (data[3]==userData.chankStep.toString()) chankRangeAll.question.push(data)
                    })
                    console.log("Chank step:"+userData.chankStep)
                    console.log("Chank step length:"+chankRangeAll.question.length)
                    message.reply('チャンクで英単語のデータを追加しました．(STEP:'+chankAll.question[chankAll.question.length-1][3]+')')
                }else if(message.content.includes("factbook")){
                    fs.writeFileSync(factbookData,fs.readFileSync(factbookData).toString()+'\n'+bd)
                    const lasti=factbookAll.question.length;
                    factbookAll.question = fs.readFileSync(factbookData).toString().replaceAll('"','').split('\n').map(row => row.split(','));           
                    console.log("Factbook Range:"+userData.factbookRange[0]+"~"+userData.factbookRange[1])
                    console.log("Factbook Range length:"+factbookRangeAll.question.length)
                    factbookRangeAll.question=[]
                    factbookRangeAll.question.push(["a","a"])
                    for (let i = 0; i < factbookAll.question.length; i++) {
                        //console.log("i="+i+","+(userData.factbookRange[0]<=i &&i<=userData.factbookRange[1]))
                        if (userData.factbookRange[0]<=i &&i<=userData.factbookRange[1]) factbookRangeAll.question.push(factbookAll.question[i]);    
                    }                  
                    message.reply('FACTBOOK 暗唱例文集のデータを追加しました．('+lasti+"~"+factbookAll.question.length+')')
                }
            })
        }
    }
})
client.login(token);
