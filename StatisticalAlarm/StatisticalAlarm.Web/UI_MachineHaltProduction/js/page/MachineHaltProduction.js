$(function () {
    InitDate();
    loadTreeGrid("first");
});
//初始化日期框
function InitDate() {
    var nowDate = new Date();
    var beforeDate = new Date();
    beforeDate.setDate(nowDate.getDate() - 1);
    var nowString = nowDate.getFullYear() + '-' + (nowDate.getMonth() + 1) + '-' + nowDate.getDate() + " " + nowDate.getHours() + ":" + nowDate.getMinutes() + ":" + nowDate.getSeconds();
    var beforeString = beforeDate.getFullYear() + '-' + (beforeDate.getMonth() + 1) + '-' + beforeDate.getDate() + " 00:00:00";
    $('#startDate').datetimebox('setValue', beforeString);
    $('#endDate').datetimebox('setValue', nowString);
}
var mOrganizationId = '';
function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
    mOrganizationId = node.OrganizationId;
}
function loadTreeGrid(type, myData) {
    if (type == "first") {
        $('#Windows_Report').treegrid({
            columns: [[
                    { field: 'text', title: '设备名称', width: 180 },
                    { field: 'HaltTime', title: '停机时间', width: 130 },
                    { field: 'RecoverTime', title: '开机时间', width: 130 },
                    { field: 'HaltSumTime', title: '停机时长', width: 90 },
                    { field: 'HaltPowerConsumption', title: '电量', width: 80 }
            ]],          
            fit: true,
            toolbar: "#toolbar_ReportTemplate",
            idField:"id",
            treeField:"text",
            rownumbers: true,
            singleSelect: true,
            striped: true,
            data: [],
        })
    }
    else {
        $('#Windows_Report').treegrid("loadData", myData);
    }
}
function query(){
    var startTime = $('#startDate').datebox('getValue');
    var endTime = $('#endDate').datebox('getValue');
    if (mOrganizationId == "") {
        $.messager.alert('警告', '请选择组织机构');
        return;
    }
    var win = $.messager.progress({
        title: '请稍后',
        msg: '数据载入中...'
    });
    $.ajax({
        type: "POST",
        url: "MachineHaltProduction.aspx/GetMachineHaltInformation",
        data: "{mOrganizationId:'" + mOrganizationId + "',startTime:'" + startTime + "',endTime:'" + endTime + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $.messager.progress('close');
            var myData = jQuery.parseJSON(msg.d);
            if (myData != undefined && myData.length == 0) {
                loadTreeGrid("last", []);
                $.messager.alert('提示', '没有查询到记录！');
            } else {
                loadTreeGrid("last", myData);
            }
        },
        beforeSend: function (XMLHttpRequest) {
            //alert('远程调用开始...');
            win;
        },
        error: function () {
            $.messager.progress('close');
            $("#Windows_Report").treegrid('loadData', []);
            $.messager.alert('失败', '加载失败！');
        }
    })
}