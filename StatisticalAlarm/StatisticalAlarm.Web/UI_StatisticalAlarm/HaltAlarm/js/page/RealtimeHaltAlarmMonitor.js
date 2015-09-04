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
function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
        realtimeAlarm();
        //g_timer = setTimeout("realtimeAlarm()", 300000);
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
    var organizationId = $('#organizationId').val();
    $.ajax({
        type: "POST",
        url: "HaltAlarm.aspx/GetRealTimeHaltAlarm",
        data: '{organizationId: "' + organizationId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,//同步执行
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
            loadDataGrid("last", m_MsgData);
            setTimer();
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
                    { field: 'HaltTime', title: '时间', width: 150 },
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
    $.ajax({
        type: "POST",
        url: "HaltAlarm.aspx/GetHistoryHaltAlarm",
        data: '{organizationId: "' + organizationId + '", startTime: "' + startTime + '", endTime: "' + endTime + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_MsgData = jQuery.parseJSON(msg.d);
            loadDataGrid("last", m_MsgData);
        },
        error: handleError
    });
}

function handleError() {
    $('#gridMain_ReportTemplate').treegrid('loadData', []);
    $.messager.alert('失败', '获取数据失败');
}