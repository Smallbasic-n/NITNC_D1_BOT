// Require the necessary discord.js classes
const { Client, Events, GatewayIntentBits, Collection, REST, Routes, SlashCommandBuilder, ChatInputCommandInteraction } = require('discord.js');
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
const client = new Client({ intents: [GatewayIntentBits.Guilds] });

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
        await interaction.reply("<@"+accountId+"> の中の人\n 名前："+givenname+"　"+firstname+"　("+rohmegivenname+" "+rohmefirstname+")\n学籍番号："+studentid);
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
        if (data == undefined) {await interaction.reply({ content: "@"+user.username+"の情報は登録されていません．", ephemeral: true});return;}
        await interaction.reply({ content: "@"+user.username+" の中の人\n 名前："+data.givenName+"　"+data.firstName+"　("+data.rohmeGivenName+" "+data.rohmeFirstName+")\n学籍番号："+data.studentId, ephemeral: true});
    },
};

commands.push(iam_command.data.toJSON());
commands.push(whois_command.data.toJSON());

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

client.login(token);