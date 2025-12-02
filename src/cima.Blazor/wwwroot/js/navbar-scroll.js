// Navbar scroll handler para Blazor
window.cima = window.cima || {};

window.cima.addScrollListener = function(dotNetHelper) {
    if (!dotNetHelper) {
        console.warn('DotNetHelper no proporcionado');
        return;
    }

    let lastScrollState = false;

    function checkScroll() {
        const currentScroll = window.pageYOffset || document.documentElement.scrollTop;
        const isScrolled = currentScroll > 50;
        
        if (isScrolled !== lastScrollState) {
            lastScrollState = isScrolled;
            dotNetHelper.invokeMethodAsync('OnScroll', isScrolled);
        }
    }

    // Remove existing listener if any
    if (window.cima.scrollHandler) {
        window.removeEventListener('scroll', window.cima.scrollHandler);
    }

    // Create new handler
    window.cima.scrollHandler = function() {
        requestAnimationFrame(checkScroll);
    };

    // Add listener
    window.addEventListener('scroll', window.cima.scrollHandler, { passive: true });

    // Check initial state
    checkScroll();
};

// Smooth scroll para links internos
document.addEventListener('click', function(e) {
    const target = e.target.closest('a[href^="#"]');
    if (!target) return;
    
    const id = target.getAttribute('href');
    if (id === '#') return;
    
    const element = document.querySelector(id);
    if (!element) return;
    
    e.preventDefault();
    
    element.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
    });
});
