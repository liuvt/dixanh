// wwwroot/js/tvtModal.js
window.tvtModal = {
    lock: function (on) {
        const html = document.documentElement;
        const body = document.body;

        if (on) {
            html.classList.add("overflow-hidden");
            body.classList.add("overflow-hidden");
            document.getElementById("siteFooter")?.classList.add("pointer-events-none");
        } else {
            html.classList.remove("overflow-hidden");
            body.classList.remove("overflow-hidden");
            document.getElementById("siteFooter")?.classList.remove("pointer-events-none");
        }
    }
};