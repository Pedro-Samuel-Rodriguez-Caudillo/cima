// Fix para Microsoft Edge - Manejo de CSS y preload
(function () {
    'use strict';

    // Detectar Edge
    const isEdge = /Edg/.test(navigator.userAgent);
    
    if (!isEdge) return; // Solo aplicar en Edge

    console.log('[CIMA] Aplicando fix para Microsoft Edge');

    // SOLO suprimir warnings específicos - NO tocar CSS
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

    console.log('[CIMA] Fix de Edge aplicado correctamente (solo supresión de warnings)');
})();
