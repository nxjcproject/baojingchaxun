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
	                                ,B.[OrganizationID]
                                    ,B.[VariableDescription]
                                FROM [equipment_EquipmentDetail] A,[system_MasterMachineDescription] B
	                           where A.[EquipmentId] = B.[ID] 
	                             and A.[Enabled] = 1
                                 and A.[OrganizationId] = @mOrganizationID
                                 and A.[EquipmentCommonId] in ('RawMaterialsGrind','RotaryKiln','CementGrind','CoalGrind') --20180806闫潇华添加，限制四大主机 
	                            order by A.DisplayIndex  ";
            SqlParameter param = new SqlParameter("@mOrganizationID", mOrganizationID);
            DataTable Table = dataFactory.Query(mySql, param);
            DataRow dr = Table.NewRow();
            dr["EquipmentId"] = "1";
            dr["EquipmentName"] = "全部";
            dr["EquipmentCommonId"] = "";
            dr["OrganizationID"] = "";
            dr["VariableDescription"] = "";
            Table.Rows.InsertAt(dr, 0);
            return Table;
        }

        public static DataTable GetHistoryHaltAlarmData(string morganizationId, string mEquipmentId, string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID as LevelCode,'LeafNode' as Type,B.Name,A.EquipmentName,A.EquipmentID,A.StartTime as StartTime,A.ReasonText,A.HaltTime as HaltTime,A.RecoverTime,A.Remarks,
                            convert(varchar,DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24)+'天'+convert(varchar,DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)+'时'+convert(varchar,DATEDIFF(minute,A.StartTime,A.HaltTime)-DATEDIFF(minute,A.StartTime,A.HaltTime)/60/24*24*60-(DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)*60)+'分' as RunTime,
                            convert(varchar,DATEDIFF(MINUTE,A.HaltTime,A.RecoverTime)/60/24)+'天'+
							convert(varchar,DATEDIFF(Minute,A.HaltTime,A.RecoverTime)/60-DATEDIFF(MINUTE,A.HaltTime,A.RecoverTime)/60/24*24)+'时'+
							convert(varchar,DATEDIFF(minute,A.HaltTime,A.RecoverTime)-DATEDIFF(minute,A.HaltTime,A.RecoverTime)/60/24*24*60
							-(DATEDIFF(Minute,A.HaltTime,A.RecoverTime)/60
							-DATEDIFF(MINUTE,A.HaltTime,A.RecoverTime)/60/24*24)*60)+'分' 
							as StopTime
                            from [dbo].[shift_MachineHaltLog] A, system_Organization B
                            where A.OrganizationID=B.OrganizationID
							and A.OrganizationID=@organizationId
                            and ((A.StartTime>=@startTime and A.StartTime<=@endTime)
                                  or (A.HaltTime>=@startTime and A.HaltTime<=@endTime))
                            and A.EquipmentID=@mEquipmentId
                            group by A.EquipmentName,A.StartTime,A.HaltTime,A.OrganizationID,LevelCode,B.Name,A.EquipmentID,A.ReasonText,Type,A.RecoverTime,A.Remarks  		                              									 
							order by EquipmentName,StartTime desc";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", morganizationId), 
                                            new SqlParameter("@startTime", startTime),
                                            new SqlParameter("@endTime", endTime),
                                            new SqlParameter("@mEquipmentId", mEquipmentId) 
                                        };
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            int length = originalTable.Rows.Count;
            if (length >= 1)
            {
                for (int j = 0; j < length; j++)
                {
                    if (Convert.ToString(originalTable.Rows[0]["HaltTime"]) != "")
                    {
                        TimeSpan runningSpan = Convert.ToDateTime(originalTable.Rows[0]["HaltTime"]) - Convert.ToDateTime(originalTable.Rows[0]["StartTime"]);
                        string runningTime = runningSpan.Days.ToString() + "天" + runningSpan.Hours.ToString() + "时" + runningSpan.Minutes.ToString() + "分";
                        originalTable.Rows[0]["RunTime"] = runningTime;
                        if (Convert.ToString(originalTable.Rows[0]["RecoverTime"])=="")
                        {
                            TimeSpan stopSpan = DateTime.Now - Convert.ToDateTime(originalTable.Rows[0]["HaltTime"]);
                            string stopTime = stopSpan.Days.ToString() + "天" + stopSpan.Hours.ToString() + "时" + stopSpan.Minutes.ToString() + "分";
                            originalTable.Rows[0]["RecoverTime"] = DBNull.Value;
                            originalTable.Rows[0]["StopTime"] = stopTime;
                        }
                    }
                    else
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
                DataRow[] rows = originalTable.Select("LevelCode='" + morganizationId + "'and EquipmentID='" + mEquipmentId + "'");
                foreach (DataRow rw in rows)
                {
                    originalTable.Rows.Remove(rw);
                }
                originalTable.AcceptChanges();
            }
            return originalTable;
        }
        public static DataTable GetHistoryHaltAlarmDataAll(string organizationID, string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID as LevelCode,'LeafNode' as Type,B.Name,A.EquipmentName,A.EquipmentID,A.StartTime as StartTime,A.ReasonText,A.HaltTime as HaltTime,A.RecoverTime,A.Remarks,
                            convert(varchar,DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24)+'天'+convert(varchar,DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)+'时'+convert(varchar,DATEDIFF(minute,A.StartTime,A.HaltTime)-DATEDIFF(minute,A.StartTime,A.HaltTime)/60/24*24*60-(DATEDIFF(Minute,A.StartTime,A.HaltTime)/60-DATEDIFF(MINUTE,A.StartTime,A.HaltTime)/60/24*24)*60)+'分' as RunTime,
                            convert(varchar,DATEDIFF(MINUTE,A.HaltTime,A.RecoverTime)/60/24)+'天'+
							convert(varchar,DATEDIFF(Minute,A.HaltTime,A.RecoverTime)/60-DATEDIFF(MINUTE,A.HaltTime,A.RecoverTime)/60/24*24)+'时'+
							convert(varchar,DATEDIFF(minute,A.HaltTime,A.RecoverTime)-DATEDIFF(minute,A.HaltTime,A.RecoverTime)/60/24*24*60
							-(DATEDIFF(Minute,A.HaltTime,A.RecoverTime)/60
							-DATEDIFF(MINUTE,A.HaltTime,A.RecoverTime)/60/24*24)*60)+'分' 
							as StopTime
                            from [dbo].[shift_MachineHaltLog] A, system_Organization B, equipment_EquipmentDetail C
                            where A.OrganizationID=B.OrganizationID
							and A.OrganizationID like @organizationID+'%'
                            and ((A.StartTime>=@startTime and A.StartTime<=@endTime)
                                  or (A.HaltTime>=@startTime and A.HaltTime<=@endTime))
                            and A.EquipmentID = C.EquipmentId 
                            and C.EquipmentCommonId in ('RawMaterialsGrind','RotaryKiln','CementGrind','CoalGrind') --20180806闫潇华添加，限制四大主机                   
                            group by A.EquipmentName,A.StartTime,A.HaltTime,A.OrganizationID,LevelCode,B.Name,A.EquipmentID,A.ReasonText,Type,A.RecoverTime,A.Remarks  		                              									 
							order by EquipmentName,StartTime desc";
            SqlParameter[] parameters = {   new SqlParameter("@organizationID",organizationID),
                                            new SqlParameter("@startTime", startTime),
                                            new SqlParameter("@endTime", endTime),                                         
                                        };
            DataTable originalTable = dataFactory.Query(mySql, parameters);
            int length = originalTable.Rows.Count;
            if (length >= 1)
            {
                for (int j = 0; j < length; j++)
                {
                    if (Convert.ToString(originalTable.Rows[0]["HaltTime"]) != "")
                    {
                        TimeSpan runningSpan = Convert.ToDateTime(originalTable.Rows[0]["HaltTime"]) - Convert.ToDateTime(originalTable.Rows[0]["StartTime"]);
                        string runningTime = runningSpan.Days.ToString() + "天" + runningSpan.Hours.ToString() + "时" + runningSpan.Minutes.ToString() + "分";
                        originalTable.Rows[0]["RunTime"] = runningTime;
                        if (Convert.ToString(originalTable.Rows[0]["RecoverTime"]) == "")
                        {
                            TimeSpan stopSpan = DateTime.Now - Convert.ToDateTime(originalTable.Rows[0]["HaltTime"]);
                            string stopTime = stopSpan.Days.ToString() + "天" + stopSpan.Hours.ToString() + "时" + stopSpan.Minutes.ToString() + "分";
                            originalTable.Rows[0]["RecoverTime"] = DBNull.Value;
                            originalTable.Rows[0]["StopTime"] = stopTime;
                        }
                    }
                    else
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
                DataRow[] rows = originalTable.Select("LevelCode like '" + organizationID + "' + '%'");
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
