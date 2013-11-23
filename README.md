LegendaryClient
===============

Beautiful, fully-featured League of Legends client based off https://github.com/Snowl/ClientOfLegends2

Still under heavy development! Lots of things don't work or don't look good...

How to compile
===========

** You require Net 4.5 to compile this! **

First, import the DLL's found in /DLLS. This includes sqlite, MahApps.Metro, SharpZipLib, jabber-net, Awesomium and PVPNetConnect.

Next, compile it. It will crash at this point. This will be fixed once the patcher is integrated, since required files aren't downloaded/copied on first run at the moment.

* Find your installation of League of Legends (C:\Riot Games\League of Legends) and copy *"gameStats_en_US.sqlite" to the /bin folder of LegendaryClient. 

* Also, while you are there, copy *"ClientLibCommon.dat" to the /bin folder of LegendaryClient.

* Next, create a folder called Assets.

* Next, find your League of Legends installation folder and navigate to *"RADS\projects\lol_air_client\releases\0.0.1.53\deploy\assets\images"*. Copy the champions folder into your Assets folder. 

* Finally, go to http://ddragon.leagueoflegends.com/tool/ and download the latest dragontail. Open it and export the files in [version]/img into your Assets folder.
 
LegendaryClient should work once this is complete. Your folder should look like this: http://puu.sh/5hXhV.png and http://puu.sh/5hXif.png

In-Client Screenshots (90-100)% complete
=====================

![Login](http://i.imgur.com/KlRS9G2.jpg)
![News Screen](http://i.imgur.com/rpGDhZ1.png)
![Featured Games](http://i.imgur.com/UTuh1e8.png)
![Store](http://i.imgur.com/VvdjTrA.png)

Still-in Progress Screenshots 
=============================

![Queue](http://i.imgur.com/BuDdtyd.png)
![ChampSelect](http://i.imgur.com/mLLLSLJ.png)
![CustomGameCreate](http://i.imgur.com/AZ74Y7L.png)
![SearchCustomGames](http://i.imgur.com/1j5Yw8c.png)
![CustomGameLobby](http://i.imgur.com/YA8JLTa.png)
![Profile](http://i.imgur.com/1Oag2Rf.png)
![Settings](http://i.imgur.com/ZTktZTY.png)
