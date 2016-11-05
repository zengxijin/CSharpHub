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

function getCookie(key) {
    return $.cookie(key);
}

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
window.setInterval("showDate()", 1000);//每隔1秒，调用一次showDate()  