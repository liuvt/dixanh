// TVT Carousel - A simple carousel implementation
// Trượt ảnh / nội dung tự động với nút điều khiển và chấm chỉ mục
window.initTvtCarousel = function () {
    const carousels = document.querySelectorAll('[data-tvt-carousel]');
    if (!carousels.length) return;

    carousels.forEach(root => {
        // tránh gắn event nhiều lần nếu Blazor render lại
        if (root.dataset.tvtInited === "1") return;
        root.dataset.tvtInited = "1";
        // console.log('Init tvtCarousel', root);
        const track = root.querySelector('[data-tvt-carousel-track]');
        const items = root.querySelectorAll('[data-tvt-carousel-item]');
        const dots = root.querySelectorAll('[data-tvt-carousel-dot]');
        const prevBtn = root.querySelector('[data-tvt-carousel-prev]');
        const nextBtn = root.querySelector('[data-tvt-carousel-next]');
        const interval = parseInt(root.dataset.interval || '5000', 10);

        if (!track || items.length === 0) return;

        let index = 0;
        let timer = null;

        function goTo(i) {
            index = (i + items.length) % items.length;

            const width = root.clientWidth;
            const offset = -index * width;

            track.style.transform = `translateX(${offset}px)`;

            // cập nhật dots
            dots.forEach((dot, di) => {
                const active = di === index;
                dot.classList.toggle('scale-125', active);
                dot.classList.toggle('bg-pink-500', active);
                dot.classList.toggle('bg-slate-300', !active);
            });
        }

        function startAuto() {
            if (!interval || interval <= 0) return;
            stopAuto();
            timer = setInterval(() => goTo(index + 1), interval);
        }

        function stopAuto() {
            if (timer) {
                clearInterval(timer);
                timer = null;
            }
        }

        // nút next/prev
        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                goTo(index + 1);
                startAuto();
            });
        }

        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                goTo(index - 1);
                startAuto();
            });
        }

        // dots
        dots.forEach((dot, di) => {
            dot.addEventListener('click', () => {
                goTo(di);
                startAuto();
            });
        });

        // hover pause
        root.addEventListener('mouseenter', stopAuto);
        root.addEventListener('mouseleave', startAuto);

        goTo(0);
        startAuto();
    });
};