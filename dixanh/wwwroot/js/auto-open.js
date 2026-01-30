// Quản lý cửa sổ quảng cáo
window.adWindow = null;

window.openAd = function () {
    // Chỉ mở nếu chưa có hoặc đã bị đóng
    if (!window.adWindow || window.adWindow.closed) {
        // Mở tab trắng – lúc này đang ở trong sự kiện click, nên KHÔNG bị chặn
        window.adWindow = window.open('about:blank', '_blank');
    }
};

window.showAd = function (url) {
    // Nếu tab còn sống thì chỉ đổi location
    if (window.adWindow && !window.adWindow.closed) {
        window.adWindow.location.href = url;
    } else {
        // Phòng hờ – nếu vì lý do gì đó adWindow mất, mở luôn tab mới
        window.open(url, '_blank');
    }
};