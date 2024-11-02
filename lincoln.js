// Require the necessary discord.js classes
const { Client, Events, GatewayIntentBits,Partials } = require('discord.js');
const express = require('express')
const fs = require('node:fs')
const { token, guildId, chankId, passphrase, salt } = require('./lincoln.json');

//const token=process.env.DISCORD_TOKEN;
//const guildId=process.env.DISCORD_GUILDID;
//const clientId=process.env.DISCORD_CLIENTID;

const client = new Client({
	intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages, GatewayIntentBits.GuildMessageReactions],
	partials: [Partials.Message, Partials.Channel, Partials.Reaction],
    disableEveryone: false
});
const app=express()
const port=3000

const rows = fs.readFileSync("./lincoln.csv").toString().split('\n');
const chank = rows.map(row => row.split(','));
var chankAns="";
client.once(Events.ClientReady,async readyClient => {
    const guild=client.guilds.cache.get(guildId);
    const member = guild.members.cache.get(client.user.id);
    member.setNickname('エイブラハム・リンカン').catch(console.error);
	console.log(`Ready! Logged in as ${readyClient.user.tag}`);
    var before=[]
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
        }
        before.push(i)
        var index=chank[i]
        chankAns=index[2];
        //console.log(i);
        guild.channels.cache.get(chankId).send("日本語："+index[0]+"\n英語："+index[1]);
    },10*1000)
});
client.login(token);

