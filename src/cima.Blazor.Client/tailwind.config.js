/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.{razor,html}',
    './Components/**/*.{razor,html}',
    './Shared/**/*.{razor,html}',
    './*.razor',
    './wwwroot/index.html'
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        'cima-primary': '#162133',
        'cima-secondary': '#64748b',
        'cima-success': '#10b981',
        'cima-danger': '#ef4444',
        'cima-warning': '#f59e0b',
        'cima-info': '#06b6d4',
        'cima-accent': '#162133',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      boxShadow: {
        'cima': '0 4px 6px -1px rgba(22, 33, 51, 0.1), 0 2px 4px -1px rgba(22, 33, 51, 0.06)',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
}