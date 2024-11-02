// Require the necessary discord.js classes
const { Client, Events, GatewayIntentBits, Collection, REST, Routes, SlashCommandBuilder, ChatInputCommandInteraction, PermissionsBitField,Partials, EmbedBuilder } = require('discord.js');
const { token, guildId, clientId, passphrase, salt } = require('./matsudaira.json');
const iv= Buffer.from('00000000000000000000000000000000', 'hex');
const fs = require('node:fs')
const crypto = require('crypto');
const algorithm = 'aes-256-ctr';

var write = (text) =>{ //encrypt
  const key = crypto.scryptSync(passphrase, salt, 32)
  var cipher = crypto.createCipheriv(algorithm,key,iv)
  var crypted = cipher.update(text,'utf8','base64')
  crypted += cipher.final('base64');
  fs.writeFileSync('./user.data',crypted);
  return crypted;
}

var read = () => {//decrypt
  const key = crypto.scryptSync(passphrase, salt, 32)
  const text=fs.readFileSync('./user.data').toString();
  var decipher = crypto.createDecipheriv(algorithm,key,iv)
  var dec = decipher.update(text,'base64','utf8')
  dec += decipher.final('utf8');
  return dec;
}


var userData={users: [{studentId: 'Z99999', givenName: 'TEST', firstName: 'TESTER', rohmeFirstName: 'TESUTA', rohmeGivenName: 'TESUTO', accountId: 'INVAILD'}],roles: []}
//if (!fs.existsSync('./user.iv')) {fs.writeFileSync("./user.iv",crypto.randomBytes(16))}
if (!fs.existsSync('./user.data')) {write(JSON.stringify(userData))}
userData=JSON.parse(read());
// Create a new client instance
const client = new Client({
	intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages, GatewayIntentBits.GuildMessageReactions],
	partials: [Partials.Message, Partials.Channel, Partials.Reaction],
    disableEveryone: false
});

const commands = [];
client.commands = new Collection();
const iam_command={
    data: new SlashCommandBuilder()
        .setName('iam')
        .setDescription('松平定信公にあなたの情報を登録します．(登録情報はこのDiscordサーバ上で公開されます．また，その情報は定信公がデプロイされているサーバ上に暗号化処理を施して安全に保存されます．)')
        .addStringOption(option=>option.setName('givenname').setDescription("日本語での名字(ex: 静岡)").setRequired(true))
        .addStringOption(option=>option.setName('firstname').setDescription("日本語での名前(ex: 太郎)").setRequired(true))
        .addStringOption(option=>option.setName('rohmegivenname').setDescription("ローマ表記での名字(ex: Shizuoka)").setRequired(true))
        .addStringOption(option=>option.setName('rohmefirstname').setDescription("ローマ表記での名前(ex: Taro)").setRequired(true))
        .addStringOption(option=>option.setName('studentid').setDescription("学籍番号(ex: D24127)").setRequired(true))
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const givenname=interaction.options.getString("givenname");
        const firstname=interaction.options.getString("firstname");
        const rohmegivenname=interaction.options.getString("rohmegivenname").toUpperCase();
        const rohmefirstname=interaction.options.getString("rohmefirstname");
        const studentid=interaction.options.getString("studentid");
        const accountId=interaction.user.id;
        var completed=false;
        for (let i = 0; i < userData.users.length; i++) {
            if (userData.users[i].accountId == accountId){
                userData.users[i].givenName = givenname
                userData.users[i].firstName = firstname
                userData.users[i].rohmeGivenName = rohmegivenname
                userData.users[i].rohmeFirstName = rohmefirstname
                userData.users[i].studentId = studentid
                completed=true;
                break;
            }
        }
        if (!completed){
            const newuser={studentId: studentid, givenName: givenname, firstName: firstname, rohmeFirstName: rohmefirstname, rohmeGivenName:rohmegivenname, accountId: accountId};
            userData.users.push(newuser);
        }
        write(JSON.stringify(userData));
        const embd=new EmbedBuilder()
            .setColor(0x87ebec)
            .setTitle('@'+interaction.user.username+' の中の人')
            .setAuthor({name: '松平定信'})
            .setDescription('アカウントの中の人の情報を松平定信公に登録しました．')
            .addFields(
                { name: '名前', value: givenname+'　'+firstname},
                { name: 'Name', value: rohmegivenname+' '+rohmefirstname},
                { name: '学籍番号', value: studentid }
            )
            .setTimestamp()
            .setFooter({ text: '老中 松平定信 https://github.com/Smallbasic-n/NITNC_D1_BOT'})
        await interaction.reply({ content: '<@'+accountId+'>',embeds: [embd]});
    },
};
const whois_command={
    data: new SlashCommandBuilder()
        .setName('whois')
        .setDescription('松平定信公に指定したアカウントの中の人を照会します．')
        .addUserOption(option=>option.setName('account').setDescription('中の人を知りたいアカウント').setRequired(true))
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const user=interaction.options.getUser("account")
        const accountId=user.id;
        const data=userData.users.find(element=>element.accountId==accountId)
        interaction.ephemeral=true;
        var roles=        interaction.guild.members.cache.get(accountId).roles.cache.map(r => `${r}`).join(' | ')
        if (data == undefined) {await interaction.reply({ content: "@"+user.username+"の情報は登録されていません．", ephemeral: true});return;}

        const embd=new EmbedBuilder()
            .setColor(0x87ceeb)
            .setTitle('@'+user.username+' の中の人')
            .setAuthor({name: '松平定信'})
            .setDescription('アカウントの中の人を松平定信公に照会しました．')
            .addFields(
                { name: '名前', value: data.givenName+'　'+data.firstName},
                { name: 'Name', value: data.rohmeGivenName+' '+data.rohmeFirstName},
                { name: '学籍番号', value: data.studentId },
                { name: 'ロール', value: roles}
            )
            .setTimestamp()
            .setFooter({ text: '老中 松平定信 https://github.com/Smallbasic-n/NITNC_D1_BOT'})
        await interaction.reply({ embeds: [embd], ephemeral: true});
    },
};
const role_command={
    data: new SlashCommandBuilder()
        .setName('create')
        .setDescription('松平定信公にロールを作成してもらいます．')
        .addStringOption(option=>option.setName("name").setDescription("ロール名").setRequired(true))
        .addStringOption(option=>option.setName('color').setDescription('ロールの色を指定します．').setRequired(false))
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const user=interaction.user;
        const role=interaction.options.getString("name");
        const color=interaction.options.getString("color")??"#87ceeb";
        const members=interaction.guild.members.cache.filter(member=>member.permissions.has('ManageRoles')).find((val)=>val.id==user.id);
        if (members==undefined){
            await interaction.reply({ content: "あなたの権限が不足しています．", ephemeral: true});return;
        }
        var id=(await interaction.guild.roles.create({
                    name: role,
                    color: color,
                    reason: '松平定信公により作成されました．',
                    permissions: [PermissionsBitField.Flags.SendMessages, PermissionsBitField.Flags.ViewChannel, PermissionsBitField.Flags.MentionEveryone,PermissionsBitField.Flags.UseExternalEmojis,PermissionsBitField.Flags.AddReactions,PermissionsBitField.Flags.CreatePublicThreads,PermissionsBitField.Default,PermissionsBitField.Flags.CreatePrivateThreads],
                    mentionable: true

            })).id;
        //if (userData.roles.find((val)=>val=id)==undefined){
            userData.roles.push(id);
        //}

        write(JSON.stringify(userData));
        await interaction.reply({ content: "次のロールを作成し，松平定信公の管理下に置きました．\nロール名：<@&"+id+">", ephemeral: true});
    },
};
const role_add_command={
    data: new SlashCommandBuilder()
        .setName('add')
        .setDescription('ロールを松平定信公の管理下に置きます．')
        .addRoleOption((role)=>role.setName("role").setDescription("松平定信公の管理下に置きたいロール").setRequired(true))
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const user=interaction.user;
        const role=interaction.options.getRole("role");
        const members=interaction.guild.members.cache.filter(member=>member.permissions.has('ManageRoles')).find((val)=>val.id==user.id);
        if (members==undefined){
            await interaction.reply({ content: "あなたの権限が不足しています．", ephemeral: true});return;
        }
        var find=(await interaction.guild.roles.fetch()).find((rl)=>rl.id=role.id)
        //if (find==undefined){
        //    await interaction.guild.roles.create(role);
        //    find=(await interaction.guild.roles.fetch()).find((rl)=>rl.id=role.id)
        //}
        //if (userData.roles.find((val)=>val=find.id)==undefined){
            userData.roles.push(find.id);
        //}

        write(JSON.stringify(userData));
        await interaction.reply({ content: "次のロールを松平定信公の管理下に置きました．\nロール名：<@&"+find.id+">", ephemeral: true});
    },
};
const role_list_command={
    data: new SlashCommandBuilder()
        .setName('list')
        .setDescription('松平定信公の管理下に置かれているロールを表示します．')
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const user=interaction.user;
        const members=interaction.guild.members.cache.filter(member=>member.permissions.has('ManageRoles')).find((val)=>val.id==user.id);
        if (members==undefined){
            await interaction.reply({ content: "あなたの権限が不足しています．", ephemeral: true});return;
        }
        var data="";
        userData.roles.forEach(element => {
            data=data+"\n<@&"+element+"> (ID:"+element+")"
        });
        await interaction.reply({ content: "次のロールは松平定信公の管理下に置かれています．"+data, ephemeral: true});
    },
};
const role_del_command={
    data: new SlashCommandBuilder()
        .setName('del')
        .setDescription('ロールを松平定信公の管理下からはずします．')
        .addRoleOption((role)=>role.setName("role").setDescription("松平定信公の管理下より外したいロール").setRequired(false))
        .addStringOption((rolei)=>rolei.setName("roleint").setDescription("松平定信公の管理下より外したいロールId(すでに削除されたロールに使用)").setRequired(false))
        
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const user=interaction.user;
        const role=interaction.options.getRole("role");
        const rolei=(interaction.options.getString("roleint"));
        if (role == undefined&&rolei==undefined){
            await interaction.reply({ content: "ロール名を指定してください．", ephemeral: true});return;
        }
        const members=interaction.guild.members.cache.filter(member=>member.permissions.has('ManageRoles')).find((val)=>val.id==user.id);
        if (members==undefined){
            await interaction.reply({ content: "あなたの権限が不足しています．", ephemeral: true});return;
        }
        const id=role == undefined? rolei:role.id
        const index=userData.roles.findIndex((val)=>val=id)
        if (index == -1){
            await interaction.reply({ content: "そのロールは松平定信公の管理下に置かれていません．", ephemeral: true});return;
        }
        userData.roles.splice(index,1);
        write(JSON.stringify(userData));
        await interaction.reply({ content: "次のロールを松平定信公の管理下から外しました．\nロール名：<@&"+id+"> (ID:"+id+")", ephemeral: true});
    },
};
const study_command={
    data: new SlashCommandBuilder()
        .setName('study')
        .setDescription('松平定信公によるロール付与令を発行します．')
        ,
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction 
     */
    async execute(interaction) {
        const user=interaction.user;
        const members=interaction.guild.members.cache.filter(member=>member.permissions.has('ManageRoles')).find((val)=>val.id==user.id);
        if (members==undefined){
            await interaction.reply({ content: "あなたの権限が不足しています．", ephemeral: true});return;
        }
        var explain=""
        var emojis=[]
        var index=1
        userData.roles.forEach(element => {
            var nowemoji=String.fromCodePoint(0x1F600 + index);
            emojis.push(nowemoji);
            explain+="\n"+nowemoji+"：<@&"+element+">"
            index+=1;
        });
        await interaction.reply({ content: "事前審査を通過しました．", ephemeral: true , });
        var message=await interaction.guild.channels.cache.get(interaction.channelId).send({content: '松平定信公によるロール付与の令\n @everyone ，自分が該当するロールにリアクションをつけてください．'+explain})
        //console.log(message)
        //interaction.guild.channels.cache.get(interaction.channelId).lastMessage
        emojis.forEach(elem=>{
            message.react(elem);
        })
        
    },
};

commands.push(iam_command.data.toJSON());
commands.push(whois_command.data.toJSON());
commands.push(role_command.data.toJSON());
commands.push(role_add_command.data.toJSON());
commands.push(role_list_command.data.toJSON());
commands.push(role_del_command.data.toJSON());
commands.push(study_command.data.toJSON());

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

client.commands.set(iam_command.data.name, iam_command);
client.commands.set(whois_command.data.name, whois_command);
client.commands.set(role_command.data.name, role_command);
client.commands.set(role_add_command.data.name, role_add_command);
client.commands.set(role_list_command.data.name, role_list_command);
client.commands.set(role_del_command.data.name, role_del_command);
client.commands.set(study_command.data.name, study_command);

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
    member.setNickname('松平定信').catch(console.error);
	console.log(`Ready! Logged in as ${readyClient.user.tag}`);
});
client.on(Events.MessageReactionAdd,  async(reaction,user)=>{
    const message = reaction.message
   const member = message.guild.members.resolve(user)
   if (member.id==client.user.id){return;}
   var ok=false
   var index=1
   userData.roles.forEach(element => {
        if (ok){return;}
       var nowemoji=String.fromCodePoint(0x1F600 + index);
       if (reaction.emoji.name === nowemoji){
            member.roles.add(message.guild.roles.cache.find(role=>role.id == element))
            ok=true;
       }
       index+=1
   });
})
client.login(token);