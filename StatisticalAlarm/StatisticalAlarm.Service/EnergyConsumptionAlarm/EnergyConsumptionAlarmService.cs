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
            string mySql = @"select C.[OrganizationID]
	                                ,B.LevelCode,B.Name+C.Name as Name 
                                    ,C.[EnergyConsumptionType]
                                    ,C.[StartTime] as AlarmDateTime
                                    ,C.[TimeSpan]
                                    ,C.[LevelCode]
                                    ,C.[StandardValue]
                                    ,C.[ActualValue]
                                    ,C.[Superscale]
                                    ,C.[Reason]
                                    ,C.[VariableID]
                                from system_TenDaysRealtimeAlarm A,system_Organization B,shift_EnergyConsumptionAlarmLog C
                                where A.OrganizationID=B.OrganizationID
                                and A.KeyId=C.EnergyConsumptionAlarmLogID
                                and (A.AlarmType='EnergyConsumption'
                                or A.AlarmType='Power'
                                or A.AlarmType='CoalConsumption')
                                and A.OrganizationID like @organizationId+'%'
                                order by C.[StartTime] desc";
            SqlParameter parameter = new SqlParameter("@organizationId", organizationId);
            DataTable table = dataFactory.Query(mySql, parameter);
            return table;
        }

        public static DataTable GetHistoryAlarmData(string organizationId,string startTime,string endTime,string variableId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            List<SqlParameter> parameterList = new List<SqlParameter>();
            string mySql = @"  select A.[OrganizationID]
	                                ,B.LevelCode,B.Name+A.Name as Name 
                                    ,A.[EnergyConsumptionType]
                                    ,A.[StartTime] as AlarmDateTime
                                    ,A.[TimeSpan]
                                    ,A.[LevelCode]
                                    ,A.[StandardValue]
                                    ,A.[ActualValue]
                                    ,A.[Superscale]
                                    ,A.[Reason]
                                    ,A.[VariableID]
                                FROM [shift_EnergyConsumptionAlarmLog] A,[system_Organization] B
                                where A.[OrganizationID]=B.[OrganizationID]
                                and A.[OrganizationID] like @organizationId+'%'
                                and (A.[StartTime]>=@startTime and A.[StartTime]<=@endTime)
                                and A.[EnergyConsumptionType]=@variableId
                                order by A.[StartTime] desc";
            parameterList.Add(new SqlParameter("@startTime", startTime));
            parameterList.Add(new SqlParameter("@endTime", endTime));
            parameterList.Add(new SqlParameter("@organizationId", organizationId));
            parameterList.Add(new SqlParameter("@variableId", variableId));
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
            SqlParameter parameter = new SqlParameter("@organizationId", organizationId);
            DataTable table = dataFactory.Query(mySql, parameter);
            return table;
        }
    }
}
