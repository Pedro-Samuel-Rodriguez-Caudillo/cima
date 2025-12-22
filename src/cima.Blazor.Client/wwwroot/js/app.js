window.cima = {
    // Navbar scroll listener
    addScrollListener: function (dotNetHelper) {
        let lastKnownScrollPosition = 0;
        let ticking = false;

        window.addEventListener('scroll', () => {
            lastKnownScrollPosition = window.scrollY;

            if (!ticking) {
                window.requestAnimationFrame(() => {
                    const isScrolled = lastKnownScrollPosition > 20;
                    dotNetHelper.invokeMethodAsync('OnScroll', isScrolled);
                    ticking = false;
                });

                ticking = true;
            }
        });
    }
};

// ========================================
// Scroll to Top Button
// ========================================

window.cimaScrollToTop = {
    dotNetRef: null,
    threshold: 300,

    init: function (dotNetHelper, threshold) {
        this.dotNetRef = dotNetHelper;
        this.threshold = threshold || 300;

        window.addEventListener('scroll', this.handleScroll.bind(this), { passive: true });
    },

    handleScroll: function () {
        const isVisible = window.scrollY > this.threshold;
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('UpdateVisibility', isVisible);
        }
    },

    scrollToTop: function () {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    },

    dispose: function () {
        window.removeEventListener('scroll', this.handleScroll);
        this.dotNetRef = null;
    }
};

// ========================================
// Lazy Image Loading with IntersectionObserver
// ========================================

window.cimaLazyImage = {
    observer: null,
    elements: new Map(),

    getObserver: function (rootMargin) {
        if (!this.observer) {
            this.observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const dotNetRef = this.elements.get(entry.target);
                        if (dotNetRef) {
                            dotNetRef.invokeMethodAsync('SetVisible');
                            this.observer.unobserve(entry.target);
                            this.elements.delete(entry.target);
                        }
                    }
                });
            }, {
                rootMargin: `${rootMargin}px 0px`,
                threshold: 0.01
            });
        }
        return this.observer;
    },

    observe: function (element, dotNetRef, rootMargin) {
        if (element) {
            this.elements.set(element, dotNetRef);
            this.getObserver(rootMargin).observe(element);
        }
    },

    unobserve: function (element) {
        if (element && this.observer) {
            this.observer.unobserve(element);
            this.elements.delete(element);
        }
    },

    dispose: function () {
        if (this.observer) {
            this.observer.disconnect();
            this.observer = null;
        }
        this.elements.clear();
    }
};

// ========================================
// Image Lazy Loading (legacy - kept for compatibility)
// ========================================

window.cimaImageLoader = {
    observer: null,

    init: function () {
        if (this.observer) return;

        this.observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.classList.remove('lazy');
                        img.classList.add('loaded');
                        this.observer.unobserve(img);
                    }
                }
            });
        }, {
            rootMargin: '50px 0px',
            threshold: 0.01
        });
    },

    observe: function (element) {
        if (this.observer && element) {
            this.observer.observe(element);
        }
    },

    dispose: function () {
        if (this.observer) {
            this.observer.disconnect();
            this.observer = null;
        }
    }
};

// ========================================
// Dropdown/Menu Outside Click Handler
// ========================================

window.cimaDropdown = {
    activeDropdown: null,

    open: function (elementId, dotNetRef) {
        this.close();
        this.activeDropdown = { elementId, dotNetRef };

        document.addEventListener('click', this.handleOutsideClick);
        document.addEventListener('keydown', this.handleEscape);
    },

    close: function () {
        if (this.activeDropdown && this.activeDropdown.dotNetRef) {
            this.activeDropdown.dotNetRef.invokeMethodAsync('Close');
        }
        this.activeDropdown = null;
        document.removeEventListener('click', this.handleOutsideClick);
        document.removeEventListener('keydown', this.handleEscape);
    },

    handleOutsideClick: function (event) {
        if (window.cimaDropdown.activeDropdown) {
            const element = document.getElementById(window.cimaDropdown.activeDropdown.elementId);
            if (element && !element.contains(event.target)) {
                window.cimaDropdown.close();
            }
        }
    },

    handleEscape: function (event) {
        if (event.key === 'Escape') {
            window.cimaDropdown.close();
        }
    }
};

// ========================================
// Focus Trap for Modals
// ========================================

window.cimaFocusTrap = {
    previousFocus: null,

    activate: function (elementId) {
        this.previousFocus = document.activeElement;

        const element = document.getElementById(elementId);
        if (!element) return;

        const focusableElements = element.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );

        if (focusableElements.length > 0) {
            focusableElements[0].focus();
        }

        element.addEventListener('keydown', this.handleTab.bind(this, element));
    },

    deactivate: function () {
        if (this.previousFocus) {
            this.previousFocus.focus();
            this.previousFocus = null;
        }
    },

    handleTab: function (element, event) {
        if (event.key !== 'Tab') return;

        const focusableElements = element.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );

        const firstFocusable = focusableElements[0];
        const lastFocusable = focusableElements[focusableElements.length - 1];

        if (event.shiftKey) {
            if (document.activeElement === firstFocusable) {
                lastFocusable.focus();
                event.preventDefault();
            }
        } else {
            if (document.activeElement === lastFocusable) {
                firstFocusable.focus();
                event.preventDefault();
            }
        }
    }
};

// ========================================
// Copy to Clipboard
// ========================================

window.cimaCopyToClipboard = async function (text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy text: ', err);
        return false;
    }
};

// ========================================
// Download File from Base64
// ========================================

window.downloadFileFromBase64 = function (fileName, base64, mimeType) {
    const link = document.createElement('a');
    link.href = `data:${mimeType};base64,${base64}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// ========================================
// Announce for Screen Readers (ARIA Live)
// ========================================

window.cimaAnnounce = function (message, priority = 'polite') {
    const announcer = document.getElementById('cima-announcer') || createAnnouncer();
    announcer.setAttribute('aria-live', priority);
    announcer.textContent = message;

    // Clear after announcement
    setTimeout(() => {
        announcer.textContent = '';
    }, 1000);
};

function createAnnouncer() {
    const announcer = document.createElement('div');
    announcer.id = 'cima-announcer';
    announcer.className = 'sr-only';
    announcer.setAttribute('aria-live', 'polite');
    announcer.setAttribute('aria-atomic', 'true');
    document.body.appendChild(announcer);
    return announcer;
}

// ========================================
// Preload Critical Resources
// ========================================

window.cimaPreload = {
    preloadImage: function (src) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(src);
            img.onerror = () => reject(new Error(`Failed to preload: ${src}`));
            img.src = src;
        });
    },

    preloadImages: function (srcs) {
        return Promise.all(srcs.map(src => this.preloadImage(src)));
    },

    prefetchPage: function (url) {
        const link = document.createElement('link');
        link.rel = 'prefetch';
        link.href = url;
        document.head.appendChild(link);
    }
};

// ========================================
// Performance Monitoring
// ========================================

window.cimaPerformance = {
    marks: {},

    mark: function (name) {
        this.marks[name] = performance.now();
        if (window.performance && window.performance.mark) {
            performance.mark(name);
        }
    },

    measure: function (name, startMark, endMark) {
        if (window.performance && window.performance.measure) {
            try {
                performance.measure(name, startMark, endMark);
            } catch (e) {
                console.warn('Performance measure failed:', e);
            }
        }

        const start = this.marks[startMark];
        const end = this.marks[endMark] || performance.now();
        return end - start;
    },

    getMetrics: function () {
        const metrics = {};

        // Navigation Timing
        if (window.performance && window.performance.timing) {
            const timing = window.performance.timing;
            metrics.pageLoad = timing.loadEventEnd - timing.navigationStart;
            metrics.domContentLoaded = timing.domContentLoadedEventEnd - timing.navigationStart;
            metrics.firstByte = timing.responseStart - timing.navigationStart;
        }

        // Core Web Vitals (if available)
        if (window.performance && window.performance.getEntriesByType) {
            const paintEntries = performance.getEntriesByType('paint');
            paintEntries.forEach(entry => {
                if (entry.name === 'first-contentful-paint') {
                    metrics.fcp = entry.startTime;
                }
            });
        }

        return metrics;
    },

    reportToConsole: function () {
        const metrics = this.getMetrics();
        console.group('CIMA Performance Metrics');
        console.table(metrics);
        console.groupEnd();
    }
};

// Auto-report performance in development
if (window.location.hostname === 'localhost') {
    window.addEventListener('load', () => {
        setTimeout(() => window.cimaPerformance.reportToConsole(), 1000);
    });
}

// ========================================
// Client-side Cache Utilities
// ========================================

window.cimaCache = {
    getKeys: function (prefix) {
        const keys = [];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key && key.startsWith(prefix)) {
                keys.push(key);
            }
        }
        return keys;
    },

    getSize: function () {
        let total = 0;
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key) {
                total += localStorage.getItem(key)?.length || 0;
            }
        }
        return total;
    },

    clearExpired: function (prefix) {
        const now = Date.now();
        const keys = this.getKeys(prefix);
        let cleared = 0;

        keys.forEach(key => {
            try {
                const item = JSON.parse(localStorage.getItem(key));
                if (item && item.expiresAt && new Date(item.expiresAt).getTime() < now) {
                    localStorage.removeItem(key);
                    cleared++;
                }
            } catch (e) {
                // Invalid JSON, remove it
                localStorage.removeItem(key);
                cleared++;
            }
        });

        return cleared;
    }
};

// Auto-clear expired cache on page load
window.addEventListener('load', () => {
    setTimeout(() => {
        const cleared = window.cimaCache.clearExpired('cima_cache_');
        if (cleared > 0) {
            console.log(`CIMA: Cleared ${cleared} expired cache items`);
        }
    }, 2000);
});

window.cimaScrollIntoViewWithin = (containerEl, targetEl) => {
    if (!containerEl || !targetEl) return;

    const cRect = containerEl.getBoundingClientRect();
    const tRect = targetEl.getBoundingClientRect();

    const top = (tRect.top - cRect.top) + containerEl.scrollTop - 12;
    containerEl.scrollTo({ top, behavior: "smooth" });
};

// ========================================
// Image Lightbox Modal (renders at body level)
// ========================================

window.cimaLightbox = {
    overlay: null,
    images: [],
    currentIndex: 0,
    dotNetRef: null,

    open: function (images, startIndex, dotNetRef) {
        this.images = images || [];
        this.currentIndex = startIndex || 0;
        this.dotNetRef = dotNetRef;

        if (this.images.length === 0) return;

        // Create overlay at body level
        this.overlay = document.createElement('div');
        this.overlay.id = 'cima-lightbox-overlay';
        this.overlay.style.cssText = `
            position: fixed;
            inset: 0;
            z-index: 999999;
            background: rgba(0, 0, 0, 0.95);
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 1rem;
            animation: cimaLightboxFadeIn 0.2s ease-out;
        `;

        // Add styles if not exists
        if (!document.getElementById('cima-lightbox-styles')) {
            const style = document.createElement('style');
            style.id = 'cima-lightbox-styles';
            style.textContent = `
                @keyframes cimaLightboxFadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                #cima-lightbox-overlay img {
                    max-width: 90vw;
                    max-height: 85vh;
                    object-fit: contain;
                    border-radius: 12px;
                    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
                }
                .cima-lightbox-btn {
                    position: absolute;
                    background: rgba(0, 0, 0, 0.6);
                    border: none;
                    color: white;
                    cursor: pointer;
                    padding: 12px;
                    border-radius: 50%;
                    transition: background 0.2s;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                .cima-lightbox-btn:hover {
                    background: rgba(0, 0, 0, 0.8);
                }
                .cima-lightbox-close {
                    top: 1rem;
                    right: 1rem;
                }
                .cima-lightbox-prev {
                    left: 1rem;
                    top: 50%;
                    transform: translateY(-50%);
                }
                .cima-lightbox-next {
                    right: 1rem;
                    top: 50%;
                    transform: translateY(-50%);
                }
                .cima-lightbox-counter {
                    position: absolute;
                    bottom: 1.5rem;
                    left: 50%;
                    transform: translateX(-50%);
                    background: rgba(0, 0, 0, 0.7);
                    color: white;
                    padding: 0.5rem 1rem;
                    border-radius: 9999px;
                    font-size: 0.875rem;
                    font-weight: 500;
                }
            `;
            document.head.appendChild(style);
        }

        this.render();
        document.body.appendChild(this.overlay);
        document.body.style.overflow = 'hidden';

        // Add keyboard listener
        document.addEventListener('keydown', this.handleKeydown);
    },

    render: function () {
        if (!this.overlay) return;

        const imgSrc = this.images[this.currentIndex];
        const hasMultiple = this.images.length > 1;

        this.overlay.innerHTML = `
            <button class="cima-lightbox-btn cima-lightbox-close" onclick="window.cimaLightbox.close()">
                <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                </svg>
            </button>
            
            <img src="${imgSrc}" alt="Imagen ${this.currentIndex + 1}" onclick="event.stopPropagation()" />
            
            ${hasMultiple ? `
                <button class="cima-lightbox-btn cima-lightbox-prev" onclick="window.cimaLightbox.prev(); event.stopPropagation()">
                    <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
                    </svg>
                </button>
                <button class="cima-lightbox-btn cima-lightbox-next" onclick="window.cimaLightbox.next(); event.stopPropagation()">
                    <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                    </svg>
                </button>
                <div class="cima-lightbox-counter">${this.currentIndex + 1} / ${this.images.length}</div>
            ` : ''}
        `;

        // Close on overlay click
        this.overlay.onclick = (e) => {
            if (e.target === this.overlay) {
                this.close();
            }
        };
    },

    next: function () {
        if (this.images.length > 1) {
            this.currentIndex = (this.currentIndex + 1) % this.images.length;
            this.render();
            this.notifyIndexChange();
        }
    },

    prev: function () {
        if (this.images.length > 1) {
            this.currentIndex = this.currentIndex === 0 ? this.images.length - 1 : this.currentIndex - 1;
            this.render();
            this.notifyIndexChange();
        }
    },

    notifyIndexChange: function () {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnLightboxIndexChanged', this.currentIndex);
        }
    },

    close: function () {
        if (this.overlay) {
            this.overlay.remove();
            this.overlay = null;
        }
        document.body.style.overflow = '';
        document.removeEventListener('keydown', this.handleKeydown);

        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnLightboxClosed');
        }
    },

    handleKeydown: function (e) {
        switch (e.key) {
            case 'Escape':
                window.cimaLightbox.close();
                break;
            case 'ArrowLeft':
                window.cimaLightbox.prev();
                break;
            case 'ArrowRight':
                window.cimaLightbox.next();
                break;
        }
    }
};

