//登录缓存
function loginCache(val) {
    var date = new Date();
    date.setTime(date.getTime() + 24*60*60* 1000);
    $.cookie('xnhlogin', val, { expires: date });
}
//缓存信息
function cacheCookie(key,val) {
    var date = new Date();
    date.setTime(date.getTime() + 24 * 60 * 60 * 1000);
    $.cookie(key, val, { expires: date });
}
//获取cookie
function getCookie(key) {
    return $.cookie(key);
}
//是否已登录
function isLogin() {
    var ckie = $.cookie('xnhlogin');
    if (ckie == 'null' || ckie == '' || ckie==undefined) {
        return false;
    } else {
        return true;
    }
}
//功能业务检查是否已登录
function checkLogin() {
    if (isLogin()) {
        showUserInfo();
    } else {
        window.location = 'login.html';
    }
}
//退出按钮
function exit() {
    if (confirm("是否确认退出？")) {
        loginCache(null);
        window.location = 'login.html';
    }
}
//从缓存的用户信息显示在底部
function showUserInfo() {
    $("#loginInfo_DEP_NAME").html(getCookie('DEP_NAME'));
    $("#loginInfo_USER_CODE").html(getCookie('USER_CODE'));
    $("#loginInfo_DEP_ID").html(getCookie('DEP_ID'));
}
//显示时间
function showDate() {
    //分别获取年、月、日、时、分、秒  
    var myDate = new Date();
    var year = myDate.getFullYear();
    var month = myDate.getMonth() + 1;
    var date = myDate.getDate();
    var hours = myDate.getHours();
    var minutes = myDate.getMinutes();
    var seconds = myDate.getSeconds();

    //月份的显示为两位数字如09月  
    if (month < 10) {
        month = "0" + month;
    }
    if (date < 10) {
        date = "0" + date;
    }

    //时间拼接  
    var dateTime = year + "年" + month + "月" + date + "日" + hours + "时" + minutes + "分" + seconds + "秒";
    $("#loginInfo_dateTime").html(dateTime);
}
//每隔1秒，调用一次showDate() 
window.setInterval("showDate()", 1000);


function map2Json(map) {
    if (map != undefined && map != null) {
        var json = '{';
        for (var key in map) {
            json += "'" + key + "':'" + map[key] + "',";
        }
        json = json.substring(0, json.length - 1);
        json += '}';
        return json;
    }
}

/***********************遮罩相关********************/
function showMask(msg) {
    $("#mask_content").html(msg);
    $("#mask_background,#mask_content").show();
    conPosition();
 }

function closeMask() {
    $("#mask_background, #mask_content").hide();
}
function conPosition() {
    var oBackground = document.getElementById("mask_background");
    var dw = $(document).width();
    var dh = $(document).height();
    oBackground.style.width = dw + 'px';
    oBackground.style.height = dh + 'px';
    var oContent = document.getElementById("mask_content");
    var scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
    var l = (document.documentElement.clientWidth - oContent.offsetWidth) / 2;
    var t = ((document.documentElement.clientHeight - oContent.offsetHeight) / 2) + scrollTop;
    oContent.style.left = l + 'px';
    oContent.style.top = t + 'px';
}

$(window).resize(function () { conPosition(); });
/***********************遮罩相关 end********************/

function getJsonObj(str) {
    return eval("(" + str + ")");
}

/***********************读卡驱动配置********************/
var CPORT = 20; //串口号
var CSEAT = 1;  //1:下卡座 2:上卡座

function GetCardinfo() {
    var str1 = document.getElementById("CIccardCtrl").GetCardinfo(CPORT, CSEAT);
    return str1;
}
