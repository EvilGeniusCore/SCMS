// Clean Blog navbar: fix to top on scroll, hide/show on direction
(function () {
    const nav = document.getElementById("mainNav");
    if (!nav) return;

    let lastScroll = 0;
    const threshold = 100;

    window.addEventListener("scroll", function () {
        const currentScroll = window.pageYOffset;

        if (currentScroll > threshold) {
            nav.classList.add("is-fixed");
        } else {
            nav.classList.remove("is-fixed");
        }

        lastScroll = currentScroll;
    });
})();
