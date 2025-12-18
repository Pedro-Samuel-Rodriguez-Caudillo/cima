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

