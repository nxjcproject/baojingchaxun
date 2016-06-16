﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EnergyConsumptionAlarm.aspx.cs" Inherits="StatisticalAlarm.Web.UI_StatisticalAlarm.EnergyConsumptionAlarm.EnergyConsumptionAlarm" %>

<%@ Register Src="/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagName="OrganisationTree" TagPrefix="uc1" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>能耗报警查询</title>
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
    <script type="text/javascript" src="/UI_StatisticalAlarm/EnergyConsumptionAlarm/js/page/RealtimeAlarmMonitor.js" charset="utf-8"></script>
</head>
<body>
    <div class="easyui-layout" data-options="fit:true,border:false">
        <div data-options="region:'west',split:true" style="width: 230px;">
            <uc1:OrganisationTree ID="OrganisationTree_ProductionLine" runat="server" />
        </div>
        <!-- 图表开始 -->
        <div id="toolbar_ReportTemplate" style="display: none;">
            <%--<table>
                <tr>
                    <td>
                        <table>
                            <tr>
                                <td style="width: 60px">生产线：</td>
                                <td>
                                    <input id="productLineName" class="easyui-textbox" style="width: 150px;" readonly="true" /><input id="organizationId" readonly="true" style="display: none;" /></td>
                                <td>
                                    <input type="radio" id="rdoYearly" name="alarmType" value="realtime" checked="checked" onclick="realtimeAlarm()" />实时
                                     <input type="radio" id="rdoMonthly" name="alarmType" value="history" onclick="setHistory()" />历史
                                </td>
                            </tr>
                            <tr>
                                <td class="queryDate" style="display: none; width: 60px">开始时间：</td>
                                <td class="queryDate" style="display: none">
                                    <input id="startDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td>
                                <td class="queryDate" style="display: none">结束时间：<input id="endDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td>
                                <td class="queryDate" style="display: none"><a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search',plain:true"
                                    onclick="QueryReportFun();">查询</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>--%>
            <div style="margin-left:10px;margin-top:5px;margin-bottom:5px">
                <span style="margin-right:12px">
                    生产线:
                </span>
                <span>
                    <input id="productLineName" class="easyui-textbox" style="width: 150px;" readonly="readonly" /><input id="organizationId" readonly="readonly" style="display: none;" />
                    <input type="radio" id="Radio1" name="alarmType" value="realtime" checked="checked" onclick="realtimeAlarm()" />实时
                    <input type="radio" id="Radio2" name="alarmType" value="history" onclick="setHistory()" />历史
                </span>
            </div>
            <div class="historyTool" style="display: none;margin-left:10px;margin-bottom:5px" >
                <span>故障名称:</span>
                <select id="cc" class="easyui-combotree" name="dept" style="width:150px;" data-options="panelHeight:true"></select>
                <span>开始时间:</span>
                <span>
                    <input id="startDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                </span>
                <span>
                    结束时间:
                </span>     
                <span>
                    <input id="endDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                </span>
                <span>
                    <%--<input type="radio" id="Radio3" name="myType" value="energyAlarm" checked="checked" onclick="realtimeAlarm()" />能耗报价
                    <input type="radio" id="Radio4" name="myType" value="coalAlarm" onclick="setHistory()" />煤耗报警--%>
                    <a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search',plain:true"
                                    onclick="QueryReportFun();">查询</a>
                </span>
            </div>
        </div>

        <div id="reportTable" class="easyui-panel" data-options="region:'center', border:true, collapsible:false, split:false">
            <table id="gridMain_ReportTemplate"></table>
        </div>
        <!-- 图表结束 -->
    </div>

    <form id="form_Main" runat="server"></form>

</body>
</html>

