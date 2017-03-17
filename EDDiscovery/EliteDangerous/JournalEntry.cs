﻿/*
 * Copyright © 2016 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using EDDiscovery.DB;
using EDDiscovery.EliteDangerous;
using EDDiscovery.EliteDangerous.JournalEvents;
using EDDiscovery2;
using EDDiscovery2.DB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EDDiscovery.EliteDangerous
{
    public enum JournalTypeEnum
    {
        Unknown = 0,

        ApproachSettlement = 5,
        Bounty = 10,
        BuyAmmo = 20,
        BuyDrones = 30,
        BuyExplorationData = 40,
        BuyTradeData = 50,
        CapShipBond = 60,
        Cargo = 63,
        ChangeCrewRole = 65,
        ClearSavedGame = 70,
        CockpitBreached = 80,
        CollectCargo = 90,
        CommitCrime = 100,
        CommunityGoalJoin = 110,
        CommunityGoalReward = 120,
        CommunityGoalDiscard = 1040,
        Continued = 125,
        CrewAssign = 126,
        CrewFire = 127,
        CrewHire = 128,
        CrewMemberJoins = 1270,
        CrewMemberQuits = 1280,
        DataScanned = 1030,
        DatalinkScan = 130,
        DatalinkVoucher = 1020,
        Died = 140,
        Docked = 145,
        DockFighter = 150,
        DockingCancelled = 160,
        DockingDenied = 170,
        DockingGranted = 180,
        DockingRequested = 190,
        DockingTimeout = 200,
        DockSRV = 210,
        EjectCargo = 220,
        EngineerApply = 230,
        EngineerCraft = 240,
        EngineerProgress = 250,
        EscapeInterdiction = 260,
        FactionKillBond = 270,
        FetchRemoteModule = 1000,
        FSDJump = 280,
        FuelScoop = 290,
        Fileheader = 300,
        HeatDamage = 310,
        HeatWarning = 320,
        HullDamage = 330,
        Interdicted = 340,
        Interdiction = 350,
        JetConeBoost = 354,
        JetConeDamage = 355,
        JoinACrew = 356,
        KickCrewMember = 357,
        LaunchFighter = 360,
        LaunchSRV = 370,
        Liftoff = 380,
        LoadGame = 390,
        Loadout = 395,
        Location = 400,
        MassModuleStore = 1010,
        MarketBuy = 410,
        MarketSell = 420,
        MaterialCollected = 430,
        MaterialDiscarded = 440,
        MaterialDiscovered = 450,
        Materials = 455,
        MiningRefined = 460,
        MissionAbandoned = 470,
        MissionAccepted = 480,
        MissionCompleted = 490,
        MissionFailed = 500,
        ModuleBuy = 510,
        ModuleRetrieve = 515,
        ModuleSell = 520,
        ModuleSellRemote = 990,
        ModuleStore = 525,
        ModuleSwap = 530,
        NewCommander = 540,
        Passengers = 545,
        PayFines = 550,
        PayLegacyFines = 560,
        PowerplayCollect = 570,
        PowerplayDefect = 580,
        PowerplayDeliver = 590,
        PowerplayFastTrack = 600,
        PowerplayJoin = 610,
        PowerplayLeave = 620,
        PowerplaySalary = 630,
        PowerplayVote = 640,
        PowerplayVoucher = 650,
        Progress = 660,
        Promotion = 670,
        PVPKill = 675,
        QuitACrew = 677,
        Rank = 680,
        RebootRepair = 690,
        ReceiveText = 700,
        RedeemVoucher = 710,
        RefuelAll = 720,
        RefuelPartial = 730,
        Repair = 740,
        RepairAll = 745,
        RestockVehicle = 750,
        Resurrect = 760,
        Scan = 770,
        Scanned = 772,
        ScientificResearch = 775,
        Screenshot = 780,
        SelfDestruct = 790,
        SellDrones = 800,
        SellExplorationData = 810,
        SendText = 820,
        SetUserShipName = 825,
        ShieldState = 830,
        ShipyardBuy = 840,
        ShipyardNew = 850,
        ShipyardSell = 860,
        ShipyardSwap = 870,
        ShipyardTransfer = 880,
        StartJump = 885,
        SupercruiseEntry = 890,
        SupercruiseExit = 900,
        Synthesis = 910,
        Touchdown = 920,
        Undocked = 930,
        USSDrop = 940,
        VehicleSwitch = 950,
        WingAdd = 960,
        WingJoin = 970,
        WingLeave = 980,

        EDDItemSet = 2000,

    }

    public enum CombatRank
    {
        Harmless = 0,
        MostlyHarmless,
        Novice,
        Competent,
        Expert,
        Master,
        Dangerous,
        Deadly,
        Elite
    }

    public enum TradeRank
    {
        Penniless = 0,
        MostlyPenniless,
        Peddler,
        Dealer,
        Merchant,
        Broker,
        Entrepreneur,
        Tycoon,
        Elite
    }

    public enum ExplorationRank
    {
        Aimless = 0,
        MostlyAimless,
        Scout,
        Surveyor,
        Explorer,
        Pathfinder,
        Ranger,
        Pioneer,
        Elite
    }

    public enum FederationRank
    {
        None = 0,
        Recruit,
        Cadet,
        Midshipman,
        PettyOfficer,
        ChiefPettyOfficer,
        WarrantOfficer,
        Ensign,
        Lieutenant,
        LtCommander,
        PostCommander,
        PostCaptain,
        RearAdmiral,
        ViceAdmiral,
        Admiral
    }

    public enum EmpireRank
    {
        None = 0,
        Outsider,
        Serf,
        Master,
        Squire,
        Knight,
        Lord,
        Baron,
        Viscount,
        Count,
        Earl,
        Marquis,
        Duke,
        Prince,
        King
    }

    public enum CQCRank
    {
        Helpless = 0,
        MostlyHelpless,
        Amateur,
        SemiProfessional,
        Professional,
        Champion,
        Hero,
        Legend,
        Elite
    }


    public enum SyncFlags
    {
        EDSM = 0x01,
        EDDN = 0x02,
        StartMarker = 0x0100,           // measure distance start pos marker
        StopMarker = 0x0200,            // measure distance stop pos marker
    };

    [DebuggerDisplay("Event {EventTypeStr} {EventTimeUTC} EdsmID {EdsmID} JID {Id} C {CommanderId}")]
    public abstract class JournalEntry
    {
        #region Instance properties and fields
        public long Id;                          // this is the entry ID
        public long TLUId;                       // this ID of the journal tlu (aka TravelLogId)
        public int CommanderId;                 // commander Id of entry

        public string EventTypeStr;             // name of event. these two duplicate each other, string if for debuggin in the db view of a browser
        public JournalTypeEnum EventTypeID;

        public DateTime EventTimeUTC;

        public long EdsmID;                      // 0 = unassigned, >0 = assigned

        public int Synced;                     // sync flags

        public DateTime EventTimeLocal { get { return EventTimeUTC.ToLocalTime(); } }

        public bool SyncedEDSM { get { return (Synced & (int)SyncFlags.EDSM) != 0; } }
        public bool SyncedEDDN { get { return (Synced & (int)SyncFlags.EDDN) != 0; } }
        public bool StartMarker { get { return (Synced & (int)SyncFlags.StartMarker) != 0; } }
        public bool StopMarker { get { return (Synced & (int)SyncFlags.StopMarker) != 0; } }
        #endregion

        #region Static properties and fields
        private static Dictionary<JournalTypeEnum, Type> JournalEntryTypes = GetJournalEntryTypes();

        /// <summary>
        /// Gets the mapping of journal type value to JournalEntry type
        /// </summary>
        /// <returns>Map of type values to types</returns>
        private static Dictionary<JournalTypeEnum, Type> GetJournalEntryTypes()
        {
            Dictionary<JournalTypeEnum, Type> typedict = new Dictionary<JournalTypeEnum, Type>();
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var types = asm.GetTypes().Where(t => typeof(JournalEntry).IsAssignableFrom(t) && !t.IsAbstract).ToList();

            foreach (Type type in types)
            {
                JournalEntryTypeAttribute typeattrib = type.GetCustomAttributes(false).OfType<JournalEntryTypeAttribute>().FirstOrDefault();
                if (typeattrib != null)
                {
                    typedict[typeattrib.EntryType] = type;
                }
            }

            return typedict;
        }

        #endregion

        #region Formatting control and Icons

        public abstract System.Drawing.Bitmap Icon { get; }

        public abstract void FillInformation(out string summary, out string info, out string detailed);

        #endregion

        #region Creation

        public JournalEntry(JObject jo, JournalTypeEnum jtype)
        {
            EventTypeID = jtype;
            EventTypeStr = jtype.ToString();
            EventTimeUTC = DateTime.Parse(jo.Value<string>("timestamp"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            TLUId = 0;
        }

        public static JournalEntry CreateJournalEntry(JObject jo)
        {
            string Eventstr = JSONHelper.GetStringNull(jo["event"]);

            if (Eventstr == null)  // Should normaly not happend unless corrupt string.
                return null;

            Type jtype = TypeOfJournalEntry(Eventstr);

            if (jtype == null)
            {
                System.Diagnostics.Trace.WriteLine("Unknown event: " + Eventstr);
                return new JournalUnknown(jo, Eventstr);
            }
            else
                return (JournalEntry)Activator.CreateInstance(jtype, jo);
        }

        static public JournalEntry CreateJournalEntry(DataRow dr, JObject jo)
        {
            JournalEntry jr = JournalEntry.CreateJournalEntry(jo);     // this sets EventTypeId, EventTypeStr and UTC via constructor above.. 

            jr.Id = (int)(long)dr["Id"];
            jr.TLUId = (int)(long)dr["TravelLogId"];
            jr.CommanderId = (int)(long)dr["CommanderId"];
            jr.EventTimeUTC = (DateTime)dr["EventTime"];
            jr.EventTypeID = (JournalTypeEnum)(long)dr["eventTypeID"];
            jr.EdsmID = (long)dr["EdsmID"];
            jr.Synced = (int)(long)dr["Synced"];
            return jr;
        }

        static public JournalEntry CreateJournalEntry(DbDataReader dr, JObject jo)
        {
            JournalEntry jr = JournalEntry.CreateJournalEntry(jo);

            jr.Id = (int)(long)dr["Id"];
            jr.TLUId = (int)(long)dr["TravelLogId"];
            jr.CommanderId = (int)(long)dr["CommanderId"];
            if (jr.EventTimeUTC == default(DateTime))
                jr.EventTimeUTC = ((DateTime)dr["EventTime"]).ToUniversalTime();
            jr.EventTypeID = (JournalTypeEnum)(long)dr["eventTypeID"];
            jr.EdsmID = (long)dr["EdsmID"];
            jr.Synced = (int)(long)dr["Synced"];
            return jr;
        }

        static public Type TypeOfJournalEntry(string text)
        {
            //foreach (JournalTypeEnum jte in Enum.GetValues(typeof(JournalTypeEnum))) // check code only to make sure names match
            //{
            //Type p = Type.GetType("EDDiscovery.EliteDangerous.JournalEvents.Journal" + jte.ToString());
            //Debug.Assert(p != null);
            //}

            Type t = Type.GetType("EDDiscovery.EliteDangerous.JournalEvents.Journal" + text, false, true); // no exception, ignore case here
            return t;
        }

        static public Type TypeOfJournalEntry(JournalTypeEnum type)
        {
            if (JournalEntryTypes.ContainsKey(type))
            {
                return JournalEntryTypes[type];
            }
            else
            {
                return TypeOfJournalEntry(type.ToString());
            }
        }

        #endregion

        #region Misc

        static public JournalTypeEnum JournalString2Type(string str)
        {
            foreach (JournalTypeEnum mat in Enum.GetValues(typeof(JournalTypeEnum)))
            {
                if (str.ToLower().Equals(mat.ToString().ToLower()))
                    return mat;
            }

            return JournalTypeEnum.Unknown;
        }

        #endregion

        public static void UpdateEDSMIDPosJump(long journalid, ISystem system, bool jsonpos, double dist, SQLiteConnectionUser cn = null, DbTransaction tn = null)
        {

        }

        public static void UpdateSyncFlagBit(long journalid, SyncFlags bit, bool value)
        {
        }

        public static void UpdateCommanderID(long journalid, int cmdrid)
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set CommanderID = @cmdrid where ID=@journalid"))
                {
                    cmd.AddParameterWithValue("@journalid", journalid);
                    cmd.AddParameterWithValue("@cmdrid", cmdrid);
                    System.Diagnostics.Trace.WriteLine(string.Format("Update cmdr id ID {0} with map colour", journalid));
                    SQLiteDBClass.SQLNonQueryText(cn, cmd);
                }
            }
        }

        public static long AddEDDItemSet(int cmdrid, DateTime dt, long jidofitemset, List<MaterialCommodities> changelist)     // add item, return journal ID
        {
            return 0;
        }

        static public List<string> GetListOfEventsWithOptMethod(bool towords, string method = null, string method2 = null)
        {
            List<string> ret = new List<string>();

            foreach (JournalTypeEnum jte in Enum.GetValues(typeof(JournalTypeEnum)))
            {
                string n = jte.ToString();

                if (method == null)
                {
                    ret.Add((towords) ? n.SplitCapsWord() : n);
                }
                else
                {
                    Type jtype = TypeOfJournalEntry(jte);

                    if (jtype != null)      // may be null, Unknown for instance
                    {
                        System.Reflection.MethodInfo m = jtype.GetMethod(method);

                        if (m == null && method2 != null)
                            m = jtype.GetMethod(method2);

                        if (m != null)
                            ret.Add((towords) ? n.SplitCapsWord() : n);
                    }
                }
            }

            return ret;
        }


    }

    public class JournalEntryDB
    {
        public JournalEntry je;

        public JObject jEventData { get; protected set; }       // event objects
        public string EventDataString { get { return jEventData.ToString(); } }     // Get only, functions will modify them to add additional data on

        public JournalEntryDB(JournalEntry jep, JObject jo )
        {
            je = jep;
            jEventData = jo;
        }

        public static JournalEntryDB Create(string s )
        {
            JObject jo = (JObject)JObject.Parse(s);
            return new JournalEntryDB(JournalEntry.CreateJournalEntry(jo), jo);
        }

        static JournalEntryDB Create(DbDataReader dr)
        {
            JObject jo = (JObject)JObject.Parse((string)dr["EventData"]);
            return new JournalEntryDB(JournalEntry.CreateJournalEntry(dr, jo), jo);
        }

        static JournalEntryDB Create(DataRow dr)
        {
            JObject jo = (JObject)JObject.Parse((string)dr["EventData"]);
            return new JournalEntryDB(JournalEntry.CreateJournalEntry(dr, jo), jo);
        }

        public static JournalEntryDB CreateFSDJournalEntry(long tluid, int cmdrid, DateTime utc, string name, double x, double y, double z, int mc, int syncflag)
        {

            JObject jo = new JObject();
            jo["timestamp"] = utc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
            jo["event"] = "FSDJump";
            jo["StarSystem"] = name;
            jo["StarPos"] = new JArray(x, y, z);
            jo["EDDMapColor"] = mc;
            JournalEntryDB jdb = new JournalEntryDB(JournalEntry.CreateJournalEntry(jo), jo);
            jdb.je.TLUId = tluid;
            jdb.je.CommanderId = cmdrid;
            jdb.je.Synced = syncflag;
            return jdb;
        }

        public bool Add()
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                bool ret = Add(cn);
                return ret;
            }
        }

        public bool Add(SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            using (DbCommand cmd = cn.CreateCommand("Insert into JournalEntries (EventTime, TravelLogID, CommanderId, EventTypeId , EventType, EventData, EdsmId, Synced) values (@EventTime, @TravelLogID, @CommanderID, @EventTypeId , @EventStrName, @EventData, @EdsmId, @Synced)", tn))
            {
                cmd.AddParameterWithValue("@EventTime", je.EventTimeUTC);           // MUST use UTC connection
                cmd.AddParameterWithValue("@TravelLogID", je.TLUId);
                cmd.AddParameterWithValue("@CommanderID", je.CommanderId);
                cmd.AddParameterWithValue("@EventTypeId", je.EventTypeID);
                cmd.AddParameterWithValue("@EventStrName", je.EventTypeStr);
                cmd.AddParameterWithValue("@EventData", EventDataString);
                cmd.AddParameterWithValue("@EdsmId", je.EdsmID);
                cmd.AddParameterWithValue("@Synced", je.Synced);

                SQLiteDBClass.SQLNonQueryText(cn, cmd);

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from JournalEntries"))
                {
                    je.Id = (int)(long)SQLiteDBClass.SQLScalar(cn, cmd2);
                }
                return true;
            }
        }

        public bool Update()
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                return Update(cn);
            }
        }

        private bool Update(SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set EventTime=@EventTime, TravelLogID=@TravelLogID, CommanderID=@CommanderID, EventTypeId=@EventTypeId, EventType=@EventStrName, EventData=@EventData, EdsmId=@EdsmId, Synced=@Synced where ID=@id", tn))
            {
                cmd.AddParameterWithValue("@ID", je.Id);
                cmd.AddParameterWithValue("@EventTime", je.EventTimeUTC);  // MUST use UTC connection
                cmd.AddParameterWithValue("@TravelLogID", je.TLUId);
                cmd.AddParameterWithValue("@CommanderID", je.CommanderId);
                cmd.AddParameterWithValue("@EventTypeId", je.EventTypeID);
                cmd.AddParameterWithValue("@EventStrName", je.EventTypeStr);
                cmd.AddParameterWithValue("@EventData", EventDataString);
                cmd.AddParameterWithValue("@EdsmId", je.EdsmID);
                cmd.AddParameterWithValue("@Synced", je.Synced);
                SQLiteDBClass.SQLNonQueryText(cn, cmd);

                return true;
            }
        }

        static public void Delete(long idvalue)
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser())
            {
                Delete(idvalue, cn);
            }
        }

        static public void Delete(long idvalue, SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("DELETE FROM JournalEntries WHERE id = @id"))
            {
                cmd.AddParameterWithValue("@id", idvalue);
                SQLiteDBClass.SQLNonQueryText(cn, cmd);
            }
        }


        static public JournalEntryDB GetDB(long journalid)
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                return GetDB(journalid, cn);
            }
        }

        static public JournalEntryDB GetDB(long journalid, SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            using (DbCommand cmd = cn.CreateCommand("select * from JournalEntries where ID=@journalid", tn))
            {
                cmd.AddParameterWithValue("@journalid", journalid);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Create(reader);
                    }
                }
            }

            return null;
        }

        static public List<JournalEntry> GetAll(int commander = -999)
        {
            List<JournalEntry> list = new List<JournalEntry>();

            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("select * from JournalEntries where CommanderID=@commander Order by EventTime ASC"))
                {
                    if (commander == -999)
                        cmd.CommandText = "select * from JournalEntries Order by EventTime ";

                    cmd.AddParameterWithValue("@commander", commander);

                    DataSet ds = SQLiteDBClass.SQLQueryText(cn, cmd);

                    if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                        return list;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        JObject jo = (JObject)JObject.Parse((string)dr["EventData"]);
                        list.Add(JournalEntry.CreateJournalEntry(dr,jo));
                    }

                    return list;
                }
            }
        }


        public static List<JournalEntry> GetByEventType(JournalTypeEnum eventtype, int commanderid, DateTime start, DateTime stop)
        {
            List<JournalEntry> vsc = new List<JournalEntry>();

            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE EventTypeID = @eventtype and  CommanderID=@commander and  EventTime >=@start and EventTime<=@Stop ORDER BY EventTime ASC"))
                {
                    cmd.AddParameterWithValue("@eventtype", (int)eventtype);
                    cmd.AddParameterWithValue("@commander", (int)commanderid);
                    cmd.AddParameterWithValue("@start", start);
                    cmd.AddParameterWithValue("@stop", stop);
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JObject jo = (JObject)JObject.Parse((string)reader["EventData"]);
                            vsc.Add(JournalEntry.CreateJournalEntry(reader, jo));
                        }
                    }
                }
            }
            return vsc;
        }

        public static List<JournalEntryDB> GetAllDBByTLU(long tluid)
        {
            List<JournalEntryDB> vsc = new List<JournalEntryDB>();

            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE TravelLogId = @source ORDER BY EventTime ASC"))
                {
                    cmd.AddParameterWithValue("@source", tluid);
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vsc.Add(Create(reader));
                        }
                    }
                }
            }
            return vsc;
        }

        public static List<JournalEntry> GetAllEntryByTLU(long tluid)
        {
            List<JournalEntry> vsc = new List<JournalEntry>();

            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE TravelLogId = @source ORDER BY EventTime ASC"))
                {
                    cmd.AddParameterWithValue("@source", tluid);
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JObject jo = (JObject)JObject.Parse((string)reader["EventData"]);
                            vsc.Add(JournalEntry.CreateJournalEntry(reader, jo));
                        }
                    }
                }
            }
            return vsc;
        }

        public static JournalEntry GetLast(int cmdrid, DateTime before, Func<JournalEntry, bool> filter)
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE CommanderId = @cmdrid AND EventTime < @time ORDER BY EventTime DESC"))
                {
                    cmd.AddParameterWithValue("@cmdrid", cmdrid);
                    cmd.AddParameterWithValue("@time", before);
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JournalEntryDB ent = Create(reader);
                            if (filter(ent.je))
                            {
                                return ent.je;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static T GetLast<T>(int cmdrid, DateTime before, Func<T, bool> filter = null)
            where T : JournalEntry
        {
            return (T)GetLast(cmdrid, before, e => e is T && (filter == null || filter((T)e)));
        }


        public static JournalEntry GetLast(DateTime before, Func<JournalEntry, bool> filter)
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE EventTime < @time ORDER BY EventTime DESC"))
                {
                    cmd.AddParameterWithValue("@time", before);
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JournalEntryDB ent = Create(reader);
                            if (filter(ent.je))
                            {
                                return ent.je;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static T GetLast<T>(DateTime before, Func<T, bool> filter = null)
            where T : JournalEntry
        {
            return (T)GetLast(before, e => e is T && (filter == null || filter((T)e)));
        }

        public static void RemoveGeneratedKeys(JObject obj, bool removeLocalised)
        {
            foreach (JProperty prop in obj.Properties().ToList())
            {
                if (prop.Name.StartsWith("EDD") || (removeLocalised && prop.Name.EndsWith("_Localised")))
                {
                    obj.Remove(prop.Name);
                }
            }

            obj.Remove("StarPosFromEDSM");
        }

        public static bool AreSameEntry(JournalEntryDB ent1, JournalEntryDB ent2, JObject ent1jo = null, JObject ent2jo = null)
        {
            if (ent1.jEventData == null || ent2.jEventData == null)
                return false;

            if (ent1jo == null)
            {
                ent1jo = (JObject)ent1.jEventData.DeepClone();
                RemoveGeneratedKeys(ent1jo, false);
            }

            if (ent2jo == null)
            {
                ent2jo = (JObject)ent2.jEventData.DeepClone();
                RemoveGeneratedKeys(ent2jo, false);
            }

            return JToken.DeepEquals(ent1jo, ent2jo);
        }

        public static List<JournalEntryDB> FindEntry(JournalEntryDB ent)
        {
            List<JournalEntryDB> entries = new List<JournalEntryDB>();
            JObject entjo = (JObject)ent.jEventData.DeepClone();
            RemoveGeneratedKeys(entjo, false);

            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE CommanderId = @cmdrid AND EventTime = @time AND TravelLogId = @tluid AND EventTypeId = @evttype ORDER BY Id ASC"))
                {
                    cmd.AddParameterWithValue("@cmdrid", ent.je.CommanderId);
                    cmd.AddParameterWithValue("@time", ent.je.EventTimeUTC);
                    cmd.AddParameterWithValue("@tluid", ent.je.TLUId);
                    cmd.AddParameterWithValue("@evttype", ent.je.EventTypeID);
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JournalEntryDB jent = Create(reader);
                            if (!AreSameEntry(ent, jent, entjo))
                            {
                                entries.Add(jent);
                            }
                        }
                    }
                }
            }

            return entries;
        }

        public static int RemoveDuplicateFSDEntries(int currentcmdrid)
        {
            // list of systems in journal, sorted by time
            List<JournalLocOrJump> vsSystemsEnts = GetAll(currentcmdrid).OfType<JournalLocOrJump>().OrderBy(j => j.EventTimeUTC).ToList();

            int count = 0;
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                for (int ji = 1; ji < vsSystemsEnts.Count; ji++)
                {
                    JournalEvents.JournalFSDJump prev = vsSystemsEnts[ji - 1] as JournalEvents.JournalFSDJump;
                    JournalEvents.JournalFSDJump current = vsSystemsEnts[ji] as JournalEvents.JournalFSDJump;

                    if (prev != null && current != null)
                    {
                        bool previssame = (prev.StarSystem.Equals(current.StarSystem, StringComparison.CurrentCultureIgnoreCase) && (!prev.HasCoordinate || !current.HasCoordinate || (prev.StarPos - current.StarPos).LengthSquared < 0.01));

                        if (previssame)
                        {
                            Delete(prev.Id, cn);
                            count++;
                            System.Diagnostics.Debug.WriteLine("Dup {0} {1} {2} {3}", prev.Id, current.Id, prev.StarSystem, current.StarSystem);
                        }
                    }
                }
            }

            return count;
        }

        public static void ClearEDSMID(int currentcmdrid = -2)      // -2 is all
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("UPDATE JournalEntries SET EdsmId=0"))
                {
                    if (currentcmdrid != -2)
                    {
                        cmd.CommandText = "UPDATE JournalEntries SET EdsmId=0 WHERE CommanderId==@cmd";
                        cmd.AddParameterWithValue("@cmd", currentcmdrid);
                    }

                    SQLiteDBClass.SQLNonQueryText(cn, cmd);
                }
            }
        }

        static public bool ResetCommanderID(int from, int to)
        {
            using (SQLiteConnectionUser cn = new SQLiteConnectionUser(utc: true))
            {
                using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set CommanderID = @cmdridto where CommanderID=@cmdridfrom"))
                {
                    if (from == -1)
                        cmd.CommandText = "Update JournalEntries set CommanderID = @cmdridto";

                    cmd.AddParameterWithValue("@cmdridto", to);
                    cmd.AddParameterWithValue("@cmdridfrom", from);
                    System.Diagnostics.Trace.WriteLine(string.Format("Update cmdr id ID {0} with {1}", from, to));
                    SQLiteDBClass.SQLNonQueryText(cn, cmd);
                }
            }
            return true;
        }

        public static void UpdateMapColour(long journalid, int mapcolour)
        {
        }


    }
}
