var g_timer;

$(function () {
    InitDate();
    loadDataGrid("first");
    LoadCombobox();
});
//初始化日期框
function InitDate() {
    var nowDate = new Date();
    var beforeDate = new Date();
    beforeDate.setDate(nowDate.getDate() - 10);
    var nowString = nowDate.getFullYear() + '-' + (nowDate.getMonth() + 1) + '-' + nowDate.getDate() + " " + nowDate.getHours() + ":" + nowDate.getMinutes() + ":" + nowDate.getSeconds();
    var beforeString = beforeDate.getFullYear() + '-' + (beforeDate.getMonth()+1) + '-' + beforeDate.getDate() + " 00:00:00";
    $('#startDate').datetimebox('setValue', beforeString);
    $('#endDate').datetimebox('setValue', nowString);
}
var AlarmType = "";
function LoadCombobox() {
    $('#alarmType').combobox({
        valueField: 'valueName',
        textField: 'typeName',
        panelHeight: 'auto',
        data: [{
            typeName: '电耗报警',
            valueName: '电耗超标'
        },{
            typeName: '煤耗报警',
            valueName: '煤耗超标'
        },{
            typeName: '功率报警',
            valueName: '功率超标'
        }],
        onSelect: function (record) {
             AlarmType = record.valueName;
        }
    });
}
function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
        realtimeAlarm();
}

function setTimer() {
    g_timer = setTimeout("realtimeAlarm()", 300000);
}
function setHistory() {
    clearTimeout(g_timer);
    $('#gridMain_ReportTemplate').datagrid("loadData", []);
    $(".historyTool").show();
}
function realtimeAlarm() {
    if (document.getElementsByName("alarmType")[0].checked == false) {
        clearTimeout(g_timer);
        return;
    }
    else {
        $(".historyTool").hide();
    }
    var organizationId = $('#organizationId').val();
    var win = $.messager.progress({
        title: '请稍后',
        msg: '数据载入中...'
    });
    $.ajax({
        type: "POST",
        url: "EnergyConsumptionAlarm.aspx/GetRealTimeAlarm",
        data: '{organizationId: "' + organizationId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,//同步执行
        success: function (msg) {
            $.messager.progress('close');
            m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData.total == 0) {
                loadDataGrid("last", []);
                $.messager.alert('提示', '当前没有报警！');
            }
            else {                
                loadDataGrid("last", m_MsgData);
                setTimer();
            }
        },
        beforeSend: function (XMLHttpRequest) {
            win;
        },
        error: function () {
            $.messager.progress('close');
            setTimer()
        }
    });
}
function loadDataGrid(type, myData) {
    if (type == "first") {
        $('#gridMain_ReportTemplate').datagrid({
            columns: [[
                    { field: 'AlarmDateTime', title: '报警开始时间', width: 130 },
                    { field: 'Name', title: '工序', width: 200 },
                 ///   { field: 'TimeSpan', title: '报警时间段', width: 200 },
                    { field: 'EnergyConsumptionType', title: '报警类型', width: 70 },
                    { field: 'StandardValue', title: '报警上限', width: 70 },
                    { field: 'ActualValue', title: '报警实际值', width: 80},
                    { field: 'Superscale', title: '超标百分比', width: 80 }
            ]],
            fit: true,
            pagination: true,
            pageSize:50,
            toolbar: "#toolbar_ReportTemplate",
            rownumbers: true,
            singleSelect: true,
            striped: true,
            data: []
        })
    }
    else {
        $('#gridMain_ReportTemplate').datagrid('loadData', myData);
    }
}

function QueryReportFun() {
    var organizationId = $('#organizationId').val();
    var myAlarmType = AlarmType;
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
    var win = $.messager.progress({
        title: '请稍后',
        msg: '数据载入中...'
    });
    $.ajax({
        type: "POST",
        url: "EnergyConsumptionAlarm.aspx/GetHistoryAlarm",
        data: '{organizationId: "' + organizationId + '", startTime: "' + startTime + '", endTime: "' + endTime + '", alarmType: "' + myAlarmType + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $.messager.progress('close');
            m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData.total == 0) {
                loadDataGrid("last", []);
                $.messager.alert('提示', '该时间段内没有报警数据...');
            }
            else {
                loadDataGrid("last", m_MsgData);
            }
        },
        beforeSend: function (XMLHttpRequest) {
            win;
        },
        error: function () {
            $.messager.progress('close');
            handleError
        }
    });
}

function handleError() {
    $('#gridMain_ReportTemplate').datagrid('loadData', []);
    $.messager.alert('失败', '获取数据失败');
}
//datagrid最下面分页栏使用
function pagerFilter(data) {
    if (typeof data.length == 'number' && typeof data.splice == 'function') {	// is array
        data = {
            total: data.length,
            rows: data
        }
    }
    var dg = $(this);
    var opts = dg.datagrid('options');
    var pager = dg.datagrid('getPager');
    pager.pagination({
        onSelectPage: function (pageNum, pageSize) {
            opts.pageNumber = pageNum;
            opts.pageSize = pageSize;
            pager.pagination('refresh', {
                pageNumber: pageNum,
                pageSize: pageSize
            });
            dg.datagrid('loadData', data);
        }
    });
    if (!data.originalRows) {
        data.originalRows = (data.rows);
    }
    var start = (opts.pageNumber - 1) * parseInt(opts.pageSize);
    var end = start + parseInt(opts.pageSize);
    data.rows = (data.originalRows.slice(start, end));
    return data;
}