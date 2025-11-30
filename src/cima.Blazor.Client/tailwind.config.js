/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Pages/**/*.{razor,html}',
        './Components/**/*.{razor,html}',
        './Shared/**/*.{razor,html}',
        './Layouts/**/*.{razor,html}',
        './*.razor',
        './wwwroot/index.html'
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
    ],
    
    theme: {
        extend: {
            // === SISTEMA DE COLORES - MINIMALISMO SUIZO ===
            colors: {
                // Azul oscuro principal (navy)
                navy: {
                    50: '#e6f0ff',
                    100: '#b3d1ff',
                    200: '#80b3ff',
                    300: '#4d94ff',
                    400: '#1a75ff',
                    500: '#0047AB', // Principal - azul oscuro corporativo
                    600: '#003d91',
                    700: '#003377',
                    800: '#00295d',
                    900: '#001f43',
                    950: '#001529',
                },
                
                // Grises neutrales (alta legibilidad)
                neutral: {
                    50: '#fafafa',
                    100: '#f5f5f5',
                    200: '#e5e5e5',
                    300: '#d4d4d4',
                    400: '#a3a3a3',
                    500: '#737373',
                    600: '#525252',
                    700: '#404040',
                    800: '#262626',
                    900: '#171717',
                    950: '#0a0a0a',
                },
                
                // Fondos de sección (diferenciación sutil)
                section: {
                    light: '#fafafa',
                    DEFAULT: '#f5f5f5',
                    dark: '#e5e5e5',
                },
                
                // Estados semánticos
                success: {
                    light: '#d1fae5',
                    DEFAULT: '#10b981',
                    dark: '#047857',
                },
                danger: {
                    light: '#fee2e2',
                    DEFAULT: '#ef4444',
                    dark: '#dc2626',
                },
                warning: {
                    light: '#fef3c7',
                    DEFAULT: '#f59e0b',
                    dark: '#d97706',
                },
                info: {
                    light: '#dbeafe',
                    DEFAULT: '#3b82f6',
                    dark: '#1d4ed8',
                },
            },

            // === TIPOGRAFÍA - MINIMALISMO SUIZO ===
            fontFamily: {
                // Interfaz: sans-serif moderna (Inter, Helvetica Neue, etc.)
                sans: [
                    'Inter',
                    '-apple-system',
                    'BlinkMacSystemFont',
                    'Segoe UI',
                    'Roboto',
                    'Helvetica Neue',
                    'Arial',
                    'sans-serif',
                ],
                // Títulos: sans-serif geométrica
                display: [
                    'Montserrat',
                    'Inter',
                    'system-ui',
                    'sans-serif',
                ],
            },

            fontSize: {
                // Escala modular clara
                'xs': ['0.75rem', { lineHeight: '1rem' }],
                'sm': ['0.875rem', { lineHeight: '1.25rem' }],
                'base': ['1rem', { lineHeight: '1.5rem' }],
                'lg': ['1.125rem', { lineHeight: '1.75rem' }],
                'xl': ['1.25rem', { lineHeight: '1.75rem' }],
                '2xl': ['1.5rem', { lineHeight: '2rem' }],
                '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
                '4xl': ['2.25rem', { lineHeight: '2.5rem' }],
                '5xl': ['3rem', { lineHeight: '1' }],
                '6xl': ['3.75rem', { lineHeight: '1' }],
            },

            // === ESPACIADO Y LAYOUT ===
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

            // === BORDES Y RADIOS ===
            borderRadius: {
                'none': '0',
                'sm': '0.25rem',
                DEFAULT: '0.375rem',
                'md': '0.5rem',
                'lg': '0.75rem',
                'xl': '1rem',
                '2xl': '1.5rem',
                'full': '9999px',
            },

            // === SOMBRAS - SUTILES Y MINIMALISTAS ===
            boxShadow: {
                'sm': '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
                DEFAULT: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
                'md': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
                'lg': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
                'xl': '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
                'inner': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.06)',
                'none': 'none',
            },

            // === TRANSICIONES ===
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
                'slide-down': {
                    '0%': { transform: 'translateY(-10px)', opacity: '0' },
                    '100%': { transform: 'translateY(0)', opacity: '1' },
                },
                'draw-line': {
                    '0%': { strokeDashoffset: '1000' },
                    '100%': { strokeDashoffset: '0' },
                },
                'flip-horizontal': {
                    '0%': { transform: 'rotateY(0deg)' },
                    '100%': { transform: 'rotateY(180deg)' },
                },
                'flip-back': {
                    '0%': { transform: 'rotateY(180deg)' },
                    '100%': { transform: 'rotateY(0deg)' },
                },
            },

            animation: {
                'fade-in': 'fade-in 0.3s ease-out',
                'slide-up': 'slide-up 0.4s ease-out',
                'slide-down': 'slide-down 0.4s ease-out',
                'draw-line': 'draw-line 2s ease-out forwards',
                'flip': 'flip-horizontal 0.6s ease-in-out',
                'flip-back': 'flip-back 0.6s ease-in-out',
            },
        },
    },
    plugins: [
        require('@tailwindcss/forms')({
            strategy: 'class',
        }),
        require('@tailwindcss/typography'),
        
        // Plugin personalizado para componentes reutilizables
        function({ addComponents, theme }) {
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
                    borderRadius: theme('borderRadius.md'),
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
                    backgroundColor: theme('colors.navy.500'),
                    color: 'white',
                    '&:hover': {
                        backgroundColor: theme('colors.navy.600'),
                    },
                    '&:active': {
                        backgroundColor: theme('colors.navy.700'),
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
                    color: theme('colors.navy.500'),
                    border: `1px solid ${theme('colors.navy.500')}`,
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
                    borderRadius: theme('borderRadius.md'),
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
                    borderRadius: theme('borderRadius.lg'),
                    overflow: 'hidden',
                    transition: 'transform 0.2s, box-shadow 0.2s',
                },
                '.cima-card-hover': {
                    '&:hover': {
                        transform: 'translateY(-2px)',
                        boxShadow: theme('boxShadow.lg'),
                    },
                },
                
                // === SECCIONES ===
                '.cima-section': {
                    width: '100%',
                    padding: '4rem 1rem',
                },
                '.cima-section-alt': {
                    backgroundColor: theme('colors.section.DEFAULT'),
                },
                
                // === CONTENEDOR ===
                '.cima-container': {
                    width: '100%',
                    maxWidth: theme('maxWidth.7xl'),
                    marginLeft: 'auto',
                    marginRight: 'auto',
                    paddingLeft: '1rem',
                    paddingRight: '1rem',
                },
                
                // === GRID RESPONSIVO ===
                '.cima-grid': {
                    display: 'grid',
                    gap: '1.5rem',
                    gridTemplateColumns: 'repeat(1, minmax(0, 1fr))',
                    '@media (min-width: 768px)': {
                        gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
                    },
                    '@media (min-width: 1024px)': {
                        gridTemplateColumns: 'repeat(3, minmax(0, 1fr))',
                    },
                },
            });
        },
    ],
};
