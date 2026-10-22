using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace LivingEmpire
{
    public class EmpireSave
    {
        public int Schema = 2;
        public long LastProcessedDay = -1;
        public int TickCount;
        public List<RegionState> Regions = new List<RegionState>();
        public List<OrganizationState> Organizations = new List<OrganizationState>();
        public List<EmpireEvent> History = new List<EmpireEvent>();
        public List<CountyState> Counties = new List<CountyState>();
        public List<CanonicalFactionState> CanonicalFactions = new List<CanonicalFactionState>();
        public List<CountyInfluence> CountyInfluences = new List<CountyInfluence>();
    }

    public class RegionState
    {
        public int Id;
        public string Name;
        public int Stability;
        public int Treasury;
    }

    public class OrganizationState
    {
        public int Id;
        public int HomeRegionId;
        public int Influence;
        public int Goal;
    }

    public class CountyState { public int Id; public int RegionId; public string Name; public int SovereignOwner; }
    public class CanonicalFactionState { public int FactionId; public int ParentId; public string Name; public int Type; public int Power; public int RawRegion; public string Allies; public string Enemies; }
    public class CountyInfluence { public int FactionId; public int CountyId; public string InfluenceType; public string Evidence; }

    public class EmpireEvent
    {
        public long Day;
        public int RegionId;
        public string Type;
        public string Description;
    }

    public class LivingEmpireMod : MonoBehaviour, IHasModSaveData
    {
        private static Mod mod;
        private EmpireSave world;
        private bool restored;
        private const long SecondsPerDay = 86400;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("Living Empire - Persistence Prototype");
            DontDestroyOnLoad(go);
            var component = go.AddComponent<LivingEmpireMod>();
            mod.SaveDataInterface = component;
            mod.IsReady = true;
            Debug.Log("[LivingEmpire] C# persistence prototype initialized.");
        }

        public Type SaveDataType { get { return typeof(EmpireSave); } }

        public object NewSaveData()
        {
            // DFU calls this for a character/save without existing mod data.
            return MakeInitialWorld();
        }

        public object GetSaveData()
        {
            if (world == null)
                world = MakeInitialWorld();
            return world;
        }

        public void RestoreSaveData(object saveData)
        {
            var loaded = saveData as EmpireSave;
            if (loaded == null || loaded.Schema != 2 || loaded.Regions == null ||
                loaded.Organizations == null || loaded.History == null ||
                loaded.Counties == null || loaded.CanonicalFactions == null || loaded.CountyInfluences == null)
            {
                Debug.LogWarning("[LivingEmpire] Invalid or incompatible save; starting a new world.");
                world = MakeInitialWorld();
            }
            else
            {
                world = loaded;
                Debug.Log("[LivingEmpire] Restored tick=" + world.TickCount +
                          " lastDay=" + world.LastProcessedDay +
                          " regions=" + world.Regions.Count +
                          " organizations=" + world.Organizations.Count);
            }
            restored = true;
        }

        private void Update()
        {
            if (!restored || world == null || DaggerfallUnity.Instance == null ||
                DaggerfallUnity.Instance.WorldTime == null)
                return;

            long day = (long)(DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToSeconds() / SecondsPerDay);
            if (world.LastProcessedDay < 0)
            {
                world.LastProcessedDay = day;
                return;
            }
            if (day < world.LastProcessedDay)
            {
                // Loading an earlier save restores its own clock via RestoreSaveData.
                // For other backward jumps, rebase rather than simulate negative time.
                world.LastProcessedDay = day;
                return;
            }
            // Bounded catch-up prevents a large fast-travel jump from freezing a frame.
            int steps = 0;
            while (world.LastProcessedDay < day && steps < 32)
            {
                world.LastProcessedDay++;
                AdvanceDay(world.LastProcessedDay);
                steps++;
            }
        }

        private void AdvanceDay(long day)
        {
            world.TickCount++;
            // Deterministic, deliberately minimal test mutation.
            // This is NOT the PowerShell political AI port.
            foreach (var region in world.Regions)
            {
                if ((day + region.Id) % 30 == 0)
                    region.Treasury += 1;
            }
            if (day % 30 == 0)
            {
                world.History.Add(new EmpireEvent {
                    Day = day, RegionId = (int)((day / 30) % world.Regions.Count),
                    Type = "simulation_heartbeat",
                    Description = "Monthly simulation checkpoint"
                });
            }
            if (world.TickCount % 30 == 0)
                Debug.Log("[LivingEmpire] Day ticks=" + world.TickCount +
                          " day=" + world.LastProcessedDay +
                          " events=" + world.History.Count);
        }

        private static EmpireSave MakeInitialWorld()
        {
            var data = new EmpireSave();
            for (int i = 0; i < RegionNames.Length; i++)
                data.Regions.Add(new RegionState {
                    Id = i, Name = RegionNames[i], Stability = 60, Treasury = 1000
                });
            for (int i = 0; i < 3000; i++)
                data.Organizations.Add(new OrganizationState {
                    Id = i, HomeRegionId = i % RegionNames.Length,
                    Influence = 10, Goal = i % 6
                });
            for (int i = 0; i < CanonicalData.CountyNames.Length; i++)
                data.Counties.Add(new CountyState { Id=i, Name=CanonicalData.CountyNames[i], RegionId=CanonicalData.CountyRegions[i], SovereignOwner=CanonicalData.CountyOwners[i] });
            for (int i = 0; i < CanonicalData.FactionIds.Length; i++)
                data.CanonicalFactions.Add(new CanonicalFactionState { FactionId=CanonicalData.FactionIds[i], ParentId=CanonicalData.ParentIds[i], Name=CanonicalData.FactionNames[i], Type=CanonicalData.FactionTypes[i], Power=CanonicalData.FactionPowers[i], RawRegion=CanonicalData.FactionRegions[i], Allies=CanonicalData.FactionAllies[i], Enemies=CanonicalData.FactionEnemies[i] });
            for (int i = 0; i < CanonicalData.InfluenceFactionIds.Length; i++)
                data.CountyInfluences.Add(new CountyInfluence { FactionId=CanonicalData.InfluenceFactionIds[i], CountyId=CanonicalData.InfluenceCountyIds[i], InfluenceType=CanonicalData.InfluenceTypes[i], Evidence="provisional-inferred" });
            return data;
        }

        private static readonly string[] RegionNames = new string[] {
            "Alik'r Desert",
            "Dragontail Mountains",
            "Glenpoint Foothills",
            "Daggerfall Bluffs",
            "Yeorth Burrowland",
            "Dwynnen",
            "Ravennian Forest",
            "Devilrock",
            "Malekna Forest",
            "Isle of Balfiera",
            "Bantha",
            "Dak'fron",
            "Islands in the Western Iliac Bay",
            "Tamarilyn Point",
            "Lainlyn Cliffs",
            "Bjoulsae River",
            "Wrothgarian Mountains",
            "Daggerfall",
            "Glenpoint",
            "Betony",
            "Sentinel",
            "Anticlere",
            "Lainlyn",
            "Wayrest",
            "Gen Tem High Rock village",
            "Gen Rai Hammerfell village",
            "Orsinium Area",
            "Skeffington Wood",
            "Hammerfell bay coast",
            "Hammerfell sea coast",
            "High Rock bay coast",
            "High Rock sea coast",
            "Northmoor",
            "Menevia",
            "Alcaire",
            "Koegria",
            "Bhoriane",
            "Kambria",
            "Phrygias",
            "Urvaius",
            "Ykalon",
            "Daenia",
            "Shalgora",
            "Abibon-Gora",
            "Kairou",
            "Pothago",
            "Myrkwasa",
            "Ayasofya",
            "Tigonus",
            "Kozanset",
            "Satakalaam",
            "Totambu",
            "Mournoth",
            "Ephesus",
            "Santaki",
            "Antiphyllos",
            "Bergama",
            "Gavaudon",
            "Tulune",
            "Glenumbra Moors",
            "Ilessan Hills",
            "Cybiades"
        };
    }
}
