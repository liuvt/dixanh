//Submit form từ JS
window.dixanhSubmitForm = async (form) => {
    if (form) form.submit();
};

//SwiperJS: trượt logo brand
window.callSwiperJSLogoBrand = async () => {
    const swiper = new Swiper('.swiperJsBrand', {
        slidesPerView: "auto",
        loop: true,
        centeredSliders: true,
        speed: 2500,
        allowTouchMove: false,
        disableOnInteraction: false,
        autoplay: {
            delay: -10,
        },
    });
}