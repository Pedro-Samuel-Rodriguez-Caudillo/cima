/* @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Pages/**/*.{razor,html}',
        './Components/**/*.{razor,html}',
        './Shared/**/*.{razor,html}',
        './*.razor',
        './wwwroot/index.html'
    ],
    theme: {
        extend: {
            // Design tokens principales
            colors: {
                // Tokens base (usa estos para nuevos diseños)
                primary: '#2563eb',
                'primary-dark': '#1d4ed8',
                'primary-soft': '#dbeafe',

                secondary: '#64748b',
                'secondary-dark': '#475569',

                success: '#10b981',
                danger: '#ef4444',
                warning: '#f59e0b',
                info: '#06b6d4',

                // Alias para mantener clases existentes (text-cima-primary, etc.)
                'cima-primary': '#2563eb',
                'cima-secondary': '#64748b',
                'cima-success': '#10b981',
                'cima-danger': '#ef4444',
                'cima-warning': '#f59e0b',
                'cima-info': '#06b6d4',
            },

            borderRadius: {
                card: '0.75rem',
                button: '0.5rem',
                input: '0.5rem',
                pill: '9999px',
            },

            boxShadow: {
                card: '0 10px 15px -3px rgba(0,0,0,.1), 0 4px 6px -4px rgba(0,0,0,.1)',
                button: '0 2px 4px -1px rgba(0,0,0,.1)',
            },

            fontFamily: {
                sans: ['Inter', 'system-ui', 'sans-serif'],
            },
        },
    },
    plugins: [
        require('@tailwindcss/forms'),
        require('@tailwindcss/typography'),
    ],
};
