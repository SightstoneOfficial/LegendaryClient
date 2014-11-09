LegendaryClient
===============

Beautiful, fully-featured League of Legends client.

Still under heavy development! Lots of things don't work or don't look good...

https://www.youtube.com/watch?v=rVWdHeQcGwM&feature=youtu.be

LegendaryClient Aram Game

Working on a patcher to patch LC [Awesome splash though]
I am aware of crashing

#How To Install
https://www.youtube.com/watch?v=ZWYn_vft5Kg&feature=youtu.be
It come with a metro-like installer [no dlls needed, a stand alone program]

Known Issues / TODO
===================
* Ranked queue will crash - don't try it
* Solo Queue is buggy (if someone dodges / leaves champ select you have to restart client, for now use team queueing even for solo)
* Team Builder is not implemented
* Logging out doesn't update friends list / crashes. (just restart the client)
* Replays are in work
* ARAM sometimes shows the wrong team
* Shop is just bad, but that's like last. (again, use official client for now if you need to purchase RP, etc.)
* Getting invited to a game makes your status flick. (Not an issue but need to edit jabber-net)
* Finish Factions
* Try implementing up to Quadre ranked queue (I am not sure but will try)
* Everything breaks with slow internet speed, if you don't finish loading play page before it loops again it creates multiple instances of queue items.
* Can't join custom game if invited

LegacyPVP
=========
The new rework of LegendaryClient. I will leave this here to honor Snowl's work.
https://github.com/eddy5641/LegacyPVP
This will still be worked on, You will not see much difference between the time I put into LegendaryClient (I finished a different project)
LegacyPVP Is Based Of LegendaryClient, Except with more winterminty features [Sorry guys no 2/3/4 queue probably]

#Accounts
I will be playing by these names
eddy5641 [na]
awesomeeddy [na]
imjustpro1545 [na]
eddy5641 [euwe]

Prerequisites
=============

Currently you must have an up-to-date installation of League of Legends. It will not patch to the latest version, but it will retrieve your current data from your current installation

Build it yourself
=================

You need these DLLS:

* Awesomium
* MahApps.Metro
* SharpZipLib
* Sqlite
* PVPNetConnect : Included as a project
 
These DLLS should be included

You will also need These DLLS:
* RAFLibPlus : https://code.google.com/p/raflib-plus/downloads/list
* Json.net : Download this from NuGet in visual studio

These DLLS are Not Included. Please Download them yourself

##Users who want to use client features but not the client
Check out these links:
* https://github.com/eddy5641/LeagueTGen | League of Legends Tournament Code Gen
* https://github.com/Snowl/LoLEnhancementSuite | Multiple mods for lol by Snowl (Original Creator)
* op.gg | Account data + recording + mmr calc
* http://www.leaguereplays.com/download | Lol replay client [use op.gg (I recommend you to)]
* https://github.com/bladecoding/LoLNotes | See who is on your team + rtmp-s sniffer for lol (Causes lag)

In-Client Screenshots (90-100)% complete
=====================

![Login](http://i.imgur.com/aG5XoQ0.png)
![News Screen](http://i.imgur.com/Y4ZJ0fk.png)
![Match History](http://i.imgur.com/03K5nmC)
![Store](http://i.imgur.com/1Bq26WV.png)

Still-in Progress Screenshots 
=============================

![Chat](http://puu.sh/5FVHS.png)
![Queue](http://i.imgur.com/ZpZiyJ5.png)
![QueuePop](http://i.imgur.com/KAt5KXR.png)
![ChampSelect](http://i.imgur.com/KEZHtw2.jpg)
![ChampSelect2](http://i.imgur.com/FBK2dw3.jpg)
![CustomGameCreate](http://i.imgur.com/vig8S6P.png)
![SearchCustomGames](http://i.imgur.com/1j5Yw8c.png)
![CustomGameLobby](http://i.imgur.com/Z345BSm.png)
![Profile](http://i.imgur.com/BSLpms5.png)
![AggregatedStats](http://puu.sh/5CHtN.jpg)
![Skins](http://i.imgur.com/Lsz3x4d.jpg)
![Settings](http://i.imgur.com/ZTktZTY.png)

Other Features
======

**Super lightweight footprint**

![Footprint](http://i.imgur.com/BAN9o6X.png)

**Multiple chat states:**

![ChatStates](http://i.imgur.com/TY96nl5.png)

**View other teams champions in ARAM**

![AramHack](http://i.imgur.com/9tPrxGy)

Please Note
======
LegendaryClient isn’t endorsed by Riot Games and doesn’t reflect the views or opinions of Riot Games or anyone officially involved in producing or managing League of Legends. League of Legends and Riot Games are trademarks or registered trademarks of Riot Games, Inc. League of Legends © Riot Games, Inc.
