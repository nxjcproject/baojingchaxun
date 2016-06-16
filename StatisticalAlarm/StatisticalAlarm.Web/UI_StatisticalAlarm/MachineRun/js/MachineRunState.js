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
        $('#gridMain_ReportTemplate').datagrid({
            columns: [[
                    { field: 'EquipmentName', title: '主机名称', width: 150 },
                    { field: 'Name', title: '产线', width: 100 },
                    //{ field: 'Count', title: '运行次数', width: 80 },
                    {
                        field: 'StartTime', title: '开机时间', width: 150, formatter: function (value, row, index) {
                            if (row.StartTime == "" && row.HaltTime == "") {
                                return value = "停机中...";
                            } else {
                                return value;
                            }
                        }
                    },
                    {
                        field: 'HaltTime', title: '停机时间', width: 150, formatter: function (value, row, index) {
                            if (row.StartTime != "" && row.HaltTime=="") {
                                return value="正在运行...";
                            } else {
                                return value;
                            }
                        }
                    },
                    { field: 'RunTime', title: '运行时间', width: 100},
                    { field: 'ReasonText', title: '原因', width: 300 }
            ]],
            fit: true,
            toolbar: "#toolbar_ReportTemplate",
            rownumbers: true,
            singleSelect: true,
            striped: true,
            data: []
        });
    }
    else {
        $('#gridMain_ReportTemplate').datagrid({ data: [] });
        $('#gridMain_ReportTemplate').datagrid('loadData', myData);
       // $('#gridMain_ReportTemplate').datagrid("collapseAll");
    }
}
var organizationID = "";
function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    organizationID = node.OrganizationId;
    $('#organizationId').val(organizationID);

    //初始化
    $('#MainMachine').combobox('setText', "");
    $('#MainMachine').combobox('setValue',"");

    //g_timer = setTimeout("realtimeAlarm()", 300000);

    LoadMainMachineClassList(organizationID);
}
//var equipmentId = "";
var mOrganizationID = "";
var variableName = "";
function LoadMainMachineClassList(organizationID) {
    $.ajax({
        type: "POST",
        url: "MachineRunState.aspx/MainMachineClassList",
        data: '{mOrganizationID:"' + organizationID + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
            $('#EquipmentName').combobox({
                data: m_MsgData.rows,
                valueField: 'VariableName',
                textField: 'EquipmentName',
                onSelect: function (param) {
                    variableName = param.VariableName;
                    mOrganizationID = param.OrganizationID;
                }
            });
        }
    });
}

//function LoadMainMachineList(mEquipmentId) {
//    $.ajax({
//        type: "POST",
//        url: "MachineRunState.aspx/MainMachineList",
//        data: '{mEquipmentId: "' + mEquipmentId  + '"}',
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        success: function (msg) {
//            var  m_MsgData = jQuery.parseJSON(msg.d);
//            $('#MainMachine').combobox({
//                data: m_MsgData.rows,
//                valueField: 'VariableName',
//                textField: 'VariableDescription',
//                onSelect: function (param) {
//                    variableName = param.VariableName;
//                    mOrganizationID = param.OrganizationID;
//                }
//            });
//        }
//    });
//}
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
                $('#gridMain_ReportTemplate').datagrid('loadData', []);
                $.messager.alert('提示', '没有相关数据！');
            }
            else {
                loadDataGrid("last", m_MsgData);
            }
        },
        error: function handleError() {
            $('#gridMain_ReportTemplate').datagrid('loadData', []);
            $.messager.alert('失败', '获取数据失败');
        }
    });
}