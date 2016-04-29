# KiteBot
##KiteBot is a Discord bot for the Giant Bomb Discord Community under the MIT license.
###Kite Co. Discord Chat Bot

####Set Up Guide
1. Clone or download repositor to local machine
2. Open the KiteBot.sln with visual studio
3. Get dependencies. 
  a. Use nuget to install Discord.NET, Discord.NET.Commands, and Discord.NET.Modules. Note that all of these are pre release check search release
  b. Get TextMarkovChains. Either go to https://github.com/LassieME/MarkovChainSentenceGenerator and download and compile or go to https://github.com/LassieME/MarkovChainSentenceGenerator/releases/tag/0.1 and extract.
  
  add settings.json under Content in kitebot folder in the following format
  ```
  {
  "DiscordEmail": null,
  "DiscordPassword": null,
  "DiscordToken": null,
  "GiantBombApiKey": "",
  "OwnerId": 0,
  "MarkovChainStart": true,
  "MarkovChainDepth": 2,
  "GiantBombVideoRefreshRate": 60000,
  "GiantBombLiveStreamRefreshRate": 60000
}
```
4. Go to https://discordapp.com/developers/docs/intro and create a bot account
5. Go to Giantbomb.com and create an account.
6. Fill in DiscordToken, GiantBombApiKey, OwnerID (this is your discord account not the bot)
7. Invite Bot to sever with https://discordapp.com/oauth2/authorize?&client_id=YOUR_BOT_ID_HERE&scope=bot
8. Complile and run
