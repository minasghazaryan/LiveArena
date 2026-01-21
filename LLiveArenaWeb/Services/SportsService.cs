using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class SportsService : ISportsService
{
    private readonly List<Sport> _sports;

    public SportsService()
    {
        // Initialize with the new sports data structure
        _sports = new List<Sport>
        {
            new Sport { Id = "4", Name = "Cricket", ChildNode = "getcompetition/4", Detail = "sportsbyid/4" },
            new Sport { Id = "1", Name = "Soccer", ChildNode = "getcompetition/1", Detail = "sportsbyid/1" },
            new Sport { Id = "2", Name = "Tennis", ChildNode = "getcompetition/2", Detail = "sportsbyid/2" },
            new Sport { Id = "6423", Name = "American Football", ChildNode = "getcompetition/6423", Detail = "sportsbyid/6423" },
            new Sport { Id = "7522", Name = "Basketball", ChildNode = "getcompetition/7522", Detail = "sportsbyid/7522" },
            new Sport { Id = "6", Name = "Boxing", ChildNode = "getcompetition/6", Detail = "sportsbyid/6" },
            new Sport { Id = "3503", Name = "Darts", ChildNode = "getcompetition/3503", Detail = "sportsbyid/3503" },
            new Sport { Id = "3", Name = "Golf", ChildNode = "getcompetition/3", Detail = "sportsbyid/3" },
            new Sport { Id = "4339", Name = "Greyhound Racing", ChildNode = "getcountries/4339", Detail = "raceschedule/4339/ALL" },
            new Sport { Id = "15", Name = "Greyhound Todays Card", ChildNode = "todayraces/4339", Detail = "raceschedule/4339/today" },
            new Sport { Id = "13", Name = "Horse Race Todays Card", ChildNode = "todayraces/7", Detail = "raceschedule/7/today" },
            new Sport { Id = "7", Name = "Horse Racing", ChildNode = "getcountries/7", Detail = "raceschedule/7/ALL" },
            new Sport { Id = "26420387", Name = "Mixed Martial Arts", ChildNode = "getcompetition/26420387", Detail = "sportsbyid/26420387" },
            new Sport { Id = "8", Name = "Motor Sport", ChildNode = "getcompetition/8", Detail = "sportsbyid/8" },
            new Sport { Id = "2378961", Name = "Politics", ChildNode = "getcompetition/2378961", Detail = "marketdetail/1.176878927" },
            new Sport { Id = "1477", Name = "Rugby League", ChildNode = "getcompetition/1477", Detail = "sportsbyid/1477" },
            new Sport { Id = "5", Name = "Rugby Union", ChildNode = "getcompetition/5", Detail = "sportsbyid/5" },
            new Sport { Id = "6422", Name = "Snooker", ChildNode = "getcompetition/6422", Detail = "sportsbyid/6422" }
        };
    }

    public Task<IEnumerable<Sport>> GetAllSportsAsync()
    {
        return Task.FromResult<IEnumerable<Sport>>(_sports);
    }

    public Task<Sport?> GetSportByIdAsync(string id)
    {
        var sport = _sports.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(sport);
    }

    public Task<Sport?> GetSportByNameAsync(string name)
    {
        var sport = _sports.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(sport);
    }
}
