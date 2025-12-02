window.cima = {
    addScrollListener: function (dotNetHelper) {
        window.addEventListener('scroll', () => {
            const isScrolled = window.scrollY > 50;
            dotNetHelper.invokeMethodAsync('OnScroll', isScrolled);
        });
    }
};
