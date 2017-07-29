using StatisticalAlarm.Service.MachineHaltProductionService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StatisticalAlarm.Web.MachineHaltProduction
{
    public partial class MachineHaltProduction : WebStyleBaseForEnergy.webStyleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
#if DEBUG
                ////////////////////调试用,自定义的数据授权
                List<string> m_DataValidIdItems = new List<string>() { "zc_nxjc_byc_byf", "zc_nxjc_klqc_klqf" };
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
#elif RELEASE
#endif
                this.OrganisationTree_ProductionLine.Organizations = GetDataValidIdGroup("ProductionOrganization");                         //向web用户控件传递数据授权参数
                this.OrganisationTree_ProductionLine.PageName = "MachineHaltProduction.aspx";                                     //向web用户控件传递当前调用的页面名称
                //this.OrganisationTree_ProductionLine.LeveDepth = 5;
                this.OrganisationTree_ProductionLine.OrganizationTypeItems.Add("水泥磨");
                this.OrganisationTree_ProductionLine.OrganizationTypeItems.Add("熟料");
            }
        }
        [WebMethod]
        public static string GetMachineHaltInformation(string mOrganizationId, string startTime, string endTime)
        {
            DataTable table = MachineHaltProductionService.GetMachineHaltInformationDataTable(mOrganizationId, startTime, endTime);
            //string json = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(table);
            string json = EasyUIJsonParser.TreeGridJsonParser.DataTableToJson(table, "id", "ParentNodeId", "text", "HaltTime", "RecoverTime", "HaltSumTime", "HaltPowerConsumption","state");
            //string json = EasyUIJsonParser.TreeGridJsonParser.DataTableToJson(table, "id", "name", "ParentNodeId", (object)("root"), null);
            //string json = EasyUIJsonParser.TreeGridJsonParser.DataTableToJson(table, "NodeId", "Name", "ParentNodeId", "0");
            return json;
        }
    }
}