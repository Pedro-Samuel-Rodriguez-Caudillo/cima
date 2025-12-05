window.cima = {
    addScrollListener: function (dotNetHelper) {
        let lastKnownScrollPosition = 0;
        let ticking = false;

        window.addEventListener('scroll', () => {
            lastKnownScrollPosition = window.scrollY;

            if (!ticking) {
                window.requestAnimationFrame(() => {
                    const isScrolled = lastKnownScrollPosition > 20; // Umbral reducido de 50px a 20px para hacer el cambio más sensible
                    dotNetHelper.invokeMethodAsync('OnScroll', isScrolled);
                    ticking = false;
                });

                ticking = true;
            }
        });
    }
};
