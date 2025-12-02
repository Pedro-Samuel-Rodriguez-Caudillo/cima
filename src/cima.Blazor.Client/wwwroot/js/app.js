window.cima = {
    addScrollListener: function (dotNetHelper) {
        let lastKnownScrollPosition = 0;
        let ticking = false;

        window.addEventListener('scroll', () => {
            lastKnownScrollPosition = window.scrollY;

            if (!ticking) {
                window.requestAnimationFrame(() => {
                    const isScrolled = lastKnownScrollPosition > 50;
                    dotNetHelper.invokeMethodAsync('OnScroll', isScrolled);
                    ticking = false;
                });

                ticking = true;
            }
        });
    }
};
