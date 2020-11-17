using Drops.Core;
using System;
using System.Threading.Tasks;

public class DailyMissionManager: IDailyMissionManager
{
    private readonly ILocalDatabase _localDatabase;
    private readonly IRemoteDatabase _remoteDatabase;
    private readonly IRemoteConfig _remoteConfig;

    public DailyMissionManager(ILocalDatabase localDatabase, IRemoteDatabase remoteDatabase, IRemoteConfig remoteConfig)
    {
        _localDatabase = localDatabase;
        _remoteDatabase = remoteDatabase;
        _remoteConfig = remoteConfig;
    }

    public Task SyncDailyMissions() => Task.Run(async () =>
    {
        var version = _remoteConfig.GetInt(Parameter.DailyMissionsVersion);

        // If there's no current daily mission, load it from Firebase
        var localItemsNumber = await _localDatabase.GetDailyMissionsNumber();
        if (localItemsNumber == 0)
        {
            await CreateOrUpdateDailyMissions(version, 0);
            return;
        }

        // If the daily mission version is less than the remote version, update it
        var maxLocalItemsVersion = await _localDatabase.GetMaxDailyMissionVersion();
        if (maxLocalItemsVersion < version)
        {
            var lastCompletedNumber = (await _localDatabase.GetLastCompletedDailyMission())?.DayNumber ?? 0;
            await CreateOrUpdateDailyMissions(version, lastCompletedNumber);
            return;
        }
    });

    public Task<DailyMission> GetCurrentDailyMission() => Task.Run(async () =>
    {
        var currentMission = await _localDatabase.GetDailyMission(DateTime.Today);
        if (currentMission == null)
        {
            currentMission = await _localDatabase.GetFirstNotCompletedDailyMission();
            if (currentMission != null)
            {
                currentMission.Date = DateTime.Today;
                await UpdateDailyMission(currentMission);
            }
        }
        return currentMission;
    });

    public Task UpdateDailyMission(DailyMission mission) => Task.Run(async () =>
    {
        var version = await _localDatabase.GetDailyMissionVersion(mission.DayNumber);
        await _localDatabase.UpdateDailyMission(mission, version);
    });

    private async Task CreateOrUpdateDailyMissions(int version, int lastCompletedNumber)
    {
        var dailyMissions = await _remoteDatabase.GetDailyMissions(lastCompletedNumber + 1);
        await _localDatabase.UpdateDailyMissions(dailyMissions, version);
    }
}
