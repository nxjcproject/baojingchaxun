<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HaltAlarm.aspx.cs" Inherits="StatisticalAlarm.Web.UI_StatisticalAlarm.HaltAlarm.HaltAlarm" %>

<%@ Register Src="/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagName="OrganisationTree" TagPrefix="uc1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>停机报警查询</title>
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css" />
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css" />

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <!--[if lt IE 8 ]><script type="text/javascript" src="/js/common/json2.min.js"></script><![endif]-->
    <script type="text/javascript" src="/lib/ealib/extend/jquery.PrintArea.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/extend/jquery.jqprint.js" charset="utf-8"></script>

    <script type="text/javascript" src="/js/common/PrintFile.js" charset="utf-8"></script>
    <script type="text/javascript" src="js/page/RealtimeHaltAlarmMonitor.js" charset="utf-8"></script>
</head>
<body>
      <div class="easyui-layout" data-options="fit:true,border:false">
        <div data-options="region:'west',split:true" style="width: 150px;">
            <uc1:OrganisationTree ID="OrganisationTree_ProductionLine" runat="server" />
        </div>
        <!-- 图表开始 -->
        <div id="toolbar_ReportTemplate" style="display: none;">
            <table>
                <tr>
                    <td>
                        <table>
                            <tr>
                                <td style="width:60px"> 生产线：</td>
                                <td>
                                    <input id="productLineName" class="easyui-textbox" style="width: 100px;" readonly="readonly" /><input id="organizationId" readonly="true" style="display: none;" /></td>
                                <td >
                                    <input type="radio" id="rdoYearly" name="alarmType" value="realtime" checked="checked" onclick="realtimeAlarm()"/>实时
                                     <input type="radio" id="rdoMonthly" name="alarmType" value="history" onclick="setHistory()"/>历史
                                </td>                                                               
                            </tr>
                            <tr>
                                <td  class="queryDate" style="display:none;width:60px">开始时间：</td>
                                <td class="queryDate" style="display:none">
                                    <input id="startDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td>     
                                <td class="queryDate" style="display:none">                            
                                    结束时间：<input id="endDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td>
                                <td  class="queryDate" style="display:none;width:80px">主设备名称：</td> 
                                <td class="queryDate" style="display:none;width:60px">
                                    <input id="MainMachine" type="text" class="easyui-combobox"  style="width: 150px;" />
                                </td>
                                <td class="queryDate" style="display:none"><a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search',plain:true"
                                    onclick="QueryReportFun();">查询</a>
                                </td>               
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td>
                        <table>                            
                        </table>
                    </td>
                </tr>
            </table>
        </div>

        <div id="reportTable" class="easyui-panel" data-options="region:'center', border:true, collapsible:true, split:false">
            <table id="gridMain_ReportTemplate"></table>
        </div>
        <!-- 图表结束 -->
    </div>
    <form id="form1" runat="server"> </form>
</body>
</html>
