$(function () {
    InitDate();
    loadDataGrid("first");
});
//初始化日期框
function InitDate() {
    var nowDate = new Date();
    var beforeDate = new Date();
    beforeDate.setDate(nowDate.getDate() - 5);
    var nowString = nowDate.getFullYear() + '-' + (nowDate.getMonth() + 1) + '-' + nowDate.getDate() + " " + nowDate.getHours() + ":" + nowDate.getMinutes() + ":" + nowDate.getSeconds();
    var beforeString = beforeDate.getFullYear() + '-' + (beforeDate.getMonth() + 1) + '-' + beforeDate.getDate() + " 00:00:00";
    $('#startDate').datetimebox('setValue', beforeString);
    $('#endDate').datetimebox('setValue', nowString);
}
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
var organizationID = "";
var mainMachine = "";
function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
    organizationID = node.OrganizationId;
    $('#MainMachineName').combobox('setValue', "");
    $('#MainMachineName').combobox('setText', "");
    $('#MainMachine').combobox('setValue', "");
    $('#MainMachine').combobox('setText', "");
    mainMachine = "";
    LoadMainMachineClassList(organizationID);

    //g_timer = setTimeout("realtimeAlarm()", 300000);
}
var variableName = "";
function LoadMainMachineClassList(OrganizationId) {
    $.ajax({
        type: "POST",
        url: "MachineRunState.aspx/MainMachineClassList",
        data: '{organizationId: "' + OrganizationId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
            $('#MainMachineName').combobox({
                data: m_MsgData.rows,
                valueField: 'VariableDescription',
                textField: 'VariableDescription',
                onSelect: function (param) {
                    variableName = param.VariableDescription;
                    $('#MainMachine').combobox('setValue', "");
                    $('#MainMachine').combobox('setText', "");
                    //MainMachine = "";
                    LoadMainMachineList(OrganizationId, variableName);
                }
            });
        }
    });
}
var mOrganizationID = "";
function LoadMainMachineList(OrganizationID, VariableName) {
    $.ajax({
        type: "POST",
        url: "MachineRunState.aspx/MainMachineList",
        data: '{organizationId: "' + OrganizationID + '",variableName:"' + VariableName + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
            $('#MainMachine').combobox({
                data: m_MsgData.rows,
                valueField: 'VariableName',
                textField: 'MainMachine',
                onSelect: function (param) {
                    $('#MainMachine').combobox('setValue', "");
                    $('#MainMachine').combobox('setText', "");
                    mOrganizationID = "";
                    mainMachine = "";
                    mOrganizationID = param.OrganizationID;
                    variableName = param.VariableName;
                    mainMachine = param.MainMachine;
                    $('#MainMachine').combobox('setValue', variableName);
                    $('#MainMachine').combobox('setText', mainMachine);
                 //   LoadMainMachineList(OrganizationId, variableName);
                }
            });
        }
    });
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
    $.ajax({
        type: "POST",
        url: "MachineRunState.aspx/GetHistoryHaltAlarm",
        data: '{organizationID: "' + mOrganizationID + '", mainMachine: "' + variableName + '", startTime: "' + startTime + '", endTime: "' + endTime + '"}',
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