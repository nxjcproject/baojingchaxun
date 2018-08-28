using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StatisticalAlarm.Web.UI_StatisticalAlarm.MachineRun
{
    public partial class MachineRunState1 : WebStyleBaseForEnergy.webStyleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
#if DEBUG
                ////////////////////调试用,自定义的数据授权

                List<string> m_DataValidIdItems = new List<string>() { "zc_nxjc_byc_byf", "zc_nxjc_qtx", "zc_nxjc_tsc_tsf", "zc_nxjc_znc_znf", "bf_spbfc_spbff" };
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
#elif RELEASE
#endif
                this.OrganisationTree_ProductionLine.Organizations = GetDataValidIdGroup("ProductionOrganization");                         //向web用户控件传递数据授权参数
                this.OrganisationTree_ProductionLine.PageName = "MachineRunState.aspx";   //向web用户控件传递当前调用的页面名称
                this.OrganisationTree_ProductionLine.LeveDepth = 5;
            }
        }

        [WebMethod]
        public static string MainMachineClassList(string mOrganizationID) 
        {
            DataTable table = StatisticalAlarm.Service.MachineRunService.MachineRunState.GetMainMachineClassList(mOrganizationID);
            string json = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(table);
            return json;      
        }
        
        [WebMethod]
        public static string GetHistoryHaltAlarm(string morganizationID, string mEquipmentId, string startTime, string endTime)
        {
            DataTable table = StatisticalAlarm.Service.MachineRunService.MachineRunState.GetHistoryHaltAlarmData(morganizationID, mEquipmentId, startTime, endTime);
            string json = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(table);
            return json;
        }

        [WebMethod]
        public static string GetHistoryHaltAlarmAll(string organizationID, string startTime, string endTime)
        {
            DataTable table = StatisticalAlarm.Service.MachineRunService.MachineRunState.GetHistoryHaltAlarmDataAll(organizationID, startTime, endTime);
            string json = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(table);
            return json;
        }
    }
}