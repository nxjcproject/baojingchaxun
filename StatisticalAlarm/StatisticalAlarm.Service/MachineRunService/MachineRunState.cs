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
        public static DataTable GetMainMachineClassList(string organizationId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select distinct(VariableDescription) from [dbo].[system_MasterMachineDescription]  
                          where OrganizationID like @organizationId+'%' order by VariableDescription ";
            SqlParameter parameters = new SqlParameter("@organizationId", organizationId);
            DataTable Table = dataFactory.Query(mySql, parameters);
            return Table;
        }
        public static DataTable GetMainMachineList(string organizationId, string variableName) 
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID,B.[Name]+A.[VariableDescription] as MainMachine,A.VariableName 
                                 from [dbo].[system_MasterMachineDescription] A,[dbo].[system_Organization] B
                             where A.OrganizationID like @organizationId+'%' and A.VariableDescription=@variableName
                             and A.OrganizationID=B.OrganizationID order by OrganizationID";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", organizationId), new SqlParameter("@variableName", variableName) };
            DataTable Table = dataFactory.Query(mySql, parameters);
            return Table;
        }
        public static DataTable GetHistoryHaltAlarmData(string organizationId, string MainMachine, string startTime, string endTime)
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
            int length = originalTable.Rows.Count;
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
            else
            {
                DataRow[] rows = originalTable.Select("LevelCode='" + organizationId + "'and MasterLabel='" + MainMachine + "'");
                foreach (DataRow rw in rows)
                {
                    originalTable.Rows.Remove(rw);
                }
                originalTable.AcceptChanges();
            }
            return originalTable;
        }
    }
}
