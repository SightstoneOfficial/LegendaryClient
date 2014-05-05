using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Game.Practice;
using PVPNetConnect.RiotObjects.Platform.Harassment;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Login;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using PVPNetConnect.RiotObjects.Platform.Reroll.Pojo;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Statistics.Team;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Platform.Summoner.Boost;
using PVPNetConnect.RiotObjects.Platform.Summoner.Icon;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Summoner.Runes;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using PVPNetConnect.RiotObjects.Platform.Trade;
using PVPNetConnect.RiotObjects.Team;
using PVPNetConnect.RiotObjects.Team.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PVPNetConnect
{
    public partial class PVPNetConnection
    {
        /// 0.)
        private void Login(AuthenticationCredentials arg0, Session.Callback callback)
        {
            Session cb = new Session(callback);
            InvokeWithCallback("loginService", "login", new object[] { arg0.GetBaseTypedObject() }, cb);
        }

        private async Task<Session> Login(AuthenticationCredentials arg0)
        {
            int Id = Invoke("loginService", "login", new object[] { arg0.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            Session result = new Session(messageBody);
            results.Remove(Id);
            return result;
        }

        public async Task<object> Subscribe(string service, double accountId)
        {
            TypedObject body = WrapBody(new TypedObject(), "messagingDestination", 0);
            body.type = "flex.messaging.messages.CommandMessage";
            TypedObject headers = body.GetTO("headers");
            if (service == "bc")
                headers.Add("DSSubtopic", "bc");
            else
                headers.Add("DSSubtopic", service + "-" + accountID);
            headers.Remove("DSRequestTimeout");
            body["clientId"] = service + "-" + accountID;
            int Id = Invoke(body);
            while (!results.ContainsKey(Id))
                await Task.Delay(10);

            TypedObject result = GetResult(Id); // Read result and discard

            return null;
        }

        /// 1.)
        public void GetLoginDataPacketForUser(LoginDataPacket.Callback callback)
        {
            LoginDataPacket cb = new LoginDataPacket(callback);
            InvokeWithCallback("clientFacadeService", "getLoginDataPacketForUser", new object[] { }, cb);
        }

        public async Task<LoginDataPacket> GetLoginDataPacketForUser()
        {
            int Id = Invoke("clientFacadeService", "getLoginDataPacketForUser", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            LoginDataPacket result = new LoginDataPacket(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 2.)
        public async Task<GameQueueConfig[]> GetAvailableQueues()
        {
            int Id = Invoke("matchmakerService", "getAvailableQueues", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            GameQueueConfig[] result = new GameQueueConfig[results[Id].GetTO("data").GetArray("body").Length];
            for (int i = 0; i < results[Id].GetTO("data").GetArray("body").Length; i++)
            {
                result[i] = new GameQueueConfig((TypedObject)results[Id].GetTO("data").GetArray("body")[i]);
            }
            results.Remove(Id);
            return result;
        }

        /// 3.)
        public void GetSumonerActiveBoosts(SummonerActiveBoostsDTO.Callback callback)
        {
            SummonerActiveBoostsDTO cb = new SummonerActiveBoostsDTO(callback);
            InvokeWithCallback("inventoryService", "getSumonerActiveBoosts", new object[] { }, cb);
        }

        public async Task<SummonerActiveBoostsDTO> GetSumonerActiveBoosts()
        {
            int Id = Invoke("inventoryService", "getSumonerActiveBoosts", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SummonerActiveBoostsDTO result = new SummonerActiveBoostsDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 4.)
        public async Task<ChampionDTO[]> GetAvailableChampions()
        {
            int Id = Invoke("inventoryService", "getAvailableChampions", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            ChampionDTO[] result = new ChampionDTO[results[Id].GetTO("data").GetArray("body").Length];
            for (int i = 0; i < results[Id].GetTO("data").GetArray("body").Length; i++)
            {
                result[i] = new ChampionDTO((TypedObject)results[Id].GetTO("data").GetArray("body")[i]);
            }
            results.Remove(Id);
            return result;
        }

        /// 5.)
        public void GetSummonerRuneInventory(Double summonerId, SummonerRuneInventory.Callback callback)
        {
            SummonerRuneInventory cb = new SummonerRuneInventory(callback);
            InvokeWithCallback("summonerRuneService", "getSummonerRuneInventory", new object[] { summonerId }, cb);
        }

        public async Task<SummonerRuneInventory> GetSummonerRuneInventory(Double summonerId)
        {
            int Id = Invoke("summonerRuneService", "getSummonerRuneInventory", new object[] { summonerId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SummonerRuneInventory result = new SummonerRuneInventory(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 6.)
        public async Task<String> PerformLCDSHeartBeat(Int32 arg0, String arg1, Int32 arg2, String arg3)
        {
            int Id = Invoke("loginService", "performLCDSHeartBeat", new object[] { arg0, arg1, arg2, arg3 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            String result = (String)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 7.)
        public void GetMyLeaguePositions(SummonerLeagueItemsDTO.Callback callback)
        {
            SummonerLeagueItemsDTO cb = new SummonerLeagueItemsDTO(callback);
            InvokeWithCallback("leaguesServiceProxy", "getMyLeaguePositions", new object[] { }, cb);
        }

        public async Task<SummonerLeagueItemsDTO> GetMyLeaguePositions()
        {
            int Id = Invoke("leaguesServiceProxy", "getMyLeaguePositions", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SummonerLeagueItemsDTO result = new SummonerLeagueItemsDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 8.)
        public async Task<object> LoadPreferencesByKey(String arg0, Double arg1, Boolean arg2)
        {
            int Id = Invoke("playerPreferencesService", "loadPreferencesByKey", new object[] { arg0, arg1, arg2 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 9.)
        public void GetMasteryBook(Double summonerId, MasteryBookDTO.Callback callback)
        {
            MasteryBookDTO cb = new MasteryBookDTO(callback);
            InvokeWithCallback("masteryBookService", "getMasteryBook", new object[] { summonerId }, cb);
        }

        public async Task<MasteryBookDTO> GetMasteryBook(Double summonerId)
        {
            int Id = Invoke("masteryBookService", "getMasteryBook", new object[] { summonerId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            MasteryBookDTO result = new MasteryBookDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 10.)
        public void CreatePlayer(PlayerDTO.Callback callback)
        {
            PlayerDTO cb = new PlayerDTO(callback);
            InvokeWithCallback("summonerTeamService", "createPlayer", new object[] { }, cb);
        }

        public async Task<PlayerDTO> CreatePlayer()
        {
            int Id = Invoke("summonerTeamService", "createPlayer", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            PlayerDTO result = new PlayerDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 11.)
        public async Task<String[]> GetSummonerNames(Double[] summonerIds)
        {
            int Id = Invoke("summonerService", "getSummonerNames", new object[] { summonerIds.Cast<object>().ToArray() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            String[] result = new String[results[Id].GetTO("data").GetArray("body").Length];
            for (int i = 0; i < results[Id].GetTO("data").GetArray("body").Length; i++)
            {
                result[i] = (String)results[Id].GetTO("data").GetArray("body")[i];
            }
            results.Remove(Id);
            return result;
        }

        /// 12.)
        public void GetChallengerLeague(String queueType, LeagueListDTO.Callback callback)
        {
            LeagueListDTO cb = new LeagueListDTO(callback);
            InvokeWithCallback("leaguesServiceProxy", "getChallengerLeague", new object[] { queueType }, cb);
        }

        public async Task<LeagueListDTO> GetChallengerLeague(String queueType)
        {
            int Id = Invoke("leaguesServiceProxy", "getChallengerLeague", new object[] { queueType });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            LeagueListDTO result = new LeagueListDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 13.)
        public void GetAllMyLeagues(SummonerLeaguesDTO.Callback callback)
        {
            SummonerLeaguesDTO cb = new SummonerLeaguesDTO(callback);
            InvokeWithCallback("leaguesServiceProxy", "getAllMyLeagues", new object[] { }, cb);
        }

        public async Task<SummonerLeaguesDTO> GetAllMyLeagues()
        {
            int Id = Invoke("leaguesServiceProxy", "getAllMyLeagues", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SummonerLeaguesDTO result = new SummonerLeaguesDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 14.)
        public void GetAllSummonerDataByAccount(Double accountId, AllSummonerData.Callback callback)
        {
            AllSummonerData cb = new AllSummonerData(callback);
            InvokeWithCallback("summonerService", "getAllSummonerDataByAccount", new object[] { accountId }, cb);
        }

        public async Task<AllSummonerData> GetAllSummonerDataByAccount(Double accountId)
        {
            int Id = Invoke("summonerService", "getAllSummonerDataByAccount", new object[] { accountId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            AllSummonerData result = new AllSummonerData(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 15.)
        public void GetPointsBalance(PointSummary.Callback callback)
        {
            PointSummary cb = new PointSummary(callback);
            InvokeWithCallback("lcdsRerollService", "getPointsBalance", new object[] { }, cb);
        }

        public async Task<PointSummary> GetPointsBalance()
        {
            int Id = Invoke("lcdsRerollService", "getPointsBalance", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            PointSummary result = new PointSummary(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 16.)
        public async Task<String> GetSummonerIcons(Double[] summonerIds)
        {
            int Id = Invoke("summonerService", "getSummonerIcons", new object[] { summonerIds.Cast<object>().ToArray() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            String result = (String)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 17.)
        public void CallKudos(String arg0, LcdsResponseString.Callback callback)
        {
            LcdsResponseString cb = new LcdsResponseString(callback);
            InvokeWithCallback("clientFacadeService", "callKudos", new object[] { arg0 }, cb);
        }

        public async Task<LcdsResponseString> CallKudos(String arg0)
        {
            int Id = Invoke("clientFacadeService", "callKudos", new object[] { arg0 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            LcdsResponseString result = new LcdsResponseString(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 18.)
        public void RetrievePlayerStatsByAccountId(Double accountId, String season,
            PlayerLifetimeStats.Callback callback)
        {
            PlayerLifetimeStats cb = new PlayerLifetimeStats(callback);
            InvokeWithCallback("playerStatsService", "retrievePlayerStatsByAccountId", new object[] { accountId, season },
                cb);
        }

        public async Task<PlayerLifetimeStats> RetrievePlayerStatsByAccountId(Double accountId, String season)
        {
            int Id = Invoke("playerStatsService", "retrievePlayerStatsByAccountId", new object[] { accountId, season });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            PlayerLifetimeStats result = new PlayerLifetimeStats(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 19.)
        public async Task<ChampionStatInfo[]> RetrieveTopPlayedChampions(Double accountId, String gameMode)
        {
            int Id = Invoke("playerStatsService", "retrieveTopPlayedChampions", new object[] { accountId, gameMode });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            ChampionStatInfo[] result = new ChampionStatInfo[results[Id].GetTO("data").GetArray("body").Length];
            for (int i = 0; i < results[Id].GetTO("data").GetArray("body").Length; i++)
            {
                result[i] = new ChampionStatInfo((TypedObject)results[Id].GetTO("data").GetArray("body")[i]);
            }
            results.Remove(Id);
            return result;
        }

        /// 20.)
        public void GetSummonerByName(String summonerName, PublicSummoner.Callback callback)
        {
            PublicSummoner cb = new PublicSummoner(callback);
            InvokeWithCallback("summonerService", "getSummonerByName", new object[] { summonerName }, cb);
        }

        public async Task<PublicSummoner> GetSummonerByName(String summonerName)
        {
            int Id = Invoke("summonerService", "getSummonerByName", new object[] { summonerName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            PublicSummoner result = new PublicSummoner(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 21.)
        public void GetAggregatedStats(Double summonerId, String gameMode, String season,
            AggregatedStats.Callback callback)
        {
            AggregatedStats cb = new AggregatedStats(callback);
            InvokeWithCallback("playerStatsService", "getAggregatedStats", new object[] { summonerId, gameMode, season },
                cb);
        }

        public async Task<AggregatedStats> GetAggregatedStats(Double summonerId, String gameMode, String season)
        {
            int Id = Invoke("playerStatsService", "getAggregatedStats", new object[] { summonerId, gameMode, season });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            AggregatedStats result = new AggregatedStats(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 22.)
        public void GetRecentGames(Double accountId, RecentGames.Callback callback)
        {
            RecentGames cb = new RecentGames(callback);
            InvokeWithCallback("playerStatsService", "getRecentGames", new object[] { accountId }, cb);
        }

        public async Task<RecentGames> GetRecentGames(Double accountId)
        {
            int Id = Invoke("playerStatsService", "getRecentGames", new object[] { accountId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            RecentGames result = new RecentGames(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 23.)
        public void FindTeamById(TeamId teamId, TeamDTO.Callback callback)
        {
            TeamDTO cb = new TeamDTO(callback);
            InvokeWithCallback("summonerTeamService", "findTeamById", new object[] { teamId.GetBaseTypedObject() }, cb);
        }

        public async Task<TeamDTO> FindTeamById(TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "findTeamById", new object[] { teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            TeamDTO result = new TeamDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        public void FindTeamByName(string TeamName, TeamDTO.Callback callback)
        {
            TeamDTO cb = new TeamDTO(callback);
            InvokeWithCallback("summonerTeamService", "findTeamByName", new object[] { TeamName }, cb);
        }

        public async Task<TeamDTO> FindTeamById(string TeamName)
        {
            int Id = Invoke("summonerTeamService", "findTeamByName", new object[] { TeamName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            TeamDTO result = new TeamDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 24.)
        public void GetLeaguesForTeam(String teamName, SummonerLeaguesDTO.Callback callback)
        {
            SummonerLeaguesDTO cb = new SummonerLeaguesDTO(callback);
            InvokeWithCallback("leaguesServiceProxy", "getLeaguesForTeam", new object[] { teamName }, cb);
        }

        public async Task<SummonerLeaguesDTO> GetLeaguesForTeam(String teamName)
        {
            int Id = Invoke("leaguesServiceProxy", "getLeaguesForTeam", new object[] { teamName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SummonerLeaguesDTO result = new SummonerLeaguesDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 25.)
        public async Task<TeamAggregatedStatsDTO[]> GetTeamAggregatedStats(TeamId arg0)
        {
            int Id = Invoke("playerStatsService", "getTeamAggregatedStats", new object[] { arg0.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TeamAggregatedStatsDTO[] result =
                new TeamAggregatedStatsDTO[results[Id].GetTO("data").GetArray("body").Length];
            for (int i = 0; i < results[Id].GetTO("data").GetArray("body").Length; i++)
            {
                result[i] = new TeamAggregatedStatsDTO((TypedObject)results[Id].GetTO("data").GetArray("body")[i]);
            }
            results.Remove(Id);
            return result;
        }

        /// 26.)
        public void GetTeamEndOfGameStats(TeamId arg0, Double arg1, EndOfGameStats.Callback callback)
        {
            EndOfGameStats cb = new EndOfGameStats(callback);
            InvokeWithCallback("playerStatsService", "getTeamEndOfGameStats",
                new object[] { arg0.GetBaseTypedObject(), arg1 }, cb);
        }

        public async Task<EndOfGameStats> GetTeamEndOfGameStats(TeamId arg0, Double arg1)
        {
            int Id = Invoke("playerStatsService", "getTeamEndOfGameStats",
                new object[] { arg0.GetBaseTypedObject(), arg1 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            EndOfGameStats result = new EndOfGameStats(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 27.)
        public async Task<object> DisbandTeam(TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "disbandTeam", new object[] { teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 28.)
        public async Task<Boolean> IsNameValidAndAvailable(String teamName)
        {
            int Id = Invoke("summonerTeamService", "isNameValidAndAvailable", new object[] { teamName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            Boolean result = (Boolean)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 29.)
        public async Task<Boolean> IsTagValidAndAvailable(String tagName)
        {
            int Id = Invoke("summonerTeamService", "isTagValidAndAvailable", new object[] { tagName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            Boolean result = (Boolean)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 30.)
        public void CreateTeam(String teamName, String tagName, TeamDTO.Callback callback)
        {
            TeamDTO cb = new TeamDTO(callback);
            InvokeWithCallback("summonerTeamService", "createTeam", new object[] { teamName, tagName }, cb);
        }

        public async Task<TeamDTO> CreateTeam(String teamName, String tagName)
        {
            int Id = Invoke("summonerTeamService", "createTeam", new object[] { teamName, tagName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            TeamDTO result = new TeamDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 31.)
        public void TeamInvitePlayer(Double summonerId, TeamId teamId, TeamDTO.Callback callback)
        {
            TeamDTO cb = new TeamDTO(callback);
            InvokeWithCallback("summonerTeamService", "invitePlayer",
                new object[] { summonerId, teamId.GetBaseTypedObject() }, cb);
        }

        public async Task<TeamDTO> TeamInvitePlayer(Double summonerId, TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "invitePlayer",
                new object[] { summonerId, teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            TeamDTO result = new TeamDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 32.)
        public void KickPlayer(Double summonerId, TeamId teamId, TeamDTO.Callback callback)
        {
            TeamDTO cb = new TeamDTO(callback);
            InvokeWithCallback("summonerTeamService", "kickPlayer",
                new object[] { summonerId, teamId.GetBaseTypedObject() }, cb);
        }

        public async Task<TeamDTO> KickPlayer(Double summonerId, TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "kickPlayer", new object[] { summonerId, teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            TeamDTO result = new TeamDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 33.)
        public void GetAllLeaguesForPlayer(Double summonerId, SummonerLeaguesDTO.Callback callback)
        {
            SummonerLeaguesDTO cb = new SummonerLeaguesDTO(callback);
            InvokeWithCallback("leaguesServiceProxy", "getAllLeaguesForPlayer", new object[] { summonerId }, cb);
        }

        public async Task<SummonerLeaguesDTO> GetAllLeaguesForPlayer(Double summonerId)
        {
            int Id = Invoke("leaguesServiceProxy", "getAllLeaguesForPlayer", new object[] { summonerId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SummonerLeaguesDTO result = new SummonerLeaguesDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 34.)
        public void GetAllPublicSummonerDataByAccount(Double accountId, AllPublicSummonerDataDTO.Callback callback)
        {
            AllPublicSummonerDataDTO cb = new AllPublicSummonerDataDTO(callback);
            InvokeWithCallback("summonerService", "getAllPublicSummonerDataByAccount", new object[] { accountId }, cb);
        }

        public async Task<AllPublicSummonerDataDTO> GetAllPublicSummonerDataByAccount(Double accountId)
        {
            int Id = Invoke("summonerService", "getAllPublicSummonerDataByAccount", new object[] { accountId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            AllPublicSummonerDataDTO result = new AllPublicSummonerDataDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 35.)
        public void FindPlayer(Double summonerId, PlayerDTO.Callback callback)
        {
            PlayerDTO cb = new PlayerDTO(callback);
            InvokeWithCallback("summonerTeamService", "findPlayer", new object[] { summonerId }, cb);
        }

        public async Task<PlayerDTO> FindPlayer(Double summonerId)
        {
            int Id = Invoke("summonerTeamService", "findPlayer", new object[] { summonerId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            PlayerDTO result = new PlayerDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 36.)
        public void GetSpellBook(Double summonerId, SpellBookDTO.Callback callback)
        {
            SpellBookDTO cb = new SpellBookDTO(callback);
            InvokeWithCallback("spellBookService", "getSpellBook", new object[] { summonerId }, cb);
        }

        public async Task<SpellBookDTO> GetSpellBook(Double summonerId)
        {
            int Id = Invoke("spellBookService", "getSpellBook", new object[] { summonerId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SpellBookDTO result = new SpellBookDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 37.)
        public void AttachToQueue(MatchMakerParams matchMakerParams, SearchingForMatchNotification.Callback callback)
        {
            SearchingForMatchNotification cb = new SearchingForMatchNotification(callback);
            InvokeWithCallback("matchmakerService", "attachToQueue",
                new object[] { matchMakerParams.GetBaseTypedObject() }, cb);
        }

        public async Task<SearchingForMatchNotification> AttachToQueue(MatchMakerParams matchMakerParams)
        {
            int Id = Invoke("matchmakerService", "attachToQueue", new object[] { matchMakerParams.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SearchingForMatchNotification result = new SearchingForMatchNotification(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 38.)
        public async Task<Boolean> CancelFromQueueIfPossible(Double summonerId)
        {
            int Id = Invoke("matchmakerService", "cancelFromQueueIfPossible", new object[] { summonerId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            Boolean result = (Boolean)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 39.)
        public async Task<String> GetStoreUrl()
        {
            int Id = Invoke("loginService", "getStoreUrl", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            String result = (String)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 40.)
        public async Task<PracticeGameSearchResult[]> ListAllPracticeGames()
        {
            int Id = Invoke("gameService", "listAllPracticeGames", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            PracticeGameSearchResult[] result =
                new PracticeGameSearchResult[results[Id].GetTO("data").GetArray("body").Length];
            for (int i = 0; i < results[Id].GetTO("data").GetArray("body").Length; i++)
            {
                result[i] = new PracticeGameSearchResult((TypedObject)results[Id].GetTO("data").GetArray("body")[i]);
            }
            results.Remove(Id);
            return result;
        }

        /// 41.)
        ///
        public async Task<object> JoinGame(Double gameId)
        {
            int Id = Invoke("gameService", "joinGame", new object[] { gameId, null });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> JoinGame(Double gameId, string password)
        {
            int Id = Invoke("gameService", "joinGame", new object[] { gameId, password });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> ObserveGame(Double gameId)
        {
            int Id = Invoke("gameService", "observeGame", new object[] { gameId, null });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> ObserveGame(Double gameId, string password)
        {
            int Id = Invoke("gameService", "observeGame", new object[] { gameId, password });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 42.)
        public async Task<String> GetSummonerInternalNameByName(String summonerName)
        {
            int Id = Invoke("summonerService", "getSummonerInternalNameByName", new object[] { summonerName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);

            String result = (String)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 43.)
        public async Task<Boolean> SwitchTeams(Double gameId)
        {
            int Id = Invoke("gameService", "switchTeams", new object[] { gameId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);

            Boolean result = (Boolean)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 44.)
        public async Task<Boolean> SwitchPlayerToObserver(Double gameId)
        {
            int Id = Invoke("gameService", "switchPlayerToObserver", new object[] { gameId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            Boolean result = (Boolean)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 44.)
        public async Task<Boolean> SwitchObserverToPlayer(Double gameId, Int32 team)
        {
            int Id = Invoke("gameService", "switchObserverToPlayer", new object[] { gameId, team });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            Boolean result = (Boolean)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        /// 45.)
        public async Task<object> QuitGame()
        {
            int Id = Invoke("gameService", "quitGame", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 46.)
        public void CreatePracticeGame(PracticeGameConfig practiceGameConfig, GameDTO.Callback callback)
        {
            GameDTO cb = new GameDTO(callback);
            InvokeWithCallback("gameService", "createPracticeGame",
                new object[] { practiceGameConfig.GetBaseTypedObject() }, cb);
        }

        public async Task<GameDTO> CreatePracticeGame(PracticeGameConfig practiceGameConfig)
        {
            int Id = Invoke("gameService", "createPracticeGame", new object[] { practiceGameConfig.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            GameDTO result = new GameDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 47.)
        public async Task<object> SelectBotChampion(Int32 arg0, BotParticipant arg1)
        {
            int Id = Invoke("gameService", "selectBotChampion", new object[] { arg0, arg1.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 48.)
        public async Task<object> RemoveBotChampion(Int32 arg0, BotParticipant arg1)
        {
            int Id = Invoke("gameService", "removeBotChampion", new object[] { arg0, arg1.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 49.)
        public void StartChampionSelection(Double gameId, Double optomisticLock, StartChampSelectDTO.Callback callback)
        {
            StartChampSelectDTO cb = new StartChampSelectDTO(callback);
            InvokeWithCallback("gameService", "startChampionSelection", new object[] { gameId, optomisticLock }, cb);
        }

        public async Task<StartChampSelectDTO> StartChampionSelection(Double gameId, Double optomisticLock)
        {
            int Id = Invoke("gameService", "startChampionSelection", new object[] { gameId, optomisticLock });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            StartChampSelectDTO result = new StartChampSelectDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 50.)
        public async Task<object> SetClientReceivedGameMessage(Double gameId, String arg1)
        {
            int Id = Invoke("gameService", "setClientReceivedGameMessage", new object[] { gameId, arg1 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 51.)
        public void GetLatestGameTimerState(Double arg0, String arg1, Int32 arg2, GameDTO.Callback callback)
        {
            GameDTO cb = new GameDTO(callback);
            InvokeWithCallback("gameService", "getLatestGameTimerState", new object[] { arg0, arg1, arg2 }, cb);
        }

        public async Task<GameDTO> GetLatestGameTimerState(Double arg0, String arg1, Int32 arg2)
        {
            int Id = Invoke("gameService", "getLatestGameTimerState", new object[] { arg0, arg1, arg2 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            GameDTO result = new GameDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 52.)
        public async Task<object> SelectSpells(Int32 spellOneId, Int32 spellTwoId)
        {
            int Id = Invoke("gameService", "selectSpells", new object[] { spellOneId, spellTwoId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 53.)
        public void SelectDefaultSpellBookPage(SpellBookPageDTO spellBookPage, SpellBookPageDTO.Callback callback)
        {
            SpellBookPageDTO cb = new SpellBookPageDTO(callback);
            InvokeWithCallback("spellBookService", "selectDefaultSpellBookPage",
                new object[] { spellBookPage.GetBaseTypedObject() }, cb);
        }

        public async Task<SpellBookPageDTO> SelectDefaultSpellBookPage(SpellBookPageDTO spellBookPage)
        {
            int Id = Invoke("spellBookService", "selectDefaultSpellBookPage",
                new object[] { spellBookPage.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SpellBookPageDTO result = new SpellBookPageDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 54.)
        public async Task<object> SelectChampion(Int32 championId)
        {
            int Id = Invoke("gameService", "selectChampion", new object[] { championId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 55.)
        public async Task<object> SelectChampionSkin(Int32 championId, Int32 skinId)
        {
            int Id = Invoke("gameService", "selectChampionSkin", new object[] { championId, skinId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 56.)
        public async Task<object> ChampionSelectCompleted()
        {
            int Id = Invoke("gameService", "championSelectCompleted", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 57.)
        public async Task<object> SetClientReceivedMaestroMessage(Double arg0, String arg1)
        {
            int Id = Invoke("gameService", "setClientReceivedMaestroMessage", new object[] { arg0, arg1 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /// 58.)
        public void RetrieveInProgressSpectatorGameInfo(String summonerName, PlatformGameLifecycleDTO.Callback callback)
        {
            PlatformGameLifecycleDTO cb = new PlatformGameLifecycleDTO(callback);
            InvokeWithCallback("gameService", "retrieveInProgressSpectatorGameInfo", new object[] { summonerName }, cb);
        }

        public async Task<PlatformGameLifecycleDTO> RetrieveInProgressSpectatorGameInfo(String summonerName)
        {
            int Id = Invoke("gameService", "retrieveInProgressSpectatorGameInfo", new object[] { summonerName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            PlatformGameLifecycleDTO result = new PlatformGameLifecycleDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        /// 59.)
        public async Task<Boolean> DeclineObserverReconnect()
        {
            int Id = Invoke("gameService", "declineObserverReconnect", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            Boolean result = (Boolean)results[Id].GetTO("data")["body"];
            results.Remove(Id);
            return result;
        }

        public async Task<object> AcceptInviteForMatchmakingGame(string gameId)
        {
            int Id = Invoke("matchmakerService", "acceptInviteForMatchmakingGame", new object[] { gameId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> AcceptPoppedGame(bool accept)
        {
            int Id = Invoke("gameService", "acceptPoppedGame", new object[] { accept });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> UpdateProfileIconId(Int32 iconId)
        {
            int Id = Invoke("summonerService", "updateProfileIconId", new object[] { iconId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> BanUserFromGame(double gameId, double accountId)
        {
            int Id = Invoke("gameService", "banUserFromGame", new object[] { gameId, accountId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> BanObserverFromGame(double gameId, double accountId)
        {
            int Id = Invoke("gameService", "banObserverFromGame", new object[] { gameId, accountId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> BanChampion(int championId)
        {
            int Id = Invoke("gameService", "banChampion", new object[] { championId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<ChampionBanInfoDTO[]> GetChampionsForBan()
        {
            int Id = Invoke("gameService", "getChampionsForBan", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            ChampionBanInfoDTO[] result = new ChampionBanInfoDTO[results[Id].GetTO("data").GetArray("body").Length];
            for (int i = 0; i < results[Id].GetTO("data").GetArray("body").Length; i++)
            {
                result[i] = new ChampionBanInfoDTO((TypedObject)results[Id].GetTO("data").GetArray("body")[i]);
            }
            results.Remove(Id);
            return result;
        }

        public async Task<MasteryBookDTO> SaveMasteryBook(MasteryBookDTO masteryBookPage)
        {
            int Id = Invoke("masteryBookService", "saveMasteryBook",
                new object[] { masteryBookPage.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            MasteryBookDTO result = new MasteryBookDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        public async Task<QueueInfo> GetQueueInformation(double queueId)
        {
            int Id = Invoke("matchmakerService", "getQueueInfo", new object[] { queueId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            QueueInfo result = new QueueInfo(messageBody);
            results.Remove(Id);
            return result;
        }

        public async Task<SummonerIconInventoryDTO> GetSummonerIconInventory(double summonerId)
        {
            int Id = Invoke("summonerIconService", "getSummonerIconInventory", new object[] { summonerId });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SummonerIconInventoryDTO result = new SummonerIconInventoryDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        public async Task<PotentialTradersDTO> GetPotentialTraders()
        {
            int Id = Invoke("lcdsChampionTradeService", "getPotentialTraders", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            PotentialTradersDTO result = new PotentialTradersDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        public async Task<object> AttemptTrade(string SummonerInternalName, int ChampionId)
        {
            int Id = Invoke("lcdsChampionTradeService", "attemptTrade", new object[] { SummonerInternalName, ChampionId, false });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> DeclineTrade()
        {
            int Id = Invoke("lcdsChampionTradeService", "dismissTrade", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> AcceptTrade(string SummonerInternalName, int ChampionId)
        {
            int Id = Invoke("lcdsChampionTradeService", "attemptTrade", new object[] { SummonerInternalName, ChampionId, true });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> CreateDefaultSummoner(string SummonerName)
        {
            int Id = Invoke("summonerService", "createDefaultSummoner", new object[] { SummonerName });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        //Todo - get actual data objects
        public async Task<object> GetGameMapList()
        {
            int Id = Invoke("gameMapService", "getGameMapList", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            results.Remove(Id);
            return messageBody;
        }

        //Todo - fix these
        //PlayerPreferences as first param
        public async Task<object> SavePreferences(object arg0)
        {
            int Id = Invoke("playerPreferencesService", "savePreferences", new object[] { arg0 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> SetPreferenceEnabled(String arg0, Boolean arg1)
        {
            int Id = Invoke("playerPreferencesService", "setEnabled", new object[] { arg0, arg1 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> LeaveTeam(int arg0, TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "leaveTeam", new object[] { arg0, teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> DeclineTeamInvite(TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "declineInvite", new object[] { teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> AcceptTeamInvite(TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "joinTeam", new object[] { teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<object> TeamChangeOwner(int arg0, TeamId teamId)
        {
            int Id = Invoke("summonerTeamService", "changeOwner", new object[] { arg0, teamId.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        //gameService tradeChampion exists... not sure if in use though

        public async Task<object> SaveSpellBook(SpellBookDTO Spellbook)
        {
            int Id = Invoke("spellBookService", "saveSpellBook",
                new object[] { Spellbook.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SpellBookDTO result = new SpellBookDTO(messageBody);
            results.Remove(Id);
            return result;
        }

        public async Task<object> CancelFromQueueIfPossible(int arg0)
        {
            int Id = Invoke("matchmakerService", "cancelFromQueueIfPossible", new object[] { arg0 });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        public async Task<SearchingForMatchNotification> AttachTeamToQueue(MatchMakerParams matchMakerParams)
        {
            int Id = Invoke("matchmakerService", "attachTeamToQueue",
                new object[] { matchMakerParams.GetBaseTypedObject() });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            TypedObject messageBody = results[Id].GetTO("data").GetTO("body");
            SearchingForMatchNotification result = new SearchingForMatchNotification(messageBody);
            results.Remove(Id);
            return result;
        }

        public async Task<object> PurgeFromQueues()
        {
            int Id = Invoke("matchmakerService", "purgeFromQueues", new object[] { });
            while (!results.ContainsKey(Id))
                await Task.Delay(10);
            results.Remove(Id);
            return null;
        }

        /*Todo
        * accountService getAccountStateForCurrentSession
        * gameService listAuditInfo
        * inventoryService getAllRuneCombiners
        * inventoryService useRuneCombiner
        * inventoryService useGrabBag
        * inventoryService retrieveInventoryTypes
        * inventoryService giftFacebookFan
        * matchmakerService attachToQueues
        * matchmakerService attachTeamToQueues
        * summonerService getSummonerCatalog
        * summonerService createDefaultSummoner
        * summonerService playerChangeSummonerName
        * summonerService changeTalentRankings
        * summonerService checkSummonerName
        * summonerService resetTalents
        * statisticsService getSummonerSummaryByInternalName
        * statisticsService altSetUserRatings
        * clientFacadeService reportPlayer
        * clientFacadeService callPersistenceMessaging
        * accountManagementService getAccountSecurityQuestion
        * accountManagementService changeAccountInformation
        * accountManagementService resetPassword
        * accountManagementService changePassword
        * accountManagementService changeEmail
        * accountManagementService changePasswordAfterReset
        * loginService logout
        * loginService getLoggedInAccountView
        * lcdsRerollService getPointsBalance
        * lcdsRerollService roll
      */
    }
}