/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Pages/**/*.{razor,html}',
        './Components/**/*.{razor,html}',
        './Shared/**/*.{razor,html}',
        './Layouts/**/*.{razor,html}',
        './*.razor',
        './wwwroot/index.html',
        // Incluir archivos del proyecto Blazor Server para que Tailwind escanee las clases
        '../cima.Blazor/Pages/**/*.{cshtml,razor,html}',
        '../cima.Blazor/Components/**/*.{razor,html}',
    ],
    // Modo JIT para reducir tamaño del CSS generado
    mode: 'jit',
    darkMode: 'class',

    // Configurar safelist solo para clases que realmente usamos dinámicamente
    safelist: [
        // Clases que se generan dinámicamente
        'cima-badge-primary',
        'cima-badge-success',
        'cima-badge-warning',
        'cima-badge-danger',
        'cima-tab-active',
        'flipped',
        'line-clamp-2',
        // Colores para badges de transacción
        'bg-emerald-500',
        'bg-blue-500',
        'bg-amber-500',
        'text-white',
        // Grid responsive
        'grid-cols-1',
        'sm:grid-cols-2',
        'lg:grid-cols-3',
        'xl:grid-cols-4',
        // Clases de autenticación (usadas en Login.cshtml fuera del alcance de Tailwind)
        'auth-page',
        'auth-container',
        'auth-logo',
        'auth-card',
        'auth-card-centered',
        'auth-title',
        'auth-subtitle',
        'auth-form-group',
        'auth-label',
        'auth-input',
        'auth-password-wrapper',
        'auth-password-toggle',
        'auth-options',
        'auth-remember',
        'auth-checkbox',
        'auth-link',
        'auth-link-light',
        'auth-alert',
        'auth-alert-error',
        'auth-submit',
        'auth-back-link',
        'auth-icon-circle',
        'auth-icon-success',
        'auth-icon-error',
        'auth-icon-warning',
        'auth-requirements',
        'auth-req-title',
        'auth-req-list',
        'auth-req-item',
        'auth-req-valid',
        'auth-text-muted',
        'auth-text-link',
        'auth-user-info',
        'auth-user-avatar',
        'auth-user-details',
        'auth-user-name',
        'auth-user-email',
        'auth-buttons',
        'auth-btn',
        'auth-btn-primary',
        'auth-btn-secondary',
        'auth-btn-danger',
        'auth-note',
        'auth-link-inline',
        'auth-loading',
        'auth-spinner',
    ],

    theme: {
        extend: {
            // === PALETA CORPORATIVA MEJORADA ===
            colors: {
                // Azul corporativo centralizado (#1e3a8a) y derivados
                navy: {
                    50: '#eef2ff',
                    100: '#e0e7ff',
                    200: '#c7d2fe',
                    300: '#a5b4fc',
                    400: '#818cf8',
                    500: '#5f70e0',
                    600: '#3f51c5',
                    700: '#172e6b', // Principal - Más oscuro
                    800: '#122452',
                    900: '#0d1a3d',
                    950: '#081129',
                },
                // Alias explícito para usar como color primario
                primary: {
                    50: '#eef2ff',
                    100: '#e0e7ff',
                    200: '#c7d2fe',
                    300: '#a5b4fc',
                    400: '#818cf8',
                    500: '#5f70e0',
                    600: '#3f51c5',
                    700: '#172e6b',
                    800: '#122452',
                    900: '#0d1a3d',
                    950: '#081129',
                },

                // Grises corporativos suaves
                neutral: {
                    50: '#f8f9fa',
                    100: '#f1f3f5',
                    200: '#e9ecef',
                    300: '#dee2e6',
                    400: '#ced4da',
                    500: '#adb5bd',
                    600: '#6c757d',
                    700: '#495057',
                    800: '#343a40',
                    900: '#212529',
                    950: '#0d0f11',
                },

                // Fondos sutiles
                section: {
                    light: '#f8f9fa',
                    DEFAULT: '#f1f3f5',
                    dark: '#e9ecef',
                },

                // Estados semánticos corporativos
                success: {
                    light: '#d4edda',
                    DEFAULT: '#28a745',
                    dark: '#1e7e34',
                },
                danger: {
                    light: '#f8d7da',
                    DEFAULT: '#dc3545',
                    dark: '#bd2130',
                },
                warning: {
                    light: '#fff3cd',
                    DEFAULT: '#ffc107',
                    dark: '#d39e00',
                },
                info: {
                    light: '#d1ecf1',
                    DEFAULT: '#17a2b8',
                    dark: '#117a8b',
                },
            },

            // === TIPOGRAFÍA CORPORATIVA ===
            fontFamily: {
                // Interfaz: sans-serif moderna (Inter, Helvetica Neue, etc.)
                sans: [
                    'Inter',
                    '-apple-system',
                    'BlinkMacSystemFont',
                    'Segoe UI',
                    'Roboto',
                    'sans-serif',
                ],
                // Títulos: sans-serif geométrica
                display: [
                    'Inter',
                    'Montserrat',
                    'system-ui',
                    'sans-serif',
                ],
            },

            fontSize: {
                // Escala modular clara
                'xs': ['0.75rem', { lineHeight: '1.25rem', letterSpacing: '0.025em' }],
                'sm': ['0.875rem', { lineHeight: '1.5rem', letterSpacing: '0.01em' }],
                'base': ['1rem', { lineHeight: '1.75rem' }],
                'lg': ['1.125rem', { lineHeight: '1.875rem' }],
                'xl': ['1.25rem', { lineHeight: '2rem' }],
                '2xl': ['1.5rem', { lineHeight: '2.25rem' }],
                '3xl': ['1.875rem', { lineHeight: '2.5rem' }],
                '4xl': ['2.25rem', { lineHeight: '2.75rem' }],
                '5xl': ['3rem', { lineHeight: '3.5rem', letterSpacing: '-0.02em' }],
                '6xl': ['3.75rem', { lineHeight: '4rem', letterSpacing: '-0.025em' }],
                '7xl': ['4.5rem', { lineHeight: '4.75rem', letterSpacing: '-0.03em' }],
            },

            // === ESPACIADO ===
            spacing: {
                '18': '4.5rem',
                '88': '22rem',
                '112': '28rem',
                '128': '32rem',
            },

            maxWidth: {
                '8xl': '88rem',
                '9xl': '96rem',
            },

            // === BORDES CORPORATIVOS ===
            borderRadius: {
                'none': '0',
                'sm': '0.375rem',
                DEFAULT: '0.5rem',
                'md': '0.625rem',
                'lg': '0.75rem',
                'xl': '1rem',
                '2xl': '1.25rem',
                'full': '9999px',
            },

            // === SOMBRAS PROFESIONALES ===
            boxShadow: {
                'sm': '0 1px 2px 0 rgba(0, 0, 0, 0.04)',
                DEFAULT: '0 2px 4px 0 rgba(0, 0, 0, 0.06)',
                'md': '0 4px 8px -1px rgba(0, 0, 0, 0.08)',
                'lg': '0 8px 16px -4px rgba(0, 0, 0, 0.1)',
                'xl': '0 16px 32px -8px rgba(0, 0, 0, 0.12)',
                '2xl': '0 24px 48px -12px rgba(0, 0, 0, 0.15)',
                'inner': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.05)',
                'none': 'none',
            },

            // === TRANSICIONES SUAVES ===
            transitionDuration: {
                '250': '250ms',
                '350': '350ms',
            },

            transitionTimingFunction: {
                'smooth': 'cubic-bezier(0.4, 0, 0.2, 1)',
            },

            // === ANIMACIONES ===
            keyframes: {
                'fade-in': {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                'slide-up': {
                    '0%': { transform: 'translateY(10px)', opacity: '0' },
                    '100%': { transform: 'translateY(0)', opacity: '1' },
                },
            },

            animation: {
                'fade-in': 'fade-in 0.3s ease-out',
                'slide-up': 'slide-up 0.4s ease-out',
            },
        },
    },
    plugins: [
        require('@tailwindcss/forms')({
            strategy: 'class',
        }),
        require('@tailwindcss/typography'),

        // Plugin personalizado para componentes reutilizables
        function ({ addComponents, theme }) {
            addComponents({
                // === BOTONES ===
                '.cima-btn': {
                    display: 'inline-flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    padding: '0.625rem 1.5rem',
                    fontSize: theme('fontSize.base[0]'),
                    fontWeight: '500',
                    lineHeight: theme('fontSize.base[1].lineHeight'),
                    borderRadius: theme('borderRadius.lg'),
                    transition: 'all 0.2s ease-in-out',
                    cursor: 'pointer',
                    border: 'none',
                    outline: 'none',
                    '&:focus-visible': {
                        outline: `2px solid ${theme('colors.navy.500')}`,
                        outlineOffset: '2px',
                    },
                },
                '.cima-btn-primary': {
                    backgroundColor: theme('colors.navy.600'),
                    color: 'white',
                    '&:hover': {
                        backgroundColor: theme('colors.navy.700'),
                    },
                    '&:active': {
                        backgroundColor: theme('colors.navy.800'),
                    },
                },
                '.cima-btn-secondary': {
                    backgroundColor: theme('colors.neutral.100'),
                    color: theme('colors.neutral.900'),
                    '&:hover': {
                        backgroundColor: theme('colors.neutral.200'),
                    },
                },
                '.cima-btn-outline': {
                    backgroundColor: 'transparent',
                    color: theme('colors.navy.600'),
                    border: `2px solid ${theme('colors.navy.600')}`,
                    '&:hover': {
                        backgroundColor: theme('colors.navy.50'),
                    },
                },

                // === INPUTS ===
                '.cima-input': {
                    width: '100%',
                    padding: '0.625rem 0.875rem',
                    fontSize: theme('fontSize.base[0]'),
                    lineHeight: theme('fontSize.base[1].lineHeight'),
                    color: theme('colors.neutral.900'),
                    backgroundColor: 'white',
                    border: `1px solid ${theme('colors.neutral.300')}`,
                    borderRadius: theme('borderRadius.lg'),
                    transition: 'border-color 0.2s, box-shadow 0.2s',
                    '&:focus': {
                        outline: 'none',
                        borderColor: theme('colors.navy.500'),
                        boxShadow: `0 0 0 3px ${theme('colors.navy.50')}`,
                    },
                    '&::placeholder': {
                        color: theme('colors.neutral.400'),
                    },
                },

                // === CARDS ===
                '.cima-card': {
                    backgroundColor: 'white',
                    borderRadius: theme('borderRadius.xl'),
                    overflow: 'hidden',
                    transition: 'transform 0.2s, box-shadow 0.2s',
                },

                // === SECCIONES ===
                '.cima-section': {
                    width: '100%',
                    padding: '4rem 1rem',
                },
                '.cima-section-alt': {
                    backgroundColor: theme('colors.neutral.50'),
                },

                // === CONTENEDOR ===
                '.cima-container': {
                    width: '100%',
                    maxWidth: '1280px', // Explicit max-width to prevent sizing issues
                    marginLeft: 'auto',
                    marginRight: 'auto',
                    paddingLeft: '1rem',
                    paddingRight: '1rem',
                },
            });
        },
    ],
};
