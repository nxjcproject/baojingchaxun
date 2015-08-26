using SqlServerDataAdapter;
using StatisticalAlarm.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalAlarm.Service.HaltAlarmService
{
   public class HaltAlarmService
    {
       public static DataTable GetRealTimeHaltAlarmData(string organizationId)
       {
           string connectionString = ConnectionStringFactory.NXJCConnectionString;
           ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
           string mySql = @"select A.OrganizationID,C.Name,B.MasterEquipmentName,B.MasterLabel,B.EquipmentName as SlaveEquipmentName,B.Label as SlaveLabel,
                                    '从机延时停机' as Type,B.WarmingTime as HaltTime,B.ReasonText,B.MasterHaltTime as time 
                                    from [dbo].[shift_MachineHaltLog] A,[dbo].[shift_SlaverHaltDelayAlarmLog] B,system_Organization C,                                        
                                         (select LevelCode from system_Organization where OrganizationID=@organizationId) D,
                                         system_TenDaysRealtimeAlarm E
                                    where E.AlarmType='MachineHalt'
                                    and E.OrganizationID=C.OrganizationID
                                    and A.OrganizationID=C.OrganizationID
                                    and E.KeyId=A.MachineHaltLogID 
                                    and A.MachineHaltLogID=B.KeyID
                                    and A.HaltTime=B.MasterHaltTime
                                    and C.LevelCode like D.LevelCode+'%'                                 
                                    group by B.MasterHaltTime,A.OrganizationID,C.Name,B.MasterEquipmentName,B.MasterLabel,B.EquipmentName,B.Label,B.ReasonText
                                    union all
                                    select A.OrganizationID,B.Name,A.EquipmentName as MasterEquipmentName,A.Label as MasterLabel,'无' as SlaveEquipmentName,
                                    '无' as SlaveLabel,'主机停机' as Type,D.AlarmDateTime as HaltTime,A.ReasonText,A.HaltTime as time
                                    from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                                         (select LevelCode from system_Organization where OrganizationID=@organizationId) C,
                                         system_TenDaysRealtimeAlarm D
                                    where D.AlarmType='MachineHalt'
                                     and D.OrganizationID=B.OrganizationID
                                     and A.OrganizationID=B.OrganizationID                                    
                                     and D.KeyId=A.MachineHaltLogID  
                                     and D.AlarmDateTime=A.HaltTime                                  
                                    and B.LevelCode like C.LevelCode+'%'                       
                                    group by D.AlarmDateTime,A.OrganizationID,B.Name,A.EquipmentName,A.Label,A.ReasonText
                                     order by HaltTime desc";
           SqlParameter parameter = new SqlParameter("organizationId", organizationId);
           DataTable originalTable = dataFactory.Query(mySql, parameter);

           DataColumn levelColumn = new DataColumn("LevelCode", typeof(string));
           originalTable.Columns.Add(levelColumn);
           DataRow[] masterRows = originalTable.Select("Type='主机停机'");
           int length = masterRows.Count();

           DataColumn count = new DataColumn("Count", typeof(int));
           originalTable.Columns.Add(count);
           for (int i = 0; i < length; i++)
           {
               DataRow dr = masterRows[i];
               dr["LevelCode"] = "M01" + (i + 1).ToString("00");
               string masterName = dr["MasterEquipmentName"].ToString().Trim();
               string myOrganizationId = dr["OrganizationID"].ToString().Trim();
               string halttime=dr["time"].ToString();
               DataRow[] slaveRows = originalTable.Select("MasterEquipmentName='" + masterName + "' and SlaveEquipmentName<>'无' and OrganizationID='" + myOrganizationId + "'"+"time='"+halttime+"'");
               int slaveLength = slaveRows.Count();
               for (int j = 0; j < slaveLength; j++)
               {
                   slaveRows[j]["LevelCode"] = "M01" + (i + 1).ToString("00") + (j + 1).ToString("00");
               }
               dr["count"] = slaveLength.ToString();
           }

           DataColumn equipmentColumn = new DataColumn("EquipmentName", typeof(string));
           DataColumn stateColumn = new DataColumn("state", typeof(string));


           originalTable.Columns.Add(equipmentColumn);
           originalTable.Columns.Add(stateColumn);
           //DataRow factoryRow = originalTable.NewRow();
           //factoryRow["Name"] = "分厂";
           //factoryRow["LevelCode"] = "M01";

           foreach (DataRow dr in originalTable.Rows)
           {
               if (dr["SlaveLabel"].ToString() == "无")//主机设备停机
               {
                   dr["EquipmentName"] = dr["MasterEquipmentName"];
                   dr["state"] = "closed";
               }
               else//从机报警
               {
                   dr["EquipmentName"] = dr["SlaveEquipmentName"];
                   dr["state"] = "open";
               }

           }
           return originalTable;
       }
       public static DataTable GetHistoryHaltAlarmData(string organizationId, string startTime, string endTime)
       {
           string connectionString = ConnectionStringFactory.NXJCConnectionString;
           ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
           string mySql = @"select A.OrganizationID,C.Name,B.MasterEquipmentName,B.MasterLabel,B.EquipmentName as SlaveEquipmentName,B.Label as SlaveLabel,
                                    '从机延时停机' as Type,B.WarmingTime as HaltTime,B.ReasonText,B.MasterHaltTime as time
                                    from [dbo].[shift_MachineHaltLog] A,[dbo].[shift_SlaverHaltDelayAlarmLog] B,system_Organization C,
                                    (select LevelCode from system_Organization where OrganizationID=@organizationId) D
                                    where A.MachineHaltLogID=B.KeyID
                                    and A.OrganizationID=C.OrganizationID
                                    and A.HaltTime=B.MasterHaltTime
                                    and C.LevelCode like D.LevelCode+'%'
                                    and B.MasterHaltTime>=@startTime
                                    and B.MasterHaltTime<=@endTime
                                    group by B.MasterHaltTime,B.WarmingTime,A.OrganizationID,C.Name,B.MasterEquipmentName,B.MasterLabel,B.EquipmentName,B.Label,B.ReasonText
                                    union all
                                    select A.OrganizationID,B.Name,A.EquipmentName as MasterEquipmentName,A.Label as MasterLabel,'无' as SlaveEquipmentName,
                                    '无' as SlaveLabel,'主机停机' as Type,A.HaltTime as HaltTime,A.ReasonText,A.HaltTime as time
                                    from [dbo].[shift_MachineHaltLog] A,system_Organization B,
                                    (select LevelCode from system_Organization where OrganizationID=@organizationId) C
                                    where A.OrganizationID=B.OrganizationID
                                    and B.LevelCode like C.LevelCode+'%'
                                    and A.HaltTime>=@startTime
                                    and A.HaltTime<=@endTime
                                    group by A.HaltTime,A.OrganizationID,B.Name,A.EquipmentName,A.Label,A.ReasonText
                                    order by HaltTime desc";
           SqlParameter[] parameters = { new SqlParameter("organizationId", organizationId), 
                                            new SqlParameter("startTime", startTime), new SqlParameter("endTime", endTime) };
           DataTable originalTable = dataFactory.Query(mySql, parameters);
           DataColumn levelColumn = new DataColumn("LevelCode", typeof(string));
           originalTable.Columns.Add(levelColumn);
           DataRow[] masterRows = originalTable.Select("Type='主机停机'");
           int length = masterRows.Count();

           DataColumn count = new DataColumn("Count", typeof(string));
           originalTable.Columns.Add(count);
           for (int i = 0; i < length; i++)
           {
               DataRow dr = masterRows[i];
               dr["LevelCode"] = "M01" + (i + 1).ToString("00");
               string masterName = dr["MasterEquipmentName"].ToString().Trim();
               string myOrganizationId = dr["OrganizationID"].ToString().Trim();
               string halttime = dr["time"].ToString();
               DataRow[] slaveRows = originalTable.Select("MasterEquipmentName='" + masterName + "' and SlaveEquipmentName<>'无' and OrganizationID='" + myOrganizationId + "'and time='" + halttime + "'");
               int slaveLength = slaveRows.Count();
               for (int j = 0; j < slaveLength; j++)
               {
                   slaveRows[j]["LevelCode"] = "M01" + (i + 1).ToString("00") + (j + 1).ToString("00");

               }
               dr["count"]=slaveLength.ToString();
           }
           
           DataColumn equipmentColumn = new DataColumn("EquipmentName", typeof(string));
           DataColumn stateColumn = new DataColumn("state", typeof(string));

           
           originalTable.Columns.Add(equipmentColumn);
           originalTable.Columns.Add(stateColumn);
           //DataRow factoryRow = originalTable.NewRow();
           //factoryRow["Name"] = "分厂";
           //factoryRow["LevelCode"] = "M01";
           
           foreach (DataRow dr in originalTable.Rows)
           {     
               if (dr["SlaveLabel"].ToString() == "无")//主机设备停机
               {
                   dr["EquipmentName"] = dr["MasterEquipmentName"];
                   dr["state"] = "closed";
               }
               else//从机报警
               {
                   dr["EquipmentName"] = dr["SlaveEquipmentName"];
                   dr["state"] = "open";          
               }
             
           }
           return originalTable;

       }
    }
}
