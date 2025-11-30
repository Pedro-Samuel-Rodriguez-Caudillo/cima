// Fix para Microsoft Edge - Manejo de CSS y preload
(function () {
    'use strict';

    // Detectar Edge
    const isEdge = /Edg/.test(navigator.userAgent);
    
    if (!isEdge) return; // Solo aplicar en Edge

    console.log('[CIMA] Aplicando fix para Microsoft Edge');

    // 1. Prevenir race conditions en carga de CSS
    window.addEventListener('DOMContentLoaded', function() {
        // Forzar recarga de estilos ABP si hay problemas
        const abpStyles = document.querySelectorAll('link[href*="Volo.Abp"]');
        
        abpStyles.forEach(link => {
            // Remover preload hint si existe
            if (link.rel === 'preload') {
                link.rel = 'stylesheet';
            }
            
            // Asegurar que onload se dispare
            if (!link.onload) {
                link.onload = function() {
                    console.log('[CIMA] CSS cargado:', link.href);
                };
            }
            
            // Timeout de seguridad para forzar recarga si falla
            setTimeout(function() {
                if (!link.sheet) {
                    console.warn('[CIMA] Recargando CSS fallido:', link.href);
                    const href = link.href;
                    link.href = '';
                    link.href = href;
                }
            }, 3000);
        });
    });

    // 2. Limpiar cache de CSS en navegación
    let lastPath = window.location.pathname;
    
    const cleanupCssOnNavigation = function() {
        const currentPath = window.location.pathname;
        
        if (currentPath !== lastPath) {
            console.log('[CIMA] Navegación detectada, verificando CSS');
            lastPath = currentPath;
            
            // Verificar que todos los estilos estén cargados
            document.querySelectorAll('link[rel="stylesheet"]').forEach(link => {
                if (!link.sheet) {
                    console.warn('[CIMA] Recargando stylesheet después de navegación:', link.href);
                    const href = link.href;
                    link.href = '';
                    requestAnimationFrame(() => {
                        link.href = href;
                    });
                }
            });
        }
    };

    // Monitorear navegación SPA
    window.addEventListener('popstate', cleanupCssOnNavigation);
    
    // Monitorear cambios en DOM para detectar navegación Blazor
    const observer = new MutationObserver(cleanupCssOnNavigation);
    observer.observe(document.body, {
        childList: true,
        subtree: false
    });

    // 3. Suprimir SOLO warnings específicos de preload
    const originalConsoleWarn = console.warn;
    console.warn = function(...args) {
        const message = args.join(' ');
        
        // Suprimir SOLO estos warnings específicos:
        if (typeof message === 'string') {
            // 1. Warnings de preload no utilizado
            if (message.includes('preload') && message.includes('not used within a few seconds')) {
                return; // Suprimir
            }
            
            // 2. Warnings de IdentityClientConfiguration (son inofensivos)
            if (message.includes('IdentityClientConfiguration') && message.includes('AbpMvcClient')) {
                return; // Suprimir
            }
        }
        
        // Dejar pasar todos los demás warnings
        originalConsoleWarn.apply(console, args);
    };

    console.log('[CIMA] Fix de Edge aplicado correctamente');
})();
