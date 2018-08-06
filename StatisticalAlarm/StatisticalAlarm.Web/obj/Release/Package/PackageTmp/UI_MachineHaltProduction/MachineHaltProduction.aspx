<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MachineHaltProduction.aspx.cs" Inherits="StatisticalAlarm.Web.MachineHaltProduction.MachineHaltProduction" %>
<%@ Register Src="~/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagName="OrganisationTree" TagPrefix="uc1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>主机停机电量统计</title>
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css"/>
	<link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css"/>
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css"/>
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtCss.css"/>

	<script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
	<script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <script type="text/javascript" src="/lib/ealib/extend/jquery.PrintArea.js" charset="utf-8"></script> 
    <script type="text/javascript" src="/lib/ealib/extend/jquery.jqprint.js" charset="utf-8"></script>
    <!--[if lt IE 8 ]><script type="text/javascript" src="/js/common/json2.min.js"></script><![endif]-->
    <script type="text/javascript" src="/js/common/PrintFile.js" charset="utf-8"></script> 

    <script type="text/javascript" src="js/page/MachineHaltProduction.js" charset="utf-8"></script>
</head>
<body>
    <div class="easyui-layout" data-options="fit:true,border:false">   
        <div data-options="region:'west',split:false" style="width: 150px;">   
            <uc1:OrganisationTree ID="OrganisationTree_ProductionLine" runat="server" />
        </div>   
        <!-- 图表开始 -->
        <div id="toolbar_ReportTemplate" style="height:30px;padding-top:10px">            
                        <table>
                            <tr>
                                <td style="width: 50px; text-align: right;">组织机构</td>
                                <td>
                                    <input id="productLineName" class="easyui-textbox" style="width: 100px;" readonly="true" />
                                    <input id="organizationId" readonly="true" style="display: none;" />
                                </td>
                                <td style="width: 50px; text-align: right;">开始时间</td>
                                <td>
                                    <input id="startDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td>
                                <td style="width: 50px; text-align: right;">结束时间</td>
                                <td>
                                    <input id="endDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td>
                                <td><a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search'" onclick="query();">查询</a>
                                </td>
                            </tr>
                        </table>       
        </div>  
        <div data-options="region:'center'" style="padding:5px;background:#eee;">
             <table id="Windows_Report" class="easyui-treegrid"></table>
         </div> 
        <!-- 图表结束 -->
    </div>
</body>
</html>
