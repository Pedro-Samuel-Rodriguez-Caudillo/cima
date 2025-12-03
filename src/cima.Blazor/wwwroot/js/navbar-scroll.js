// Navbar scroll handler - Progressive Enhancement
// FUNCIONA INMEDIATAMENTE sin esperar a Blazor

(function() {
    'use strict';
    
    // === PARTE 1: FUNCIONALIDAD PURA JS (inmediata) ===
    
    var SCROLL_THRESHOLD = 50;
    var navbar = null;
    var isScrolled = false;
    
    function updateNavbarStyle(scrolled) {
        if (!navbar) return;
        
        if (scrolled) {
            // Remover clases transparentes
            navbar.classList.remove('bg-transparent', 'border-b', 'border-white/10');
            // Agregar clases solidas
            navbar.classList.add('bg-navy-950', 'shadow-lg');
        } else {
            // Remover clases solidas
            navbar.classList.remove('bg-navy-950', 'shadow-lg');
            // Agregar clases transparentes
            navbar.classList.add('bg-transparent', 'border-b', 'border-white/10');
        }
    }
    
    function checkScrollPure() {
        var currentScroll = window.pageYOffset || document.documentElement.scrollTop;
        var newIsScrolled = currentScroll > SCROLL_THRESHOLD;
        
        if (newIsScrolled !== isScrolled) {
            isScrolled = newIsScrolled;
            updateNavbarStyle(isScrolled);
        }
    }
    
    function initPureScrollHandler() {
        // Buscar el navbar por ID primero, luego por selector
        navbar = document.getElementById('main-navbar') || document.querySelector('header.fixed');
        
        if (navbar) {
            console.log('[cima] Navbar encontrado, inicializando scroll handler puro');
            
            // Verificar estado inicial
            checkScrollPure();
            
            // Listener optimizado con passive para mejor performance
            window.addEventListener('scroll', function() {
                requestAnimationFrame(checkScrollPure);
            }, { passive: true });
            
            return true;
        }
        return false;
    }
    
    // Ejecutar INMEDIATAMENTE cuando el DOM está listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPureScrollHandler);
    } else {
        // DOM ya está listo
        initPureScrollHandler();
    }
    
    // Reintentar si no se encontró (prerenderizado puede tardar)
    var retryCount = 0;
    var retryInterval = setInterval(function() {
        if (navbar || retryCount > 20) {
            clearInterval(retryInterval);
            return;
        }
        if (initPureScrollHandler()) {
            clearInterval(retryInterval);
        }
        retryCount++;
    }, 100);
    
    
    // === PARTE 2: INTEGRACIÓN CON BLAZOR (cuando esté disponible) ===
    
    window.cima = window.cima || {};
    
    window.cima.addScrollListener = function(dotNetHelper) {
        if (!dotNetHelper) {
            console.warn('[cima] DotNetHelper no proporcionado');
            return null;
        }

        var lastScrollState = isScrolled;
        var isDisposed = false;
        var isInvoking = false;

        function checkScrollBlazor() {
            if (isDisposed || !dotNetHelper || isInvoking) {
                return;
            }

            var currentScroll = window.pageYOffset || document.documentElement.scrollTop;
            var newIsScrolled = currentScroll > SCROLL_THRESHOLD;
            
            // Sincronizar estado
            isScrolled = newIsScrolled;
            
            if (newIsScrolled !== lastScrollState) {
                lastScrollState = newIsScrolled;
                isInvoking = true;
                
                dotNetHelper.invokeMethodAsync('OnScroll', newIsScrolled)
                    .then(function() {
                        isInvoking = false;
                    })
                    .catch(function(error) {
                        // No loguear errores de disposed - son esperados
                        isInvoking = false;
                        isDisposed = true;
                    });
            }
        }

        // Remover listener anterior si existe
        if (window.cima.scrollHandler) {
            window.removeEventListener('scroll', window.cima.scrollHandler);
        }

        window.cima.scrollHandler = function() {
            requestAnimationFrame(checkScrollBlazor);
        };

        window.addEventListener('scroll', window.cima.scrollHandler, { passive: true });

        // Sincronizar estado inicial
        if (isScrolled !== lastScrollState) {
            dotNetHelper.invokeMethodAsync('OnScroll', isScrolled).catch(function() {});
        }

        console.log('[cima] Blazor scroll listener registrado');

        return {
            dispose: function() {
                isDisposed = true;
                if (window.cima.scrollHandler) {
                    window.removeEventListener('scroll', window.cima.scrollHandler);
                    window.cima.scrollHandler = null;
                }
                console.log('[cima] Blazor scroll listener disposed');
            }
        };
    };

    window.cima.removeScrollListener = function() {
        if (window.cima.scrollHandler) {
            window.removeEventListener('scroll', window.cima.scrollHandler);
            window.cima.scrollHandler = null;
        }
    };
    
    
    // === PARTE 3: SMOOTH SCROLL para links internos ===
    
    document.addEventListener('click', function(e) {
        var target = e.target.closest('a[href^="#"]');
        if (!target) return;
        
        var id = target.getAttribute('href');
        if (id === '#') return;
        
        var element = document.querySelector(id);
        if (!element) return;
        
        e.preventDefault();
        
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    });
    
    
    // === PARTE 4: LOADING INDICATOR helper ===
    
    window.cima.hideLoadingIndicator = function() {
        var loader = document.getElementById('blazor-loading-indicator');
        if (loader) {
            loader.classList.add('fade-out');
            setTimeout(function() {
                loader.style.display = 'none';
            }, 300);
        }
    };
    
})();
