function loginCache(val) {
    var date = new Date();
    date.setTime(date.getTime() + 24*60*60* 1000);
    $.cookie('xnhlogin', val, { expires: date });
}

function isLogin() {
    var ckie = $.cookie('xnhlogin');
    if (ckie == 'null' || ckie == '') {
        return false;
    } else {
        return true;
    }
}