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
        public static DataTable GetMainMachineClassList(string mOrganizationID)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @" SELECT A.[EquipmentId]
                                ,A.[EquipmentName]
                                ,A.[EquipmentCommonId]
	                            ,B.[OrganizationID]+','+B.[VariableName] as Variable
                                ,B.[VariableDescription]
                                FROM [NXJC].[dbo].[equipment_EquipmentDetail] A, [NXJC].[dbo].[system_MasterMachineDescription] B
	                            where  A.[EquipmentId]=B.[ID] 
	                              and A.[Enabled]=1
                                  and A.[OrganizationId]=@mOrganizationID
	                            order by A.DisplayIndex  ";
            SqlParameter param = new SqlParameter("@mOrganizationID", mOrganizationID);
            DataTable Table = dataFactory.Query(mySql, param);
            return Table;
        }
        public static DataTable GetMainMachineList(string mEquipmentId) 
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"SELECT [ID]
                                  ,[OrganizationID]
                                  ,[VariableID]
                                  ,[VariableName]
                                  ,[VariableDescription]
                                  ,[DataBaseName]
                                  ,[TableName]
                                  ,[Record]
                                  ,[ValidValues]
                                  ,[Remarks]
                                  ,[KeyID]
                                  ,[MachineHaltReason]
                                  ,[MachineHaltRecord]
                                  ,[OutputField]
                              FROM [NXJC].[dbo].[system_MasterMachineDescription]  where ID=@mEquipmentId";
            SqlParameter param = new SqlParameter("@mEquipmentId", mEquipmentId);
            DataTable Table = dataFactory.Query(mySql, param);
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
                            and A.StartTime<=@endTime
                            and A.Label=@mainMachine
                            group by A.EquipmentName,A.StartTime,A.HaltTime,A.OrganizationID,LevelCode,B.Name,A.Label,A.ReasonText,Type,A.RecoverTime  		                              									 
							order by EquipmentName,StartTime desc";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", organizationId), 
                                            new SqlParameter("@startTime", startTime),
                                            new SqlParameter("@endTime", endTime),
                                            new SqlParameter("@mainMachine", MainMachine) 
                                        };
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            int length = originalTable.Rows.Count;
            if (length > 1)
            {
                for(int j=0;j<length;j++)
                {
                    if (Convert.ToString(originalTable.Rows[0]["HaltTime"]) != "")
                    {
                        TimeSpan runningSpan = Convert.ToDateTime(originalTable.Rows[0]["HaltTime"]) - Convert.ToDateTime(originalTable.Rows[0]["StartTime"]);
                        string runningTime = runningSpan.Days.ToString() + "天" + runningSpan.Hours.ToString() + "时" + runningSpan.Minutes.ToString() + "分";
                        originalTable.Rows[0]["RunTime"] = runningTime;
                    }else
                    {
                        TimeSpan runningSpan = DateTime.Now - Convert.ToDateTime(originalTable.Rows[0]["StartTime"]);
                        string runningTime = runningSpan.Days.ToString() + "天" + runningSpan.Hours.ToString() + "时" + runningSpan.Minutes.ToString() + "分";
                        originalTable.Rows[0]["HaltTime"] = DBNull.Value;
                        originalTable.Rows[0]["RunTime"] = runningTime;                  
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
