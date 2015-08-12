using StatisticalAlarm.Infrastructure.Configuration;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalAlarm.Service
{
    public class EnergyConsumptionAlarmService
    {
        public static DataTable GetRealTimeAlarmData(string organizationId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select top(200) A.AlarmDateTime,B.Name as ProductLineName,C.Name,C.EnergyConsumptionType,C.StandardValue,C.ActualValue
                                from system_TenDaysRealtimeAlarm A,system_Organization B,shift_EnergyConsumptionAlarmLog C
                                where A.OrganizationID=B.OrganizationID
                                and A.KeyId=C.EnergyConsumptionAlarmLogID
                                and (A.AlarmType='EnergyConsumption'
                                or A.AlarmType='Power'
                                or A.AlarmType='CoalConsumption')
                                and B.LevelCode like (
						                                SELECT E.LevelCode
						                                FROM system_Organization E
						                                WHERE E.OrganizationID=@organizationId)+'%'
                                order by A.AlarmDateTime desc";
            SqlParameter parameter = new SqlParameter("organizationId", organizationId);
            DataTable table = dataFactory.Query(mySql, parameter);
            return table;
        }

        public static DataTable GetHistoryAlarmData(string organizationId,string startTime,string endTime,string variableId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = "";
            List<SqlParameter> parameterList = new List<SqlParameter>();
            if (variableId == "null")
            {
                mySql = @"select A.StartTime as AlarmDateTime, B.Name as ProductLineName,A.Name,A.EnergyConsumptionType,A.StandardValue,A.ActualValue
                        from shift_EnergyConsumptionAlarmLog A,system_Organization B
                        where A.OrganizationID=B.OrganizationID
                        and (A.StartTime>=@startTime and A.StartTime<=@endTime)
                        and B.LevelCode like (
				                        SELECT E.LevelCode
				                        FROM system_Organization E
				                        WHERE E.OrganizationID=@organizationId)+'%'
                        order by A.StartTime desc";
                parameterList.Add(new SqlParameter("startTime", startTime));
                parameterList.Add(new SqlParameter("endTime", endTime));
                parameterList.Add(new SqlParameter("organizationId", organizationId));
            }
            else
            {
                mySql = @"select A.StartTime as AlarmDateTime, B.Name as ProductLineName,A.Name,A.EnergyConsumptionType,A.StandardValue,A.ActualValue
                        from shift_EnergyConsumptionAlarmLog A,system_Organization B
                        where A.OrganizationID=B.OrganizationID
                        and (A.StartTime>=@startTime and A.StartTime<=@endTime)
                        and B.LevelCode like (
				                        SELECT E.LevelCode
				                        FROM system_Organization E
				                        WHERE E.OrganizationID=@organizationId)+'%'
                        and VariableID=@variableId
                        order by A.StartTime desc";
                parameterList.Add(new SqlParameter("startTime", startTime));
                parameterList.Add(new SqlParameter("endTime", endTime));
                parameterList.Add(new SqlParameter("organizationId", organizationId));
                parameterList.Add(new SqlParameter("variableId", variableId));
            }
            SqlParameter[] parameters = parameterList.ToArray();
            DataTable table = dataFactory.Query(mySql, parameters);
            return table;
        }

        /// <summary>
        /// 根据组织机构查询工序名称和设备名称
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static DataTable GetAlarmItem(string organizationId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.OrganizationID,B.VariableId,B.LevelCode,B.Name as text,C.Type,
                                case when len(B.LevelCode)=5 then 'closed' else 'open' end as state
                                from [dbo].[tz_Formula] A,[dbo].[formula_FormulaDetail] B,system_Organization C
                                where A.KeyID=B.KeyID
								and A.OrganizationID=C.OrganizationID
                                and A.OrganizationID=@organizationId
                                order by OrganizationID,B.LevelCode";
            SqlParameter parameter = new SqlParameter("organizationId", organizationId);
            DataTable table = dataFactory.Query(mySql, parameter);
            return table;
        }
    }
}
