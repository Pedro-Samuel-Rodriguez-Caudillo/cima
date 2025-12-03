window.cima = {
    addScrollListener: function (dotNetHelper) {
        let lastKnownScrollPosition = 0;
        let ticking = false;

        window.addEventListener('scroll', () => {
            lastKnownScrollPosition = window.scrollY;

            if (!ticking) {
                window.requestAnimationFrame(() => {
                    // Umbral reducido de 50px a 20px para hacer el navbar más sensible
                    const isScrolled = lastKnownScrollPosition > 20;
                    dotNetHelper.invokeMethodAsync('OnScroll', isScrolled);
                    ticking = false;
                });

                ticking = true;
            }
        });
    }
};
