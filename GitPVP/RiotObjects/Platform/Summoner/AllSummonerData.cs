using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{

public class AllSummonerData : RiotGamesObject
{
public override string TypeName
{
get
{
return this.type;
}
}

private string type = "com.riotgames.platform.summoner.AllSummonerData";

public AllSummonerData()
{
}

public AllSummonerData(Callback callback)
{
this.callback = callback;
}

public AllSummonerData(TypedObject result)
{
base.SetFields(this, result);
}

public delegate void Callback(AllSummonerData result);

private Callback callback;

public override void DoCallback(TypedObject result)
{
base.SetFields(this, result);
callback(this);
}

[InternalName("spellBook")]
public SpellBookDTO SpellBook { get; set; }

[InternalName("summonerDefaultSpells")]
public SummonerDefaultSpells SummonerDefaultSpells { get; set; }

[InternalName("summonerTalentsAndPoints")]
public SummonerTalentsAndPoints SummonerTalentsAndPoints { get; set; }

[InternalName("summoner")]
public Summoner Summoner { get; set; }

[InternalName("masteryBook")]
public MasteryBookDTO MasteryBook { get; set; }

[InternalName("summonerLevelAndPoints")]
public SummonerLevelAndPoints SummonerLevelAndPoints { get; set; }

[InternalName("summonerLevel")]
public SummonerLevel SummonerLevel { get; set; }

}
}
