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
// Image Lazy Loading with Intersection Observer
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

