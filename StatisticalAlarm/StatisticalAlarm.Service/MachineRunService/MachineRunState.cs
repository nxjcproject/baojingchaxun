using SqlServerDataAdapter;
using StatisticalAlarm.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace StatisticalAlarm.Service.MachineRunService
{
    public class MachineRunState
    {
        public static DataTable GetHistoryHaltAlarmData(string organizationId, string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID,B.Name,A.EquipmentName,A.Label as MasterLabel,A.StartTime,A.ReasonText,A.HaltTime,
                                convert(varchar,DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24)+'天'+convert(varchar,DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)+'时'+convert(varchar,DATEDIFF(minute,A.StartTime,A.HaltTime)-DATEDIFF(minute,A.StartTime,A.HaltTime)/60/24*24*60-(DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)*60)+'分' as RunTime
                                from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                                (select LevelCode from system_Organization where OrganizationID=@organizationId) C
                                where A.OrganizationID=B.OrganizationID
                                and B.LevelCode like C.LevelCode+'%'
                                and A.StartTime>=@startTime
                                and A.HaltTime<=@endTime
                                group by A.StartTime,A.HaltTime,A.OrganizationID,B.Name,A.EquipmentName,A.Label,A.ReasonText
                                order by HaltTime desc";
            SqlParameter[] parameters = { new SqlParameter("organizationId", organizationId), 
                                            new SqlParameter("startTime", startTime), new SqlParameter("endTime", endTime) };
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            return originalTable;
        }
        public static DataTable GetHistoryHaltAlarmData(string organizationId, string MainMachine,string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID as LevelCode,'LeafNode' as Type,B.Name,A.EquipmentName,A.Label as MasterLabel,A.StartTime as StartTime,A.ReasonText,A.HaltTime as HaltTime,A.RecoverTime,
                            convert(varchar,DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24)+'天'+convert(varchar,DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)+'时'+convert(varchar,DATEDIFF(minute,A.StartTime,A.HaltTime)-DATEDIFF(minute,A.StartTime,A.HaltTime)/60/24*24*60-(DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)*60)+'分' as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B
                            where A.OrganizationID=B.OrganizationID
							and A.OrganizationID=@organizationId
                            and A.StartTime>=@startTime
                            and A.HaltTime<=@endTime
                            and A.Label=@mainMachine
                            group by A.EquipmentName,A.StartTime,A.HaltTime,A.OrganizationID,LevelCode,B.Name,A.Label,A.ReasonText,Type,A.RecoverTime  
		                    union all
                            select distinct(A.OrganizationID),'LeafNode' as Type,B.Name,A.EquipmentName,A.Label as MasterLabel,NULL as StartTime,'' as ReasonText,getdate() as HaltTime,'' as RecoverTime,
                            NULL as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                            (select LevelCode from system_Organization where OrganizationID =@organizationId) C
                            where A.OrganizationID=B.OrganizationID
                            and B.LevelCode like C.LevelCode+'%'
                            and A.OrganizationID=@organizationId
                            and A.Label=@mainMachine
                            group by A.OrganizationID,A.EquipmentName,B.Name,A.Label,A.ReasonText,Type,RecoverTime										 
							order by EquipmentName,HaltTime desc";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", organizationId), 
                                            new SqlParameter("@startTime", startTime),
                                            new SqlParameter("@endTime", endTime),
                                            new SqlParameter("@mainMachine", MainMachine) 
                                        };
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            int length= originalTable.Rows.Count;
            if (length > 1)
            {
                for (int j = 0; j < length; j++)
                {
                    if (j == 0 && Convert.ToString(originalTable.Rows[1]["RecoverTime"]) != "")
                    {
                        //dr["StartTime"] = originalTable.Rows[1]["RecoverTime"];
                        originalTable.Rows[0]["StartTime"] = originalTable.Rows[1]["RecoverTime"];
                        TimeSpan runningSpan = Convert.ToDateTime(originalTable.Rows[0]["HaltTime"]) - Convert.ToDateTime(originalTable.Rows[0]["StartTime"]);
                        string runningTime = runningSpan.Days.ToString() + "天" + runningSpan.Hours.ToString() + "时" + runningSpan.Minutes.ToString() + "分";
                        //dr["RunTime"] = runningTime;
                        originalTable.Rows[0]["RunTime"] = runningTime;
                        originalTable.Rows[0]["HaltTime"] = DBNull.Value;
                    }
                    else if (j == 0 && Convert.ToString(originalTable.Rows[1]["RecoverTime"]) == "")
                    {
                        originalTable.Rows[0]["HaltTime"] = DBNull.Value;
                    }
                }
            }
            else {
                DataRow[] rows = originalTable.Select("LevelCode='" + organizationId  + "'and MasterLabel='" + MainMachine + "'");
                foreach (DataRow rw in rows)
                {
                    originalTable.Rows.Remove(rw);
                }
                originalTable.AcceptChanges();           
            }          
            return originalTable;
        }
        public static DataTable TempHistoryHaltAlarmData(string organizationId, string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID,'LeafNode' as Type,B.Name,B.LevelCode as level,A.EquipmentName,A.Label as MasterLabel,A.StartTime as StartTime,A.ReasonText,A.HaltTime as HaltTime,A.RecoverTime,
                            convert(varchar,DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24)+'天'+convert(varchar,DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)+'时'+convert(varchar,DATEDIFF(minute,A.StartTime,A.HaltTime)-DATEDIFF(minute,A.StartTime,A.HaltTime)/60/24*24*60-(DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)*60)+'分' as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                            (select LevelCode from system_Organization where OrganizationID like @organizationId+'%') C
                            where A.OrganizationID=B.OrganizationID
                            and B.LevelCode like C.LevelCode+'%'
                            and A.StartTime>=@startTime
                            and A.HaltTime<=@endTime
                            group by A.EquipmentName,A.StartTime,A.HaltTime,A.OrganizationID,B.Name,A.Label,A.ReasonText,Type,B.LevelCode,A.RecoverTime    
                            union all
                            select distinct(A.OrganizationID),'Node' as Type,B.Name,B.LevelCode as level,A.EquipmentName,A.Label as MasterLabel,NULL as StartTime,'' as ReasonText,NULL as HaltTime,'' as RecoverTime,
                            NULL as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                            (select LevelCode from system_Organization where OrganizationID like @organizationId+'%') C
                            where A.OrganizationID=B.OrganizationID
                            and B.LevelCode like C.LevelCode+'%'
                            group by A.OrganizationID,A.EquipmentName,B.Name,A.Label,A.ReasonText,Type,B.LevelCode,A.RecoverTime 
							 union all
                            select distinct(A.OrganizationID),'LeafNode' as Type,B.Name,B.LevelCode as level,A.EquipmentName,A.Label as MasterLabel,NULL as StartTime,'' as ReasonText,getdate() as HaltTime,'' as RecoverTime,
                            NULL as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                            (select LevelCode from system_Organization where OrganizationID like @organizationId+'%') C
                            where A.OrganizationID=B.OrganizationID
                            and B.LevelCode like C.LevelCode+'%'
                            group by A.OrganizationID,A.EquipmentName,B.Name,A.Label,A.ReasonText,Type,B.LevelCode,A.RecoverTime
                            order by  B.LevelCode,A.EquipmentName,HaltTime desc,Type";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", organizationId), 
                                          new SqlParameter("@startTime", startTime), 
                                          new SqlParameter("@endTime", endTime) };
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            //DataTable originalTable = dataFactory.Query(mySql);
            DataColumn levelColumn = new DataColumn("LevelCode", typeof(string));
            originalTable.Columns.Add(levelColumn);
            DataRow[] masterRows = originalTable.Select("Type='Node'");
            int length = masterRows.Count();

           DataColumn count = new DataColumn("Count", typeof(string));
           originalTable.Columns.Add(count);
           for (int i = 0; i < length; i++)
           {
               DataRow dr = masterRows[i];
               dr["LevelCode"] = "M01" + (i + 1).ToString("00");


               string Type = "LeafNode";
               string myOrganizationId = dr["OrganizationID"].ToString().Trim();
               string Level = dr["level"].ToString().Trim();
               string Name = dr["Name"].ToString().Trim();
               string EquipmentName = dr["EquipmentName"].ToString().Trim();            
               string MasterLabel = dr["MasterLabel"].ToString();

               DataRow[] slaveRows = originalTable.Select("OrganizationID='" + myOrganizationId + "' and Type='" + Type + "' and Name='" + Name + "' and EquipmentName='" + EquipmentName + "'and MasterLabel='" + MasterLabel + "'");
               int slaveLength = slaveRows.Count();
               if (slaveLength > 1)
               {
                   for (int j = 0; j < slaveLength; j++)
                   {


                       slaveRows[j]["LevelCode"] = "M01" + (i + 1).ToString("00") + (j + 1).ToString("00");

                       if (j == 0 && Convert.ToString(slaveRows[1]["RecoverTime"]) != "")
                       {
                           dr["StartTime"] = slaveRows[1]["RecoverTime"];
                           slaveRows[0]["StartTime"] = slaveRows[1]["RecoverTime"];
                           TimeSpan runningSpan = Convert.ToDateTime(slaveRows[0]["HaltTime"]) - Convert.ToDateTime(slaveRows[0]["StartTime"]);
                           string runningTime = runningSpan.Days.ToString() + "天" + runningSpan.Hours.ToString() + "时" + runningSpan.Minutes.ToString() + "分";

                           slaveRows[0]["RunTime"] = runningTime;
                           slaveRows[0]["HaltTime"] = DBNull.Value;
                       }
                       else if (j == 0 && Convert.ToString(slaveRows[1]["RecoverTime"]) == "")
                       {
                           slaveRows[0]["HaltTime"] = DBNull.Value;
                       }


                   }
               }
               else {
                   DataRow[] rows = originalTable.Select("level='" + Level + "' and Name='" + Name + "' and EquipmentName='" + EquipmentName + "'and MasterLabel='" + MasterLabel + "'");
                   foreach(DataRow rw in rows){
                       originalTable.Rows.Remove(rw);
                   }
                   originalTable.AcceptChanges();
                   //slaveRows[0]["LevelCode"] = "M01" + (i + 1).ToString("00") + (1).ToString("00");
                   //slaveRows[0]["HaltTime"] = DBNull.Value;
               }            
               dr["count"] = slaveLength.ToString();
               dr["RunTime"] = "停机次数: " + (Convert.ToInt16(dr["count"])-1).ToString();
           }
           
            return originalTable;
        }
        public static DataTable GetProductionLineList(string organizationId) 
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select * from NXJC.dbo.system_Organization 
	                            where OrganizationID like @organizationId+'_%'
	                            order by LevelCode";
            SqlParameter parameters = new SqlParameter("@organizationId", organizationId);
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            return originalTable;        
        }
        public static DataTable GetMainMachineList(string organizationId) 
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select * from [dbo].[system_MasterMachineDescription]
                             	where OrganizationID=@organizationId";
            SqlParameter parameters = new SqlParameter("@organizationId", organizationId);
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            return originalTable;    
        }
    }
}
