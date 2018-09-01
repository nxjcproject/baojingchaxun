using SqlServerDataAdapter;
using StatisticalAlarm.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalAlarm.Service.MachineHaltProductionService
{
    public class MachineHaltProductionService
    {
        public static DataTable GetMachineHaltInformationDataTable(string mOrganizationId, string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mysql = @"select E.[EquipmentID],E.[EquipmentName],F.Name as text,E.StartTime,E.HaltTime,(case when E.RecoverTime is NULL then @endTime else E.RecoverTime end) as RecoverTime,E.MeterDatabase,F.VariableId,F.LevelCode
                                    , F.VariableId + convert(varchar(32),E.StartTime) as id
                                    , (case when E.VariableId = F.VariableId then Lower(convert(varchar(64),E.[EquipmentID])) else E.VariableId + convert(varchar(32),E.StartTime) end) as ParentNodeId
                                    from (SELECT B.[EquipmentName]
                                          ,A.[EquipmentID]
                                          ,A.[StartTime]
                                          ,A.[HaltTime]
                                          ,A.[RecoverTime]
	                                      ,B.VariableId
	                                      ,C.KeyId
	                                      ,D.LevelCode
	                                      ,G.[MeterDatabase]
                                      FROM [shift_MachineHaltLog] A,[equipment_EquipmentDetail] B,[tz_Formula] C,[formula_FormulaDetail] D,[system_Organization] P,[system_Database] G
                                      where A.[OrganizationID]=@mOrganizationId
                                      and ((A.HaltTime>=@startTime and A.RecoverTime<=@endTime)
                                      or (A.HaltTime<=@startTime and RecoverTime>=@startTime)
                                      or (A.HaltTime<=@endTime and A.RecoverTime>=@endTime)
                                      or (A.HaltTime<=@endTime and A.RecoverTime is NULL))
                                      and B.EquipmentId=A.EquipmentID
                                      and C.Type=2 and C.ENABLE=1 and C.State=0 
                                      and C.[OrganizationID]=@mOrganizationId
                                      and C.KeyID=(SELECT TOP 1 [KeyID]
                                                   FROM [tz_Formula]
                                                   WHERE OrganizationID=@mOrganizationId AND ENABLE = 1 AND State = 0 AND CreatedDate <=@endTime 
                                                   ORDER BY CreatedDate DESC)
                                      and D.KeyID=C.KeyID 
                                      and D.[VariableId]=B.VariableId
                                      and P.OrganizationID=@mOrganizationId
                                      and P.[DatabaseID]=G.[DatabaseID]
                                       )  E
                                      left join [NXJC_DEV].[dbo].[formula_FormulaDetail] F 
                                      on F.KeyID=E.KeyID and F.LevelCode like E.LevelCode + '%'
                                      --group by E.EquipmentName,E.StartTime,E.HaltTime,E.RecoverTime,E.VariableId,E.KeyID,E.LevelCode,E.MeterDatabase,F.VariableId,F.LevelCode,F.Name
                                      order by E.[EquipmentName],E.StartTime,E.LevelCode";
            SqlParameter[] parameter ={
                                     new SqlParameter("@mOrganizationId",mOrganizationId),
                                     new SqlParameter("@startTime",startTime),
                                     new SqlParameter("@endTime",endTime)
                                     };
            DataTable texttable = dataFactory.Query(mysql, parameter);
            DataTable table = new DataTable();
            table.Columns.Add("EquipmentID");
            table.Columns.Add("EquipmentName");
            table.Columns.Add("text");
            table.Columns.Add("HaltTime");
            table.Columns.Add("RecoverTime");
            table.Columns.Add("HaltSumTime");
            table.Columns.Add("HaltPowerConsumption");
            table.Columns.Add("id");
            table.Columns.Add("ParentNodeId");
            for (int i = 0; i < texttable.Rows.Count; i++)
            {
                string equipmentId = texttable.Rows[i]["EquipmentID"].ToString().Trim();
                string newName = texttable.Rows[i]["EquipmentName"].ToString().Trim();
                string equipmentName = texttable.Rows[i]["text"].ToString().Trim();            
                string variableid = texttable.Rows[i]["VariableId"].ToString().Trim();
                string dataBase = texttable.Rows[i]["MeterDatabase"].ToString().Trim();
                string halttime = texttable.Rows[i]["HaltTime"].ToString().Trim();
                string retime = texttable.Rows[i]["RecoverTime"].ToString().Trim();
                string nodeId = texttable.Rows[i]["id"].ToString().Trim();
                string parentId = texttable.Rows[i]["ParentNodeId"].ToString().Trim();
                string haltsumTime;
                if (halttime == "" || retime == "")
                {
                    haltsumTime = "";
                }
                else {
                    DateTime t1 = Convert.ToDateTime(halttime);
                    DateTime t2 = Convert.ToDateTime(retime);
                    TimeSpan ts = t2 - t1;
                    haltsumTime = (ts.Days * 24 + ts.Hours) + "时" + ts.Minutes + "分" + ts.Seconds + "秒";
                }
                string sqltable;
                if (texttable.Rows[i]["ParentNodeId"].ToString() == texttable.Rows[i]["EquipmentID"].ToString())
                {
                    sqltable = "[HistoryFormulaValue]";
                }
                else
                {
                    sqltable = "[HistoryMainMachineFormulaValue]";
                }
                /////////////20180608闫潇华 因运行信号和电量采集的周期不同，在计算电量时将停机时间延后10分钟，若延迟后大于开机时间则不计算，记为0 修改开始//////////////////
                string mHaltFormulaCount;  //停机电量
                DateTime mActualComputeHaltTime = Convert.ToDateTime(halttime).AddMinutes(10);
                DateTime mActualComputeRecoverTime = Convert.ToDateTime(retime);
                mActualComputeHaltTime.AddMinutes(10);
                mActualComputeHaltTime.AddMinutes(10);
                if (DateTime.Compare(mActualComputeHaltTime, mActualComputeRecoverTime) > 0)
                {
                    mHaltFormulaCount = "0.00";
                }
                else
                {
                    string mmsql = @"SELECT cast(sum([FormulaValue]) as decimal(18,2)) as HaltPowerConsumption from {0}.[dbo].{1}
                                  where [OrganizationID]=@mOrganizationId
                                        and VariableID=@variableid
                                        and vDate>=@mActualComputeHaltTime
                                        and vDate<=@mActualComputeRecoverTime";
                    SqlParameter[] ppara ={
                                 new SqlParameter("@mOrganizationId",mOrganizationId),
                                 new SqlParameter("@variableid",variableid),
                                 new SqlParameter("@mActualComputeHaltTime",mActualComputeHaltTime),
                                 new SqlParameter ("@mActualComputeRecoverTime",mActualComputeRecoverTime)
                                 };
                    DataTable ftable = dataFactory.Query(string.Format(mmsql, dataBase, sqltable), ppara);
                    mHaltFormulaCount = ftable.Rows[0]["HaltPowerConsumption"].ToString().Trim();
                }
                /////////////20180608闫潇华 因运行信号和电量采集的周期不同，在计算电量时将停机时间延后10分钟，若延迟后大于开机时间则不计算，记为0 修改结束//////////////////                
                DataRow dr = table.NewRow();
                dr["EquipmentID"] = equipmentId;
                dr["EquipmentName"]=newName;
                dr["text"] = equipmentName;
                dr["HaltTime"] = halttime;
                dr["RecoverTime"] = retime;
                dr["HaltSumTime"] = haltsumTime;
                dr["HaltPowerConsumption"] = mHaltFormulaCount;
                dr["id"] = nodeId;
                dr["ParentNodeId"] = parentId;
                table.Rows.Add(dr);
            }
            for (int i = 0; i < table.Rows.Count;)
            {
                string m_Name = table.Rows[i]["EquipmentID"].ToString();
                DataRow[] m_SubRoot = table.Select("EquipmentID = '" + m_Name + "'");
                int length = m_SubRoot.Length;
                int sumSecond = 0;
                double sumPower = 0.00;
                for (int j = 0; j < length; j++)
                {
                    if (m_SubRoot[j]["EquipmentID"].ToString() == m_SubRoot[j]["ParentNodeId"].ToString())
                    {
                        if (m_SubRoot[j]["HaltPowerConsumption"].ToString() != "")
                        {
                            sumPower = Convert.ToDouble(m_SubRoot[j]["HaltPowerConsumption"].ToString()) + sumPower;
                        }
                        int totalSecond = Convert.ToInt32((Convert.ToDateTime(m_SubRoot[j]["RecoverTime"].ToString()) - Convert.ToDateTime(m_SubRoot[j]["HaltTime"].ToString())).TotalSeconds.ToString());
                        sumSecond = totalSecond + sumSecond;                    
                    }                   
                }
                TimeSpan newTs = new TimeSpan(0, 0, sumSecond);
                string stopTime = (newTs.Days * 24 + newTs.Hours) + "时" + newTs.Minutes + "分" + newTs.Seconds + "秒";
                DataRow newRow = table.NewRow();
                newRow["EquipmentID"] = table.Rows[i]["EquipmentID"].ToString();
                newRow["EquipmentName"] = table.Rows[i]["EquipmentName"].ToString();
                newRow["text"] = table.Rows[i]["EquipmentName"].ToString();
                newRow["HaltTime"] = "";
                newRow["RecoverTime"] = "";
                newRow["HaltSumTime"] = stopTime;
                newRow["HaltPowerConsumption"] = sumPower.ToString("0.00");
                newRow["id"] = table.Rows[i]["EquipmentID"].ToString();
                newRow["ParentNodeId"] = null;
                table.Rows.InsertAt(newRow,0);
                i = i + length +1;
            }

            DataColumn stateColumn = new DataColumn("state", typeof(string));
            table.Columns.Add(stateColumn);
            //此处代码是控制树开与闭的
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i]["ParentNodeId"].ToString()=="")
                {
                    table.Rows[i]["state"] = "closed";
                }
                else
                {
                    table.Rows[i]["state"] = "open";
                }            
            }
            return table;           
        }
    }
}
