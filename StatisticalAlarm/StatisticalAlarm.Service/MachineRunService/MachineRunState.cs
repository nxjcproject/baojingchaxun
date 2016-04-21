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
            string mySql = @"select A.OrganizationID as LevelCode,B.Name,A.EquipmentName,A.Label as MasterLabel,A.StartTime as StartTime,A.ReasonText,A.HaltTime as HaltTime,
                            convert(varchar,DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24)+'天'+convert(varchar,DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)+'时'+convert(varchar,DATEDIFF(minute,A.StartTime,A.HaltTime)-DATEDIFF(minute,A.StartTime,A.HaltTime)/60/24*24*60-(DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)*60)+'分' as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B
                            where A.OrganizationID=B.OrganizationID
							and A.OrganizationID=@organizationId
                            and A.StartTime>=@startTime
                            and A.HaltTime<=@endTime
                            and A.Label=@mainMachine
                            group by A.EquipmentName,A.StartTime,A.HaltTime,A.OrganizationID,LevelCode,B.Name,A.Label,A.ReasonText,Type   
							order by B.LevelCode,EquipmentName,HaltTime desc";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", organizationId), 
                                            new SqlParameter("@startTime", startTime),
                                            new SqlParameter("@endTime", endTime),
                                            new SqlParameter("@mainMachine", MainMachine) 
                                        };
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            return originalTable;
        }
        public static DataTable TempHistoryHaltAlarmData(string organizationId, string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID,'LeafNode' as Type,B.Name,B.LevelCode as level,A.EquipmentName,A.Label as MasterLabel,A.StartTime as StartTime,A.ReasonText,A.HaltTime as HaltTime,
                            convert(varchar,DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24)+'天'+convert(varchar,DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)+'时'+convert(varchar,DATEDIFF(minute,A.StartTime,A.HaltTime)-DATEDIFF(minute,A.StartTime,A.HaltTime)/60/24*24*60-(DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)*60)+'分' as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                            (select LevelCode from system_Organization where OrganizationID like @organizationId+'%') C
                            where A.OrganizationID=B.OrganizationID
                            and B.LevelCode like C.LevelCode+'%'
                            and A.StartTime>=@startTime
                            and A.HaltTime<=@endTime
                            group by A.EquipmentName,A.StartTime,A.HaltTime,A.OrganizationID,B.Name,A.Label,A.ReasonText,Type,B.LevelCode   
                            union all
                            select distinct(A.OrganizationID),'Node' as Type,B.Name,B.LevelCode as level,A.EquipmentName,A.Label as MasterLabel,NULL as StartTime,'' as ReasonText,NULL as HaltTime,
                            NULL as RunTime
                            from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                            (select LevelCode from system_Organization where OrganizationID like @organizationId+'%') C
                            where A.OrganizationID=B.OrganizationID
                            and B.LevelCode like C.LevelCode+'%'
                            group by A.OrganizationID,A.EquipmentName,B.Name,A.Label,A.ReasonText,Type,B.LevelCode
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
               string Name = dr["Name"].ToString().Trim();
               string EquipmentName = dr["EquipmentName"].ToString().Trim();            
               string MasterLabel = dr["MasterLabel"].ToString();

               DataRow[] slaveRows = originalTable.Select("OrganizationID='" + myOrganizationId + "' and Type='" + Type + "' and Name='" + Name + "' and EquipmentName='" + EquipmentName + "'and MasterLabel='" + MasterLabel + "'");
               int slaveLength = slaveRows.Count();
               for (int j = 0; j < slaveLength; j++)
               {
                   slaveRows[j]["LevelCode"] = "M01" + (i + 1).ToString("00") + (j + 1).ToString("00");

               }
               dr["count"] = slaveLength.ToString();
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
