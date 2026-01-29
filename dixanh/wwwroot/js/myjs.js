// wwwroot/js/myjs.js

// 1) Submit native form (Blazor gọi trực tiếp)
window.tvtSubmitForm = (form) => {
    if (form) form.submit();
};

// 2) Lấy timezone/offset của CLIENT
window.getClockClient = {
    timeZone: () => Intl.DateTimeFormat().resolvedOptions().timeZone,
    offsetNow: () => new Date().getTimezoneOffset(),
    // isoLocal: "yyyy-MM-ddTHH:mm:ss" hoặc "yyyy-MM-ddTHH:mm"
    offsetAtLocal: (isoLocal) => {
        const d = new Date(isoLocal);
        return d.getTimezoneOffset();
    }
};

// 3) Swiper init
window.callSwiperJSLogoBrand = () => {
    if (!window.Swiper) return;

    // tránh init trùng nhiều lần (nếu page nav nhiều)
    if (window.__tvtBrandSwiper) {
        try { window.__tvtBrandSwiper.destroy(true, true); } catch { }
        window.__tvtBrandSwiper = null;
    }

    window.__tvtBrandSwiper = new Swiper('.swiperJsBrand', {
        slidesPerView: "auto",
        loop: true,
        centeredSlides: true,
        speed: 2500,
        allowTouchMove: false,
        disableOnInteraction: false,
        autoplay: { delay: 0 }
    });
};

// (optional) debug
console.log("myjs loaded OK");