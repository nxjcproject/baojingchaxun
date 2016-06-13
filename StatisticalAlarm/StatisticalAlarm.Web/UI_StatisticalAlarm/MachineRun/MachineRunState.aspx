<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MachineRunState.aspx.cs" Inherits="StatisticalAlarm.Web.UI_StatisticalAlarm.MachineRun.MachineRunState1" %>

<%@ Register Src="/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagName="OrganisationTree" TagPrefix="uc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
     <title>主机运行</title>
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css" />
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css" />

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <!--[if lt IE 8 ]><script type="text/javascript" src="/js/common/json2.min.js"></script><![endif]-->
    <script type="text/javascript" src="/lib/ealib/extend/jquery.PrintArea.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/extend/jquery.jqprint.js" charset="utf-8"></script>

    <script type="text/javascript" src="js/MachineRunState.js"charset="utf-8"></script>
</head>
<body>
    <div class="easyui-layout" data-options="fit:true,border:false">
        <div data-options="region:'west',split:true" style="width: 230px;">
            <uc1:OrganisationTree ID="OrganisationTree_ProductionLine" runat="server" />
        </div>
        <!-- 图表开始 -->
        <div id="toolbar_ReportTemplate" style="display: none;">
            <table>
                <tr>
                    <td>
                         <table>
                            <tr>
                                <td  class="queryDate" style="width:60px">开始时间：</td>

                                <td class="queryDate">
                                    <input id="startDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td>     
                                <td class="queryDate">                            
                                    结束时间：<input id="endDate" type="text" class="easyui-datetimebox" required="required" style="width: 150px;" />
                                </td> 
                                <td style="width:20px"></td>  
                                <td class="queryDate"><a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search',plain:true"
                                    onclick="QueryReportFun();">查询</a>
                                </td>               
                            </tr>
                        </table>
                        <table>
                            <tr>
                                <td style="width:50px"> 生产线：</td>
                                <td><input id="productLineName" class="easyui-textbox" style="width: 90px;" readonly="readonly" /><input id="organizationId" readonly="readonly" style="display: none;" /></td>                                                                                          
                                <td style="width:80px"> 主设备名称：</td> 
                                <td><input id="MainMachineName" class="easyui-combobox"data-options="panelHeight: 'auto'" style="width: 140px;"/></td>      
                                <td style="width:50px"> 主设备：</td> 
                                <td><input id="MainMachine" class="easyui-combobox" data-options="panelHeight: 'auto'"style="width:240px;" /></td>                           
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

    <form id="form1" runat="server">
    <div>    
    </div>
    </form>
</body>
</html>

