var g_timer;
var variableId = "";
$(function () {
    InitDate();
    loadDataGrid("first");
    loadCombobox("first");
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
function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
        realtimeAlarm();
    //g_timer = setTimeout("realtimeAlarm()", 300000);
        loadMainMachineList(node.OrganizationId);
}

function setTimer() {
    g_timer = setTimeout("realtimeAlarm()", 300000);
}
function setHistory() {
    clearTimeout(g_timer);
    $('#gridMain_ReportTemplate').treegrid("loadData", []);
    $(".queryDate").show();
}
function realtimeAlarm() {
    if (document.getElementsByName("alarmType")[0].checked == false) {
        clearTimeout(g_timer);
        return;
    }
    else {
        $(".queryDate").hide();
    }
    var mager = $.messager.alert('提示', '数据加载中...');
    var organizationId = $('#organizationId').val();
    $.ajax({
        type: "POST",
        url: "HaltAlarm.aspx/GetRealTimeHaltAlarm",
        data: '{organizationId: "' + organizationId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,//同步执行
        success: function (msg) {
            mager.window('close');
            if (msg.d == "[]") {
                loadDataGrid("last", []);
                $.messager.alert('提示', '当前没有报警！');
            }
            else {
                m_MsgData = jQuery.parseJSON(msg.d);
               loadDataGrid("last", m_MsgData);
                setTimer();
            }           
        },
        beforeSend: function (XMLHttpRequest) {
            mager;
        },
        error: setTimer()
    });
}
function loadDataGrid(type, myData) {
    if (type == "first") {
        $('#gridMain_ReportTemplate').treegrid({
            columns: [[
                    { field: 'EquipmentName', title: '报警名称', width: 240 },                 
                    { field: 'Name', title: '产线', width: 100 },
                    { field: 'Type', title: '报警类别', width: 80, align: "center" },
                    { field: 'Count', title: '报警从机数', width: 80, align: "center" },
                    { field: 'HaltTime', title: '停机时间', width: 150 },
                    { field: 'ReasonText', title: '原因', width: 300 }
            ]],
            fit:true,
            toolbar: "#toolbar_ReportTemplate",
            rownumbers: true,
            singleSelect: true,
            striped: true,
            data:[],
            idField:"id",
            treeField: "EquipmentName"
        })
    }
    else {
        $('#gridMain_ReportTemplate').treegrid("loadData",myData);
    }
}
function QueryReportFun() {
    editIndex = undefined;
    var organizationId = $('#organizationId').val();
    var startTime = $('#startDate').datetimebox('getValue');//开始时间
    var endTime = $('#endDate').datetimebox('getValue');//结束时间
    if (organizationId == "" || startDate == "" || endDate == "") {
        $.messager.alert('警告', '请选择生产线和时间');
        return;
    }
    if (startDate > endDate) {
        $.messager.alert('警告', '结束时间不能大于开始时间！');
        return;
    }
    var m_url = "HaltAlarm.aspx/GetHistoryHaltAlarmbyMainMachine";
    var m_data = '{organizationId: "' + organizationId + '", variableId: "'+ variableId + '", startTime: "' + startTime + '", endTime: "' + endTime + '"}';
    if (variableId == "") {
        m_url = "HaltAlarm.aspx/GetHistoryHaltAlarm";
        m_data = '{organizationId: "' + organizationId + '", startTime: "' + startTime + '", endTime: "' + endTime + '"}';
    } 

    var mager = $.messager.alert('提示', '数据加载中...');
    $.ajax({
        type: "POST",
        url: m_url,
        data: m_data,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            mager.window('close');
            if (msg.d == "[]") {
                loadDataGrid("last", []);
               $.messager.alert('提示', '该时间段内没有报警数据!');
            }
            else {
                m_MsgData = jQuery.parseJSON(msg.d);
                loadDataGrid("last", m_MsgData);
            }                         
        },
        beforeSend: function (XMLHttpRequest) {
            mager;
        },
        error: handleError
    });
}

function handleError() {
    $('#gridMain_ReportTemplate').treegrid('loadData', []);
    $.messager.alert('失败', '获取数据失败');
}

function loadCombobox(type, myData)
{
    if (type == "first") {
        $('#MainMachine').combobox({
            valueField: 'VariableName',
            textField: 'VariableDescription',
            //singleSelect: true,
            panelHeight: 'auto',
            editable: false,
            loadFilter:function(data){
                data.unshift({ VariableName: '', VariableDescription: '全部' });
                return data;},
            onSelect: function (record) {
                variableId = record.VariableName;
            }
        });
    }
    else if(type == "last")
    {
        $('#MainMachine').combobox('loadData', myData.rows);
    }
}

function loadMainMachineList(m_OrganizationId)
{
    var organizationId = m_OrganizationId;
    $.ajax({
        type: "POST",
        url: "HaltAlarm.aspx/GetMainMachineList",
        data: '{organizationId: "' + organizationId  + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d == "[]") {
                alert("没有查询的数据");
            }
            else {
                m_MsgData = jQuery.parseJSON(msg.d);
                loadCombobox("last", m_MsgData);
            }
        }
    });


}