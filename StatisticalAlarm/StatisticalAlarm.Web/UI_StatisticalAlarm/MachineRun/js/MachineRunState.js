var g_timer;

$(function () {
    InitDate();
    loadDataGrid("first");
});
//初始化日期框
function InitDate() {
    var nowDate = new Date();
    var beforeDate = new Date();
    beforeDate.setDate(nowDate.getDate() - 10);
    var nowString = nowDate.getFullYear() + '-' + (nowDate.getMonth() + 1) + '-' + nowDate.getDate() + " " + nowDate.getHours() + ":" + nowDate.getMinutes() + ":" + nowDate.getSeconds();
    var beforeString = beforeDate.getFullYear() + '-' + (beforeDate.getMonth() + 1) + '-' + beforeDate.getDate() + " 00:00:00";
    $('#startDate').datetimebox('setValue', beforeString);
    $('#endDate').datetimebox('setValue', nowString);
}
var organizationID = "";
function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
    organizationID = node.OrganizationId;
    LoadProductionLine(node.OrganizationId);

    //g_timer = setTimeout("realtimeAlarm()", 300000);
}
function LoadProductionLine(OrganizationId) {
    
    $.ajax({
        type: "POST",
        url: "MachineRunState.aspx/GetProductionLine",
        data: '{organizationId: "' + OrganizationId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
                $('#productLine').combobox({
                    data: m_MsgData.rows,
                    valueField: 'OrganizationID',
                    textField: 'Name',
                    onSelect: function (param) {
                        organizationID = param.OrganizationID;
                     LoadMainMachine(param.OrganizationID);
                    }
                });        
        }     
    });
}
var MainMachine = "";
function LoadMainMachine(mOrganizationId) {
    $.ajax({
        type: "POST",
        url: "MachineRunState.aspx/GetMainMachineList",
        data: '{organizationId: "' + mOrganizationId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
                $('#MainMachineName').combobox({
                    data: m_MsgData.rows,
                    valueField: 'VariableName',
                    textField: 'VariableDescription',
                    onSelect: function (param) {
                        MainMachine = param.VariableName;
                    }
                });         
        }
    });

};
function loadDataGrid(type, myData) {
    if (type == "first") {
        $('#gridMain_ReportTemplate').treegrid({
            columns: [[
                    { field: 'EquipmentName', title: '主机名称', width: 150 },
                    { field: 'Name', title: '产线', width: 100 },
                    //{ field: 'Count', title: '运行次数', width: 80 },
                    { field: 'StartTime', title: '开机时间', width: 150 },
                    { field: 'HaltTime', title: '停机时间', width: 150 },
                    { field: 'RunTime', title: '运行时间', width: 100 },
                    { field: 'ReasonText', title: '原因', width: 300 }
            ]],
            fit: true,
            toolbar: "#toolbar_ReportTemplate",
            rownumbers: true,
            singleSelect: true,
            striped: true,
            data: [],
            idField: 'LevelCode',
            treeField: 'EquipmentName',
        });      
    }
    else {
        $('#gridMain_ReportTemplate').treegrid({ data: [] });
        $('#gridMain_ReportTemplate').treegrid('loadData', myData);
        $('#gridMain_ReportTemplate').treegrid("collapseAll");
    }
}
function QueryReportFun() {
    editIndex = undefined;
    var startTime = $('#startDate').datetimebox('getValue');//开始时间
    var endTime = $('#endDate').datetimebox('getValue');//结束时间
    if (organizationID == "" || startDate == "" || endDate == "") {
        $.messager.alert('警告', '请选择生产线和时间');
        return;
    }
    if (startDate > endDate) {
        $.messager.alert('警告', '结束时间不能大于开始时间！');
        return;
    }
    var queryUrl = "";
    var queryData = "";
    
    
    if ($('#MainMachineName').combobox('getText') == "") {
        MainMachine = "";
    }
    if (MainMachine == "") {
        queryUrl = "MachineRunState.aspx/GetHistoryHaltAlarmA";
        queryData= '{organizationId: "' + organizationID + '", startTime: "' + startTime + '", endTime: "' + endTime + '"}'; 
    }
    else if (MainMachine != "") {
        queryUrl = "MachineRunState.aspx/GetHistoryHaltAlarmB";
        queryData = '{organizationID: "' + organizationID + '", mainMachine: "' + MainMachine + '", startTime: "' + startTime + '", endTime: "' + endTime + '"}';
    }
    $.ajax({
        type: "POST",
        url: queryUrl,
        data: queryData,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData.total == 0) {
                $.messager.alert('提示', '没有查询的数据');
            }
            else {
                loadDataGrid("last", m_MsgData);
            }
        },
        error: function handleError() {
            $('#gridMain_ReportTemplate').treegrid('loadData', []);
            $.messager.alert('失败', '获取数据失败');
        }
    });
}
