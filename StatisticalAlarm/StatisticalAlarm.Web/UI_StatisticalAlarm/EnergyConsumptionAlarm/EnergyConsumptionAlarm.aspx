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
        <div data-options="region:'west',split:true" style="width: 150px;">
            <uc1:OrganisationTree ID="OrganisationTree_ProductionLine" runat="server" />
        </div>
        <!-- 图表开始 -->
        <div id="toolbar_ReportTemplate" style="display: none;">
            <div style="margin-top:5px;margin-bottom:5px">
                <span style="padding-left:5px;">组织机构</span>
                <span>
                    <input id="productLineName" class="easyui-textbox" style="width: 100px;" readonly="readonly" /><input id="organizationId" readonly="readonly" style="display: none;" />
                    <input type="radio" id="Radio1" name="alarmType" value="realtime" checked="checked" onclick="realtimeAlarm()" />实时
                    <input type="radio" id="Radio2" name="alarmType" value="history" onclick="setHistory()" />历史
                </span>
            </div>
            <div class="historyTool" style="display: none; margin-bottom:5px" >
                <span style="padding-left:5px;">报警类型</span>
                <input id="alarmType" class="easyui-combobox" style="width:100px;"/>
                <span style="padding-left:10px;">开始时间</span>
                <span>
                    <input id="startDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                </span>
                <span style="padding-left:10px;">结束时间</span>     
                <span>
                    <input id="endDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                </span>
                <span>
                    <a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search'" onclick="QueryReportFun();">查询</a>
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

