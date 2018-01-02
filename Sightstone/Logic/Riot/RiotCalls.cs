﻿using System;
using RtmpSharp.IO;
using System.Reflection;
using System.Linq;
using Sightstone.Logic.Riot.Kudos;
using System.Threading.Tasks;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.Riot.Team;
using Sightstone.Logic.Riot.Leagues;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using RtmpSharp.Messaging;
using Formatting = Newtonsoft.Json.Formatting;
using Sightstone.Logic.MultiUser;
using RtmpSharp.Net;

namespace Sightstone.Logic.Riot
{
    public class RiotCalls
    {
        private UserClient client;
        public RiotCalls(UserClient _client)
        {
            client = _client;
        }
        public delegate void OnInvocationErrorHandler(object sender, Exception error);
        public event OnInvocationErrorHandler OnInvocationError;

        /// <summary>
        /// Login to Riot's servers.
        /// </summary>
        /// <param name="credentials">The credentials for the user</param>
        /// <returns>Session information for the user</returns>
        public Task<Session> Login(AuthenticationCredentials credentials)
        {
            return InvokeAsync<Session>("loginService", "login", credentials);
        }

        public Task<String> Login(string obj)
        {
            return InvokeAsync<String>("auth", "8", obj);
        }

        public Task<object> LoadPreferencesByKey()
        {
            InvokeAsync<String>("playerPreferencesService", "loadPreferencesByKey", "KEY_BINDINGS",
                  double.NaN,
                  false);
            return null;
        }

        public Task<object> CheckLobbyStatus()
        {
            return InvokeAsync<object>("lcdsGameInvitationService", "checkLobbyStatus");
        }

        /// <summary>
        /// Heartbeat to send every 2 minutes.
        /// </summary>
        /// <param name="AccountId">The users id</param>
        /// <param name="SessionToken">The token for the user</param>
        /// <param name="HeartbeatCount">The current amount that heartbeat has been sent</param>
        /// <param name="CurrentTime">The current time in GMT-0700 in format ddd MMM d yyyy HH:mm:ss</param>
        public Task<string> PerformLCDSHeartBeat(int AccountId, string SessionToken, int HeartbeatCount, string CurrentTime)
        {
            return InvokeAsync<string>("loginService", "performLCDSHeartBeat", AccountId, SessionToken, HeartbeatCount, CurrentTime);
        }

        /// <summary>
        /// Gets the store url with token information for the current user.
        /// </summary>
        /// <returns>Returns the store URL</returns>
        public Task<String> GetStoreUrl()
        {
            return InvokeAsync<String>("loginService", "getStoreUrl");
        }

        /// <summary>
        /// Gets the state for the current account
        /// </summary>
        /// <returns>Return the accounts state</returns>
        public Task<String> GetAccountState()
        {
            return InvokeAsync<String>("accountService", "getAccountStateForCurrentSession");
        }

        /// <summary>
        /// Gets the login packet for the user with all the information for the user.
        /// </summary>
        /// <returns>Returns the login data packet</returns>
        public Task<LoginDataPacket> GetLoginDataPacketForUser()
        {
            return InvokeAsync<LoginDataPacket>("clientFacadeService", "getLoginDataPacketForUser");
        }

        /// <summary>
        /// Call kudos (get information or send a kudos).
        /// </summary>
        /// <param name="JSONInformation"></param>
        /// <returns>Json Data about kudos</returns>
        public Task<LcdsResponseString> CallKudos(string JSONInformation)
        {
            return InvokeAsync<LcdsResponseString>("clientFacadeService", "callKudos", JSONInformation);
        }

        /// <summary>
        /// Get the queues that are currently enabled.
        /// </summary>
        /// <returns>Returns an array of queues that are enabled</returns>
        public Task<GameQueueConfig[]> GetAvailableQueues()
        {
            return InvokeAsync<GameQueueConfig[]>("matchmakerService", "getAvailableQueues");
        }

        /// <summary>
        /// Get the current IP & EXP Boosts for the user.
        /// </summary>
        /// <returns>Returns the active boosts for the user</returns>
        public Task<SummonerActiveBoostsDTO> GetSummonerActiveBoosts()
        {
            return InvokeAsync<SummonerActiveBoostsDTO>("inventoryService", "getSumonerActiveBoosts");
        } 
        // Yes this is Sumoner not Summoner

        /// <summary>
        /// Get the current champions for the user.
        /// </summary>
        /// <returns>Returns an array of champions</returns>
        public Task<ChampionDTO[]> GetAvailableChampions()
        {
            return InvokeAsync<ChampionDTO[]>("inventoryService", "getAvailableChampions");
        }

        /// <summary>
        /// Get the runes the user owns.
        /// </summary>
        /// <param name="SummonerId">The summoner ID for the user</param>
        /// <returns>Returns the inventory for the user</returns>
        public Task<SummonerRuneInventory> GetSummonerRuneInventory(double SummonerId)
        {
            return InvokeAsync<SummonerRuneInventory>("summonerRuneService", "getSummonerRuneInventory", SummonerId);
        }

        /// <summary>
        /// Get the current Mastery Book for the user.
        /// </summary>
        /// <param name="SummonerId">The summoner ID for the user</param>
        /// <returns>Returns the mastery books for the user</returns>
        public Task<MasteryBookDTO> GetMasteryBook(double SummonerId)
        {
            return InvokeAsync<MasteryBookDTO>("summonerRuneService", "getSummonerRuneInventory", SummonerId);
        }

        /// <summary>
        /// Gets the runes for a user
        /// </summary>
        /// <param name="SummonerId">The summoner ID for the user</param>
        /// <returns>Returns the rune pages for a user</returns>
        public Task<SpellBookDTO> GetSpellBook(double SummonerId)
        {
            return InvokeAsync<SpellBookDTO>("spellBookService", "getSpellBook", SummonerId);
        }

        /// <summary>
        /// Gets the league positions for the user
        /// </summary>
        /// <returns>Returns the league positions for a user</returns>
        public Task<SummonerLeagueItemsDTO> GetMyLeaguePositions()
        {
            return InvokeAsync<SummonerLeagueItemsDTO>("leaguesServiceProxy", "getMyLeaguePositions");
        }

        /// <summary>
        /// Gets the top 50 players for a queue type
        /// </summary>
        /// <param name="queueType">Queue type</param>
        /// <returns>Returns the top 50 players league info</returns>
        public Task<LeagueListDTO> GetChallengerLeague(string queueType)
        {
            return InvokeAsync<LeagueListDTO>("leaguesServiceProxy", "getChallengerLeague", queueType);
        }

        /// <summary>
        /// Gets the current leagues for a user's tier (e.g Gold)
        /// </summary>
        /// <returns>Returns the leagues for a user</returns>
        public Task<SummonerLeaguesDTO> GetAllMyLeagues()
        {
            return InvokeAsync<SummonerLeaguesDTO>("leaguesServiceProxy", "getAllMyLeagues");
        }

        /// <summary>
        /// Gets the league for a team
        /// </summary>
        /// <param name="TeamName">The team name</param>
        /// <returns>Returns the league information for a team</returns>
        public Task<SummonerLeaguesDTO> GetLeaguesForTeam(string TeamName)
        {
            return InvokeAsync<SummonerLeaguesDTO>("leaguesServiceProxy", "getLeaguesForTeam", TeamName);
        }

        /// <summary>
        /// Get the leagues for a player
        /// </summary>
        /// <param name="SummonerId">The summoner id of the player</param>
        /// <returns>Returns the league information for a team</returns>
        public Task<SummonerLeaguesDTO> GetAllLeaguesForPlayer(double SummonerId)
        {
            return InvokeAsync<SummonerLeaguesDTO>("leaguesServiceProxy", "getAllLeaguesForPlayer", SummonerId);
        }

        /// <summary>
        /// 
        /// </summary>
        public Task<PlayerDTO> CreatePlayer()
        {
            return InvokeAsync<PlayerDTO>("summonerTeamService", "createPlayer");
        }

        /// <summary>
        /// Find a team by the TeamId
        /// </summary>
        /// <param name="TeamId">The team Id</param>
        /// <returns>Returns the information for a team</returns>
        public Task<TeamDTO> FindTeamById(TeamId TeamId)
        {
            return InvokeAsync<TeamDTO>("summonerTeamService", "findTeamById", TeamId);
        }

        /// <summary>
        /// Find a team by name
        /// </summary>
        /// <param name="TeamName">The team name</param>
        /// <returns>Returns the information for a team</returns>
        public Task<TeamDTO> FindTeamByName(string TeamName)
        {
            return InvokeAsync<TeamDTO>("summonerTeamService", "findTeamByName", TeamName);
        }

        /// <summary>
        /// Disbands a team
        /// </summary>
        /// <param name="TeamId">The team Id</param>
        public Task<object> DisbandTeam(TeamId TeamId)
        {
            return InvokeAsync<object>("summonerTeamService", "disbandTeam", TeamId);
        }

        /// <summary>
        /// Checks if a name is available 
        /// </summary>
        /// <param name="TeamName">The name that you want to validate</param>
        /// <returns>Returns a bool as the result</returns>
        public Task<bool> IsTeamNameValidAndAvailable(string TeamName)
        {
            return InvokeAsync<bool>("summonerTeamService", "isNameValidAndAvailable", TeamName);
        }

        /// <summary>
        /// Checks if a tag is available 
        /// </summary>
        /// <param name="TeamName">The tag that you want to validate</param>
        /// <returns>Returns a bool as the result</returns>
        public Task<bool> IsTeamTagValidAndAvailable(string TagName)
        {
            return InvokeAsync<bool>("summonerTeamService", "isTagValidAndAvailable", TagName);
        }
        
        /// <summary>
        /// Creates a ranked team if the name and tag is valid
        /// </summary>
        /// <param name="TeamName">The team name</param>
        /// <param name="TagName">The tag name</param>
        /// <returns>Returns the information for a team</returns>
        public Task<TeamDTO> CreateTeam(string TeamName, string TagName)
        {
            return InvokeAsync<TeamDTO>("summonerTeamService", "createTeam", TeamName, TagName);
        }

        /// <summary>
        /// Invites a player to a ranked team
        /// </summary>
        /// <param name="SummonerId">The summoner id of the player you want to invite</param>
        /// <param name="TeamId">The team id</param>
        /// <returns>Returns the information for a team</returns>
        public Task<TeamDTO> TeamInvitePlayer(double SummonerId, TeamId TeamId)
        {
            return InvokeAsync<TeamDTO>("summonerTeamService", "invitePlayer", SummonerId, TeamId);
        }

        /// <summary>
        /// Kicks a player from a ranked team
        /// </summary>
        /// <param name="SummonerId">The summoner id of the player you want to kick</param>
        /// <param name="TeamId">The team id</param>
        /// <returns>Returns the information for a team</returns>
        public Task<TeamDTO> KickPlayer(double SummonerId, TeamId TeamId)
        {
            return InvokeAsync<TeamDTO>("summonerTeamService", "kickPlayer", SummonerId, TeamId);
        }

        /// <summary>
        /// Finds a player by Summoner Id
        /// </summary>
        /// <param name="SummonerId">The summoner id</param>
        /// <returns>Returns the information for a player</returns>
        public Task<PlayerDTO> FindPlayer(double SummonerId)
        {
            return InvokeAsync<PlayerDTO>("summonerTeamService", "findPlayer", SummonerId);
        }

        /// <summary>
        /// Gets summoner data by account id
        /// </summary>
        /// <param name="AccountId">The account id</param>
        /// <returns>Returns all the summoner data for an account</returns>
        public Task<AllSummonerData> GetAllSummonerDataByAccount(double AccountId)
        {
            return InvokeAsync<AllSummonerData>("summonerService", "getAllSummonerDataByAccount", AccountId);
        }

        /// <summary>
        /// Gets summoner by name
        /// </summary>
        /// <param name="SummonerName">The name of the summoner</param>
        /// <returns>Returns the summoner</returns>
        public Task<PublicSummoner> GetSummonerByName(string SummonerName)
        {
            return InvokeAsync<PublicSummoner>("summonerService", "getSummonerByName", SummonerName);
        }

        /// <summary>
        /// Gets the public summoner data by account id
        /// </summary>
        /// <param name="AccountId">The account id</param>
        /// <returns>Returns all the public summoner data for an account</returns>
        public Task<AllPublicSummonerDataDTO> GetAllPublicSummonerDataByAccount(double AccountId)
        {
            return InvokeAsync<AllPublicSummonerDataDTO>("summonerService", "getAllPublicSummonerDataByAccount", AccountId);
        }

        /// <summary>
        /// Gets the summoner internal name of a summoner
        /// </summary>
        /// <param name="SummonerName">The summoner name</param>
        /// <returns>Returns a summoners internal name</returns>
        public Task<String> GetSummonerInternalNameByName(string SummonerName)
        {
            return InvokeAsync<String>("summonerService", "getSummonerInternalNameByName", SummonerName);
        }

        /// <summary>
        /// Updates the profile icon for the user
        /// </summary>
        /// <param name="IconId">The icon id</param>
        public Task<object> UpdateProfileIconId(int IconId)
        {
            return InvokeAsync<object>("summonerService", "updateProfileIconId", IconId);
        }

        /// <summary>
        /// Get the summoner names for an array of Summoner IDs.
        /// </summary>
        /// <param name="SummonerIds">Array of Summoner IDs</param>
        /// <returns>Returns an array of Summoner Names</returns>
        public Task<String[]> GetSummonerNames(double[] SummonerIds)
        {
            return InvokeAsync<String[]>("summonerService", "getSummonerNames", SummonerIds);
        }

        /// <summary>
        /// Sends a players display name when logging in.
        /// </summary>
        /// <param name="PlayerName">Display name for the summoner</param>
        /// <returns></returns>
        public Task<AllSummonerData> CreateDefaultSummoner(string PlayerName)
        {
            return InvokeAsync<AllSummonerData>("summonerService", "createDefaultSummoner", PlayerName);
        }

        /// <summary>
        /// Sends the skill of the player to the server when initially logging in to seed MMR.
        /// </summary>
        /// <param name="PlayerSkill">The skill of the player</param>
        /// <returns></returns>
        public Task<object> ProcessELOQuestionaire(PlayerSkill PlayerSkill)
        {
            return InvokeAsync<object>("playerStatsService", "processEloQuestionaire", PlayerSkill.ToString());
        }

        /// <summary>
        /// Gets the player reroll balance
        /// </summary>
        /// <returns>Returns the reroll balance for the player</returns>
        public Task<PointSummary> GetPointsBalance()
        {
            return InvokeAsync<PointSummary>("lcdsRerollService", "getPointsBalance");
        }

        /// <summary>
        /// Attempts to reroll the champion. Only works in AllRandomPickStrategy
        /// </summary>
        /// <returns>Returns the amount of rolls left for the player</returns>
        public Task<RollResult> Roll()
        {
            return InvokeAsync<RollResult>("lcdsRerollService", "roll");
        }

        /// <summary>
        /// Gets the players overall stats
        /// </summary>
        /// <param name="AccountId">The account id</param>
        /// <param name="Season">The season you want to retrieve stats from</param>
        /// <returns>Returns the player stats for a season</returns>
        public Task<PlayerLifetimeStats> RetrievePlayerStatsByAccountId(double AccountId, string Season)
        {
            return InvokeAsync<PlayerLifetimeStats>("playerStatsService", "retrievePlayerStatsByAccountId", AccountId, Season);
        }

        /// <summary>
        /// Gets the top 3 played champions for a player
        /// </summary>
        /// <param name="AccountId">The account id</param>
        /// <param name="GameMode">The game mode</param>
        /// <returns>Returns an array of the top 3 champions</returns>
        public Task<ChampionStatInfo[]> RetrieveTopPlayedChampions(double AccountId, string GameMode)
        {
            return InvokeAsync<ChampionStatInfo[]>("playerStatsService", "retrieveTopPlayedChampions", AccountId, GameMode);
        }

        /// <summary>
        /// Gets the aggregated stats of a players ranked games
        /// </summary>
        /// <param name="SummonerId">The summoner id of a player</param>
        /// <param name="GameMode">The game mode requested</param>
        /// <param name="Season">The season you want to retrieve stats from</param>
        /// <returns>Returns the aggregated stats requested</returns>
        public Task<AggregatedStats> GetAggregatedStats(double SummonerId, string GameMode, string Season)
        {
            return InvokeAsync<AggregatedStats>("playerStatsService", "getAggregatedStats", SummonerId, GameMode, Season);
        }

        /// <summary>
        /// Gets the top 10 recent games for a player
        /// </summary>
        /// <param name="AccountId">The account id of a player</param>
        /// <returns>Returns the recent games for a player</returns>
        public Task<RecentGames> GetRecentGames(double AccountId)
        {
            return InvokeAsync<RecentGames>("playerStatsService", "getRecentGames", AccountId);
        }

        /// <summary>
        /// Gets the aggregated stats for a team for all game modes
        /// </summary>
        /// <param name="TeamId">The team id</param>
        /// <returns>Returns an array </returns>
        public Task<TeamAggregatedStatsDTO[]> GetTeamAggregatedStats(TeamId TeamId)
        {
            return InvokeAsync<TeamAggregatedStatsDTO[]>("playerStatsService", "getTeamAggregatedStats", TeamId);
        }

        /// <summary>
        /// Gets the end of game stats for a team for any game
        /// </summary>
        /// <param name="TeamId">The team id</param>
        /// <param name="GameId">The game id</param>
        /// <returns>Returns the end of game stats for a game</returns>
        public Task<EndOfGameStats> GetTeamEndOfGameStats(TeamId TeamId, double GameId)
        {
            return InvokeAsync<EndOfGameStats>("playerStatsService", "getTeamEndOfGameStats", TeamId, GameId);
        }

        /// <summary>
        /// Attaches to a queue
        /// </summary>
        /// <param name="MatchMakerParams">The parameters for the queue</param>
        /// <returns>Returns a notification to tell you if it was successful</returns>
        public Task<SearchingForMatchNotification> AttachToQueue(MatchMakerParams MatchMakerParams)
        {
            return InvokeAsync<SearchingForMatchNotification>("matchmakerService", "attachToQueue", MatchMakerParams);
        }

        /// <summary>
        /// Attaches to a queue with LeaverBusted token
        /// </summary>
        /// <param name="MatchMakerParams">The parameters for the queue</param>
        /// <param name="token">Token required to queue with LeaverBusted</param>
        /// <returns>Returns a notification to tell you if it was successful</returns>
        public Task<SearchingForMatchNotification> AttachToQueue(MatchMakerParams MatchMakerParams, AsObject token)
        {
            return InvokeAsync<SearchingForMatchNotification>("matchmakerService", "attachToQueue", new object[]{ MatchMakerParams, token});
        }

        /// <summary>
        /// Attemps to leave a queue
        /// </summary>
        /// <param name="SummonerId">The users summoner id</param>
        /// <returns>If successfully cancelled returns true, otherwise champion select about to start</returns>
        public Task<bool> CancelFromQueueIfPossible(double SummonerId)
        {
            return InvokeAsync<bool>("matchmakerService", "cancelFromQueueIfPossible", SummonerId);
        }

        /// <summary>
        /// Accepts an invite to a matchmaking game
        /// </summary>
        /// <param name="InviteId">The invite id</param>
        public Task<object> AcceptInviteForMatchmakingGame(string InviteId)
        {
            return InvokeAsync<object>("matchmakerService", "acceptInviteForMatchmakingGame", InviteId);
        }

        /// <summary>
        /// Attaches a premade team to a queue
        /// </summary>
        /// <param name="MatchMakerParams">The parameters for the queue</param>
        /// <returns>Returns a notification to tell you if it was successful</returns>
        public Task<SearchingForMatchNotification> AttachTeamToQueue(MatchMakerParams matchMakerParams)
        {
            return InvokeAsync<SearchingForMatchNotification>("matchmakerService", "attachTeamToQueue", matchMakerParams);
        }

        /// <summary>
        /// Attaches a premade team to a queue with LeaverBusted token
        /// </summary>
        /// <param name="MatchMakerParams">The parameters for the queue</param>
        /// <param name="token">Token required to queue with LeaverBusted</param>
        /// <returns>Returns a notification to tell you if it was successful</returns>
        public Task<SearchingForMatchNotification> AttachTeamToQueue(MatchMakerParams matchMakerParams, AsObject token)
        {
            return InvokeAsync<SearchingForMatchNotification>("matchmakerService", "attachTeamToQueue", matchMakerParams, token);
        }

        /// <summary>
        /// Gets all the practice games
        /// </summary>
        /// <returns>Returns an array of practice games</returns>
        public Task<PracticeGameSearchResult[]> ListAllPracticeGames()
        {
            return InvokeAsync<PracticeGameSearchResult[]>("gameService", "listAllPracticeGames");
        }

        /// <summary>
        /// Joins a game
        /// </summary>
        /// <param name="GameId">The game id the user wants to join</param>
        public Task<object> JoinGame(double GameId)
        {
            return InvokeAsync<object>("gameService", "joinGame", GameId, null);
        }

        /// <summary>
        /// Joins a private game
        /// </summary>
        /// <param name="GameId">The game id the user wants to join</param>
        /// <param name="Password">The password of the game</param>
        /// <returns></returns>
        public Task<object> JoinGame(double GameId, string Password)
        {
            return InvokeAsync<object>("gameService", "joinGame", GameId, Password);
        }

        public Task<object> ObserveGame(double GameId)
        {
            return InvokeAsync<object>("gameService", "observeGame", GameId, null);
        }

        public Task<object> ObserveGame(double GameId, string Password)
        {
            return InvokeAsync<object>("gameService", "observeGame", GameId, Password);
        }

        /// <summary>
        /// Switches the teams in a custom game
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <returns>Returns true if successful</returns>
        public Task<bool> SwitchTeams(double GameId)
        {
            return InvokeAsync<bool>("gameService", "switchTeams", GameId);
        }

        /// <summary>
        /// Switches from a player to spectator
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <returns>Returns true if successful</returns>
        public Task<bool> SwitchPlayerToObserver(double GameId)
        {
            return InvokeAsync<bool>("gameService", "switchPlayerToObserver", GameId );
        }

        /// <summary>
        /// Switches from a spectator to player
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <returns>Returns true if successful</returns>
        public Task<bool> SwitchObserverToPlayer(double GameId, int Team)
        {
            return InvokeAsync<bool>("gameService", "switchObserverToPlayer", GameId, Team);
        }

        /// <summary>
        /// Quits from the current game
        /// </summary>
        public Task<object> QuitGame()
        {
            return InvokeAsync<object>("gameService", "quitGame");
        }

        /// <summary>
        /// Creates a practice game.
        /// </summary>
        /// <param name="Config">The configuration for the practice game</param>
        /// <returns>Returns a GameDTO if successfully created, otherwise null</returns>
        public Task<GameDTO> CreatePracticeGame(PracticeGameConfig Config)
        {
            return InvokeAsync<GameDTO>("gameService", "createPracticeGame", Config);
        }

        /// <summary>
        /// Starts champion selection for a custom game
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <param name="OptomisticLock">The optomistic lock (provided by GameDTO)</param>
        /// <returns>Returns a StartChampSelectDTO</returns>
        public Task<StartChampSelectDTO> StartChampionSelection(double GameId, double OptomisticLock)
        {
            return InvokeAsync<StartChampSelectDTO>("gameService", "startChampionSelection", GameId, OptomisticLock);
        }

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <param name="Argument">The argument to be passed</param>
        public Task<object> SetClientReceivedGameMessage(double GameId, string Argument)
        {
            return InvokeAsync<object>("gameService", "setClientReceivedGameMessage", GameId, Argument);
        }

        /// <summary>
        /// Gets the latest GameDTO for a game
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <param name="GameState">The current game state</param>
        /// <param name="PickTurn">The current pick turn</param>
        /// <returns>Returns the latest GameDTO</returns>
        public Task<GameDTO> GetLatestGameTimerState(double GameId, string GameState, int PickTurn)
        {
            return InvokeAsync<GameDTO>("gameService", "getLatestGameTimerState", GameId, GameState, PickTurn);
        }

        /// <summary>
        /// Selects the spells for a player for the current game
        /// </summary>
        /// <param name="SpellOneId">The spell id for the first spell</param>
        /// <param name="SpellTwoId">The spell id for the second spell</param>
        public Task<object> SelectSpells(int SpellOneId, int SpellTwoId)
        {
            return InvokeAsync<object>("gameService", "selectSpells", SpellOneId, SpellTwoId);
        }

        /// <summary>
        /// Selects a rune page for use
        /// </summary>
        /// <param name="SpellbookPage">The spellbook page the player wants to use</param>
        /// <returns>The selected spellbook page</returns>
        public Task<SpellBookPageDTO> SelectDefaultSpellBookPage(SpellBookPageDTO SpellbookPage)
        {
            return InvokeAsync<SpellBookPageDTO>("spellBookService", "selectDefaultSpellBookPage", SpellbookPage);
        }

        /// <summary>
        /// Saves the players spellbook
        /// </summary>
        /// <param name="Spellbook">The players SpellBookDTO</param>
        public Task<SpellBookDTO> SaveSpellBook(SpellBookDTO Spellbook)
        {
            return InvokeAsync<SpellBookDTO>("spellBookService", "saveSpellBook", Spellbook);
        }

        /// <summary>
        /// Selects a champion for use
        /// </summary>
        /// <param name="ChampionId">The selected champion id</param>
        public Task<object> SelectChampion(int ChampionId)
        {
            return InvokeAsync<object>("gameService", "selectChampion", ChampionId);
        }

        /// <summary>
        /// Selects a champion skin for a champion
        /// </summary>
        /// <param name="ChampionId">The selected champion id</param>
        /// <param name="SkinId">The selected champion skin</param>
        public Task<object> SelectChampionSkin(int ChampionId, int SkinId)
        {
            return InvokeAsync<object>("gameService", "selectChampionSkin", ChampionId, SkinId);
        }

        /// <summary>
        /// Lock in your champion selection
        /// </summary>
        public Task<object> ChampionSelectCompleted()
        {
            return InvokeAsync<object>("gameService", "championSelectCompleted");
        }
        
        /// <summary>
        /// Gets the spectator game info for a summoner
        /// </summary>
        /// <param name="SummonerName">The summoner name</param>
        /// <returns>Returns the game info</returns>
        public Task<PlatformGameLifecycleDTO> RetrieveInProgressSpectatorGameInfo(string SummonerName)
        {
            return InvokeAsync<PlatformGameLifecycleDTO>("gameService", "retrieveInProgressSpectatorGameInfo", SummonerName);
        }

        /// <summary>
        /// Accepts a popped queue
        /// </summary>
        /// <param name="AcceptGame">Accept or decline the queue</param>
        public Task<object> AcceptPoppedGame(bool AcceptGame)
        {
            return InvokeAsync<object>("gameService", "acceptPoppedGame", AcceptGame);
        }

        /// <summary>
        /// Bans a user from a custom game
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <param name="AccountId">The account id of the player</param>
        public Task<object> BanUserFromGame(double GameId, double AccountId)
        {
            return InvokeAsync<object>("gameService", "banUserFromGame", GameId, AccountId);
        }

        /// <summary>
        /// Bans a user from a custom game
        /// </summary>
        /// <param name="GameId">The game id</param>
        /// <param name="AccountId">The account id of the player</param>
        public Task<object> BanObserverFromGame(double GameId, double AccountId)
        {
            return InvokeAsync<object>("gameService", "banObserverFromGame", GameId, AccountId);
        }

        /// <summary>
        /// Bans a champion from the game (must be dUring PRE_CHAMP_SELECT and the users PickTurn)
        /// </summary>
        /// <param name="ChampionId">The champion id</param>
        public Task<object> BanChampion(int ChampionId)
        {
            return InvokeAsync<object>("gameService", "banChampion", ChampionId);
        }

        /// <summary>
        /// Gets the champions from the other team to ban
        /// </summary>
        /// <returns>Returns an array of champions to ban</returns>
        public Task<ChampionBanInfoDTO[]> GetChampionsForBan()
        {
            return InvokeAsync<ChampionBanInfoDTO[]>("gameService", "getChampionsForBan");
        }

        /// <summary>
        /// Saves the mastery book
        /// </summary>
        /// <param name="MasteryBookPage">The mastery book information</param>
        /// <returns>Returns the mastery book</returns>
        public Task<MasteryBookDTO> SaveMasteryBook(MasteryBookDTO MasteryBookPage)
        {
            return InvokeAsync<MasteryBookDTO>("masteryBookService", "saveMasteryBook", MasteryBookPage);
        }

        /// <summary>
        /// Gets the queue information for a selected queue
        /// </summary>
        /// <param name="QueueId">The queue id</param>
        /// <returns>Returns the queue information</returns>
        public Task<QueueInfo> GetQueueInformation(double QueueId)
        {
            return InvokeAsync<QueueInfo>("matchmakerService", "getQueueInfo", QueueId);
        }

        /// <summary>
        /// Gets the summoner icons for a user
        /// </summary>
        /// <param name="SummonerId">The summoner id</param>
        /// <returns>Returns the summoner icons</returns>
        public Task<SummonerIconInventoryDTO> GetSummonerIconInventory(double SummonerId)
        {
            return InvokeAsync<SummonerIconInventoryDTO>("summonerIconService", "getSummonerIconInventory", SummonerId);
        }

        /// <summary>
        /// Gets the allowed traders in the current game
        /// </summary>
        /// <returns>Returns a list of traders</returns>
        public Task<PotentialTradersDTO> GetPotentialTraders()
        {
            return InvokeAsync<PotentialTradersDTO>("lcdsChampionTradeService", "getPotentialTraders");
        }

        /// <summary>
        /// Attempts to trade with a player
        /// </summary>
        /// <param name="SummonerInternalName">The internal name of a summoner</param>
        /// <param name="ChampionId">The champion id requested</param>
        public Task<object> AttemptTrade(string SummonerInternalName, int ChampionId)
        {
            return InvokeAsync<object>("lcdsChampionTradeService", "attemptTrade", SummonerInternalName, ChampionId, false);
        }

        /// <summary>
        /// Decline the current trade
        /// </summary>
        public Task<object> DeclineTrade()
        {
            return InvokeAsync<object>("lcdsChampionTradeService", "dismissTrade");
        }

        /// <summary>
        /// Accepts the current trade
        /// </summary>
        /// <param name="SummonerInternalName">The internal name of a summoner</param>
        /// <param name="ChampionId">The champion id requested</param>
        public Task<object> AcceptTrade(string SummonerInternalName, int ChampionId)
        {
            return InvokeAsync<object>("lcdsChampionTradeService", "attemptTrade", SummonerInternalName, ChampionId, true);
        }

        /// <summary>
        /// Creates a team builder lobby
        /// </summary>
        /// <param name="QueueId">The queue ID for the lobby</param>
        /// <param name="UUID">The generated UUID of the lobby</param>
        /// <returns></returns>
        public Task<LobbyStatus> CreateGroupFinderLobby(int QueueId, string UUID)
        {
            return InvokeAsync<LobbyStatus>("lcdsServiceProxy", "createGroupFinderLobby", QueueId, UUID);
        }

        /// <summary>
        /// Sends a call to the LCDS Service Proxy
        /// </summary>
        /// <param name="UUID">The generated UUID of the group finder lobby</param>
        /// <param name="GameMode">The game mode (usually "cap")</param>
        /// <param name="ProcedureCall">The procedure to call</param>
        /// <param name="Parameters">The parameters to pass in JSON encoded format</param>
        /// <returns></returns>
        public Task<object> CallLCDS(string UUID, string GameMode, string ProcedureCall, string Parameters)
        {
            return InvokeAsync<object>("lcdsServiceProxy", "call", UUID, GameMode, ProcedureCall, Parameters);
        }

        /// <summary>
        /// Accept invite to the game
        /// </summary>
        /// <param name="inviteID">The id of accepted invite</param>
        /// <returns>LobbyStatus</returns>
        public Task<LobbyStatus> AcceptInvite(string inviteID)
        {
            return InvokeAsync<LobbyStatus>("lcdsGameInvitationService", "accept", inviteID);
        }

        /// <summary>
        /// Decline invite to the game
        /// </summary>
        /// <param name="inviteID">The id of declined invite</param>
        /// <returns>null</returns>
        public Task<LobbyStatus> DeclineInvite(string inviteID)
        {
            return InvokeAsync<LobbyStatus>("lcdsGameInvitationService", "decline", inviteID);
        }

        /// <summary>
        /// Leave lobby
        /// </summary>
        /// <returns>null</returns>
        public async Task Leave()
        {
            await InvokeAsync<object>("lcdsGameInvitationService", "leave");
        }

        /// <summary>
        /// Removes player ability to invite players
        /// </summary>
        /// <param name="summonerId">The id of selected summoner</param>
        /// <returns></returns>
        public Task<object> RevokeInvite(double summonerId)
        {
            InvokeAsync<object>("lcdsGameInvitationService", "revokeInvitePrivileges", summonerId);
            return null;
        }

        /// <summary>
        /// Gives player ability to invite players
        /// </summary>
        /// <param name="summonerId">The id of selected summoner</param>
        /// <returns></returns>
        public Task<object> GrantInvite(double summonerId)
        {
            InvokeAsync<object>("lcdsGameInvitationService", "grantInvitePrivileges", summonerId);
            return null;
        }

        /// <summary>
        /// Creates lobby for the team
        /// </summary>
        /// <param name="queueId">The id of selected queue</param>
        /// <returns>LobbyStatus</returns>
        public Task<LobbyStatus> CreateArrangedTeamLobby(double queueId)
        {
            return InvokeAsync<LobbyStatus>("lcdsGameInvitationService", "createArrangedTeamLobby", queueId);
        }

        /// <summary>
        /// Creates lobby for the ranked team
        /// </summary>
        /// <param name="queueId">The id of selected queue</param>
        /// <param name="teamName">The name of the selected team</param>
        /// <returns>LobbyStatus</returns>
        public Task<LobbyStatus> CreateArrangedRankedTeamLobby(double queueId, string teamName)
        {
            return InvokeAsync<LobbyStatus>("lcdsGameInvitationService", "createArrangedRankedTeamLobby", queueId, teamName);
        }

        /// <summary>
        /// Creates team lobby for the bot game
        /// </summary>
        /// <param name="queueId">The id of selected queue</param>
        /// <param name="botLevel">The difficulty of the bots</param>
        /// <returns>LobbyStatus</returns>
        public Task<LobbyStatus> CreateArrangedBotTeamLobby(double queueId, string botLevel)
        {
            return InvokeAsync<LobbyStatus>("lcdsGameInvitationService", "createArrangedBotTeamLobby", queueId, botLevel);
        }

        /// <summary>
        /// Declines invite to the ranked team
        /// </summary>
        /// <param name="teamId">The id of inviting team</param>
        /// <returns></returns>
        public Task<object> DeclineTeamInvite(TeamId teamId)
        {
            return InvokeAsync<object>("summonerTeamService", "leaveTeam", teamId);
        }

        /// <summary>
        /// Accept invite to the ranked team
        /// </summary>
        /// <param name="teamId">The id of inviting team</param>
        /// <returns></returns>
        public Task<object> AcceptTeamInvite(TeamId teamId)
        {
            return InvokeAsync<object>("summonerTeamService", "joinTeam", teamId);
        }

        /// <summary>
        /// Kicks the bot from custom game lobby
        /// </summary>
        /// <param name="champId">The id of kicked bot</param>
        /// <param name="item">The bot info object</param>
        /// <returns></returns>
        public Task<object> RemoveBotChampion(int champId, BotParticipant item)
        {
            return InvokeAsync<object>("gameService", "removeBotChampion", champId, item);
        }

        /// <summary>
        /// Adds the bot from custom game lobby
        /// </summary>
        /// <param name="champId">The id of added bot</param>
        /// <param name="item">The bot info object</param>
        /// <returns></returns>
        public Task<object> SelectBotChampion(int champId, BotParticipant item)
        {
            return InvokeAsync<object>("gameService", "selectBotChampion", champId, item);
        }

        /// <summary>
        /// Leave all queues
        /// </summary>
        /// <returns></returns>
        public Task<object> PurgeFromQueues()
        {
            return InvokeAsync<object>("matchmakerService", "purgeFromQueues");
        }

        public Task<object> LeaveLeaverBuster(string access)
        {
            return InvokeAsync<object>("matchmakerService", "purgeFromQueues", access);
        }

        /// <summary>
        /// Transfers ownership of the Lobby
        /// </summary>
        /// <param name="summonerId">Id of the summoner</param>
        /// <returns></returns>
        public Task<object> MakeOwner(double summonerId)
        {
            return InvokeAsync<object>("lcdsGameInvitationService", "transferOwnership", summonerId);
        }

        /// <summary>
        /// Checks for pending invitation
        /// </summary>
        /// <returns></returns>
        public Task<object[]> GetPendingInvitations()
        {
            return InvokeAsync<object[]>("lcdsGameInvitationService", "getPendingInvitations");
        }

        /// <summary>
        /// Used for accepting messages
        /// </summary>
        /// 
        /// <returns></returns>
        public Task<object> CallPersistenceMessaging(SimpleDialogMessageResponse response)
        {
            return InvokeAsync<object>("lcdsGameInvitationService", "callPersistenceMessaging", response);
        }

        /// <summary>
        /// Used for inviting players to lobby
        /// </summary>
        /// <param name="id">Account id of the summoner</param>
        /// <returns></returns>
        public Task<object> Invite(string id)
        {
            return InvokeAsync<object>("lcdsGameInvitationService", "invite", id);
        }

        /// <summary>
        /// Used for inviting friend of the friend players to lobby
        /// </summary>
        /// <param name="summonerId">Account id of the summoner to invite</param>
        /// <param name="commonFriendId">Account id of the friend in common</param>
        /// <returns></returns>
        public Task<object> InviteFriendOfFriend(double summonerId, double commonFriendId)
        {
            return InvokeAsync<object>("lcdsGameInvitationService", "invite", summonerId, commonFriendId);
        }

        /// <summary>
        /// Used to kick summoner from team lobby
        /// </summary>
        /// <param name="summonerId">Summoner id</param>
        /// <returns></returns>
       public Task<object> Kick(double summonerId)		
        {		
            return InvokeAsync<object>("lcdsGameInvitationService", "kick", summonerId);		
        }

       /// <summary>
       /// Used to invite multiple summoners
       /// </summary>
       /// <param name="summonerId">Summoner ids array</param>
       /// <returns></returns>
       public Task<object> InviteBulk(object[] summonerIds)
       {
           return InvokeAsync<object>("lcdsGameInvitationService", "inviteBulk", summonerIds, "DEFAULT");
       }

        /// <summary>
        /// Used to report player after game
        /// </summary>
       /// <param name="report">All report info</param>
        /// <returns></returns>
       public Task<object> ReportPlayer(HarassmentReport report)		
        {
            return InvokeAsync<object>("clientFacadeService", "reportPlayer", report);		
        }

        public Task<T> InvokeAsync<T>(string destination, string method, params object[] arguments)
        {
            while (client.isConnectedToRTMP == false)
                Task.Delay(100);
            try
            {
                return client.RiotConnection.InvokeAsync<T>("my-rtmps", destination, method, arguments);
            }
            catch (InvocationException e)
            {
                if (OnInvocationError != null)
                    OnInvocationError(null, e);
                return null;
            }
        }
        public string EscapeItem(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return item;
            }
            string str = Regex.Replace(item, "(\\\\*)\"", "$1\\$0");
            return Regex.Replace(str, "^(.*\\s.*?)(\\\\*)$", "\"$1$2$2\"");
        }


        private string node;
        private int numb;
        private string champ;
        private async Task<int> GetCurrentQueuePosition(string LoginQueue)
        {
            int num;
            string str = string.Format("{0}/{1}", LoginQueue + "login-queue/rest/queue/ticker", champ);
            string str1 = await (new WebClient()).DownloadStringTaskAsync(str);
            string item = (string)JObject.Parse(str1)[node];
            if (item != null)
            {
                int num1 = int.Parse(item, NumberStyles.HexNumber);
                num = Math.Max(numb - num1, 0);
            }
            else
            {
                num = 0;
            }
            return num;
        }
        private async Task CancelQueue(string LoginQueue)
        {
            await (new WebClient()).DownloadStringTaskAsync(LoginQueue + "login-queue/rest/queue/cancelQueue");
        }

        private async Task<int> GetCurrentQueuePosition(string champion, string node, int queueIndex, string loginQueue)
        {
            int num;
            string str = string.Format("{0}/{1}", loginQueue + "login-queue/rest/queue/ticker", champion);
            string str1 = await (new WebClient()).DownloadStringTaskAsync(str);
            string item = (string)JObject.Parse(str1)[node];
            if (item != null)
            {
                int num1 = int.Parse(item, NumberStyles.HexNumber);
                num = Math.Max(queueIndex - num1, 0);
            }
            else
            {
                num = 0;
            }
            return num;
        }

        private async Task<string> GetToken(string token, string loginqueue)
        {
            string str;
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(loginqueue + "login-queue/rest/queue/token");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                await streamWriter.WriteAsync(token);
            }
            WebResponse responseAsync = await httpWebRequest.GetResponseAsync();
            using (StreamReader streamReader = new StreamReader(responseAsync.GetResponseStream()))
            {
                JObject jObjects = JObject.Parse(await streamReader.ReadToEndAsync());
                string item = (string)jObjects["status"];
                if (!item.Equals("join", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Format("Unknown login status '{0}'.", item));
                }
                str = jObjects["lqt"].ToString(Formatting.None);
            }
            return str;
        }
        int times;

        public async Task<String> GetRestToken(string username, string password, string loginQueue, string gtoken = null)
        {
            while (true)
            {
                times = 0;
                string token = null;
                //LoginException.ResponseType responseType;
                try
                {
                    var str = string.Format("user={0},password={1}", HttpUtility.UrlEncode(username), HttpUtility.UrlEncode(password));
                    if (client.Garena && gtoken != null)
                    {
                        str = gtoken;
                    }
                    var httpWebRequest = WebRequest.CreateHttp(loginQueue + "login-queue/rest/queue/authenticate");
                    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                    httpWebRequest.Method = "POST";
                    using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
                    {
                        if (client.Garena)
                        {
                            await streamWriter.WriteAsync(string.Concat("payload=8393%20", str));
                        }
                        await streamWriter.WriteAsync(string.Concat("payload=", str));
                    }
                    Stream responseStream;
                    try
                    {
                        responseStream = httpWebRequest.GetResponse().GetResponseStream();
                    }
                    catch (WebException webException)
                    {
                        responseStream = webException.Response.GetResponseStream();
                    }
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        JObject jObjects = JObject.Parse(await streamReader.ReadToEndAsync());
                        foreach (var xd in jObjects)
                        {
                            Client.Log(xd.Key + "+|+" + xd.Value);
                        }
                        Func<string> func = () => jObjects["token"].ToString(); //lqt
                        //string ss;
                        if (client.Garena)
                        {
                            client.UID = jObjects["user"].ToString(Formatting.Indented).Substring(1,jObjects["user"].ToString(Formatting.None).Length-2);
                            client.Gas = jObjects["gasToken"].ToString();
                        }
                        JArray item = (JArray) jObjects["tickers"];
                        if (item != null)
                        {
                            string item1 = (string) jObjects["node"];
                            JToken jTokens = item.FirstOrDefault(t => (string) t["node"] == item1);
                            if (jTokens == null)
                            {
                                //throw new LoginException(LoginException.ResponseType.Unknown, null);
                            }
                            string str1 = (string) jTokens["champ"];
                            int num = (int) jTokens["id"];
                            while (true)
                            {
                                int currentQueuePosition = await GetCurrentQueuePosition(str1, item1, num, loginQueue);
                                int num1 = currentQueuePosition;
                                int num2 = currentQueuePosition;
                                if (num1 <= 0)
                                {
                                    break;
                                }
                                OnQueuePositionChanged(num2);
                                await Task.Delay(2000);
                            }
                            OnQueuePositionChanged(0);
                            token = await GetToken(func(), loginQueue);
                        }
                        else
                        {
                            string item2 = (string) jObjects["status"];
                            string str2 = (string) jObjects["reason"];

                            if (str2 == "invalid_credentials")
                            {
                                return str2;
                            }
                            if (!item2.Equals("login", StringComparison.OrdinalIgnoreCase))
                            {
                                bool flag = item2.Equals("busy", StringComparison.OrdinalIgnoreCase);
                                bool flag1 = item2.Equals("failed", StringComparison.OrdinalIgnoreCase);
                            }
                            token = func();
                        }
                    }
                    return token;
                }
                catch (Exception exception1)
                {
                    Client.Log("Login Failure: " + exception1.Message);
                }
                times++;
                if ((times > 5) && token != null)
                    return token;
            }
        }

        private void OnQueuePositionChanged(int e)
        {
            EventHandler<int> eventHandler = QueuePositionChanged;
            if (eventHandler != null)
            {
                eventHandler(typeof(RiotCalls), e);
            }
        }
        public event EventHandler<int> QueuePositionChanged;


        public string GetIpAddress()
        {
            StringBuilder sb = new StringBuilder();

            WebRequest con = WebRequest.Create("http://ll.leagueoflegends.com/services/connection_info");
            WebResponse response = con.GetResponse();

            int c;
            while ((c = response.GetResponseStream().ReadByte()) != -1)
                sb.Append((char)c);

            con.Abort();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, string> deserializedJSON = serializer.Deserialize<Dictionary<string, string>>(sb.ToString());

            return deserializedJSON["ip_address"];
        }

        public SerializationContext RegisterObjects()
        {
            var context = new SerializationContext();

            Assembly Sightstone = Assembly.GetExecutingAssembly();

            var x = Sightstone.GetTypes().Where(t => string.Equals(t.Namespace, "Sightstone.Logic.Riot.Platform", StringComparison.Ordinal));
            foreach (Type Platform in x)
                context.Register(Platform);

            x = Sightstone.GetTypes().Where(t => string.Equals(t.Namespace, "Sightstone.Logic.Riot.Leagues", StringComparison.Ordinal));
            foreach (Type League in x)
                context.Register(League);

            x = Sightstone.GetTypes().Where(t => string.Equals(t.Namespace, "Sightstone.Logic.Riot.Team", StringComparison.Ordinal));
            foreach (Type Team in x)
                context.Register(Team);

            context.RegisterAlias(typeof(PendingKudosDTO), "com.riotgames.kudos.dto.PendingKudosDTO", true);
            context.RegisterAlias(typeof(Icon), "com.riotgames.platform.summoner.icon.SummonerIcon", true);
            context.RegisterAlias(typeof(StoreAccountBalanceNotification), "com.riotgames.platform.messaging.StoreAccountBalanceNotification", true);
            context.RegisterAlias(typeof(PlayerParticipant), "com.riotgames.platform.reroll.pojo.AramPlayerParticipant", true);
            context.RegisterAlias(typeof(GameDTO), "com.riotgames.platform.game.GameDTO", true);
            context.RegisterAlias(typeof(PlayerCredentialsDto), "com.riotgames.platform.game.PlayerCredentialsDto", true);
            context.RegisterAlias(typeof(InvitationRequest), "com.riotgames.platform.gameinvite.contract.InvitationRequest", true);
            context.RegisterAlias(typeof(Member), "com.riotgames.platform.gameinvite.contract.Member", true);
            context.RegisterAlias(typeof(LcdsServiceProxyResponse), "com.riotgames.platform.serviceproxy.dispatch.LcdsServiceProxyResponse", true);
            context.RegisterAlias(typeof(GameNotification), "com.riotgames.platform.game.message.GameNotification", true);
            context.RegisterAlias(typeof(SearchingForMatchNotification), "com.riotgames.platform.matchmaking.SearchingForMatchNotification", true);
            context.RegisterAlias(typeof(BroadcastNotification), "com.riotgames.platform.broadcast.BroadcastNotification", true);
            context.RegisterAlias(typeof(BroadcastNotification), "com.riotgames.platform.broadcast.BroadcastMessage", true);
            context.RegisterAlias(typeof(SimpleDialogMessageResponse), "com.riotgames.platform.messaging.persistence.SimpleDialogMessage", true);
            context.RegisterAlias(typeof(TradeContractDTO), "com.riotgames.platform.trade.api.contract.TradeContractDTO", true);
            context.RegisterAlias(typeof(EndOfGameStats), "com.riotgames.platform.statistics.EndOfGameStats", true);
            context.RegisterAlias(typeof(HarassmentReport), "com.riotgames.platform.harassment.HarassmentReport", true);
            context.RegisterAlias(typeof(LobbyStatus), "com.riotgames.platform.gameinvite.contract.LobbyStatus", true);
            context.RegisterAlias(typeof(ClientLoginKickNotification), "com.riotgames.platform.messaging.ClientLoginKickNotification", true);

            return context;
        }

        public enum PlayerSkill
        {
            BEGINNER,
            VETERAN,
            RTS_PLAYER
        }
    }
}
