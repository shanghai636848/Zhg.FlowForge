// uno.config.mjs
import {
    defineConfig,
    presetUno,
    presetAttributify,
    presetIcons,
    presetTypography,
    transformerDirectives,
    transformerVariantGroup,
} from 'unocss'

export default defineConfig({
    content: {
        filesystem: [
            './**/*.razor',
            './**/*.razor.css',
            './wwwroot/**/*.html',
            './wwwroot/**/*.js',
            './Pages/**/*.razor',
            './Components/**/*.razor',
        ],
    },

    presets: [
        presetUno({
            dark: 'class',
        }),
        presetAttributify(),
        presetIcons({
            scale: 1.2,
            warn: true,
        }),
        presetTypography(),
    ],

    transformers: [
        transformerDirectives(),
        transformerVariantGroup(),
    ],

    theme: {
        container: {
            center: true,
            padding: '1rem',
        },
        colors: {
            primary: {
                50: '#eff6ff',
                100: '#dbeafe',
                200: '#bfdbfe',
                300: '#93c5fd',
                400: '#60a5fa',
                500: '#3b82f6',
                600: '#2563eb',
                700: '#1d4ed8',
                800: '#1e40af',
                900: '#1e3a8a',
            },
            secondary: {
                50: '#f8fafc',
                100: '#f1f5f9',
                200: '#e2e8f0',
                300: '#cbd5e1',
                400: '#94a3b8',
                500: '#64748b',
                600: '#475569',
                700: '#334155',
                800: '#1e293b',
                900: '#0f172a',
            },
            success: {
                50: '#f0fdf4',
                100: '#dcfce7',
                200: '#bbf7d0',
                300: '#86efac',
                400: '#4ade80',
                500: '#22c55e',
                600: '#16a34a',
                700: '#15803d',
                800: '#166534',
                900: '#14532d',
            },
            warning: {
                50: '#fffbeb',
                100: '#fef3c7',
                200: '#fde68a',
                300: '#fcd34d',
                400: '#fbbf24',
                500: '#f59e0b',
                600: '#d97706',
                700: '#b45309',
                800: '#92400e',
                900: '#78350f',
            },
            error: {
                50: '#fef2f2',
                100: '#fee2e2',
                200: '#fecaca',
                300: '#fca5a5',
                400: '#f87171',
                500: '#ef4444',
                600: '#dc2626',
                700: '#b91c1c',
                800: '#991b1b',
                900: '#7f1d1d',
            },
            info: {
                50: '#f0f9ff',
                100: '#e0f2fe',
                200: '#bae6fd',
                300: '#7dd3fc',
                400: '#38bdf8',
                500: '#0ea5e9',
                600: '#0284c7',
                700: '#0369a1',
                800: '#075985',
                900: '#0c4a6e',
            }
        },
        animation: {
            'fade-in': 'fadeIn 0.5s ease-in-out',
            'slide-up': 'slideUp 0.3s ease-out',
            'bounce-subtle': 'bounceSubtle 2s infinite',
            'pulse-subtle': 'pulseSubtle 2s infinite',
            'shake': 'shake 0.5s ease-in-out',
            'slide-in-right': 'slideInRight 0.3s ease-out',
            'zoom-in': 'zoomIn 0.2s ease-out',
        },
        keyframes: {
            fadeIn: {
                '0%': { opacity: '0' },
                '100%': { opacity: '1' },
            },
            slideUp: {
                '0%': { transform: 'translateY(10px)', opacity: '0' },
                '100%': { transform: 'translateY(0)', opacity: '1' },
            },
            bounceSubtle: {
                '0%, 100%': { transform: 'translateY(0)' },
                '50%': { transform: 'translateY(-5px)' },
            },
            pulseSubtle: {
                '0%, 100%': { opacity: '1' },
                '50%': { opacity: '0.8' },
            },
            shake: {
                '0%, 100%': { transform: 'translateX(0)' },
                '10%, 30%, 50%, 70%, 90%': { transform: 'translateX(-5px)' },
                '20%, 40%, 60%, 80%': { transform: 'translateX(5px)' },
            },
            slideInRight: {
                '0%': { transform: 'translateX(100%)', opacity: '0' },
                '100%': { transform: 'translateX(0)', opacity: '1' },
            },
            zoomIn: {
                '0%': { transform: 'scale(0.9)', opacity: '0' },
                '100%': { transform: 'scale(1)', opacity: '1' },
            },
        },
    },

    preflights: [
        {
            getCSS: () => `
html, body {
  overflow-x: hidden;
  width: 100%;
  margin: 0;
  padding: 0;
  scroll-behavior: smooth;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  line-height: 1.6;
  color: #333;
  background-color: #f9fafb;
}

*,
*::before,
*::after {
  box-sizing: border-box;
}

/* ========== 动态根字体 ========== */
html { font-size: 16px; }

@media (max-width: 1440px) { html { font-size: 15.5px; } }
@media (max-width: 1280px) { html { font-size: 15px; } }
@media (max-width: 1024px) { html { font-size: 14.5px; } }
@media (max-width: 900px)  { html { font-size: 14px; } }
@media (max-width: 768px)  { html { font-size: 13.5px; } }
@media (max-width: 640px)  { html { font-size: 13px; } }
@media (max-width: 480px)  { html { font-size: 12.5px; } }
@media (max-width: 375px)  { html { font-size: 12px; } }
@media (max-width: 320px)  { html { font-size: 11.5px; } }

@media (min-width: 1920px) { html { font-size: 17px; } }
@media (min-width: 2560px) { html { font-size: 18px; } }
@media (min-width: 3840px) { html { font-size: 20px; } }

/* 平滑过渡 */
* {
  transition: all 0.2s ease-in-out;
}

/* 自定义滚动条 */
::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

/* 选择文本样式 */
::selection {
  background-color: #3b82f6;
  color: white;
}

/* 焦点样式 */
:focus {
  outline: 2px solid #3b82f6;
  outline-offset: 2px;
}
      `,
        },
    ],

    shortcuts: {
        // ========== 基础工具类对比 ==========
        // 原子类: 'flex items-center justify-center'
        // 快捷类: 'flex-center'

        // 原子类: 'flex items-center justify-between'
        // 快捷类: 'flex-between'

        // 原子类: 'bg-white rounded-lg shadow-md p-6'
        // 快捷类: 'card'

        // ========== 布局系统 ==========
        'layout-base': 'w-full min-h-screen',
        'layout-full': 'w-screen h-screen',
        'layout-top-bottom': 'flex flex-col min-h-screen',
        'layout-top-main': 'flex-1 overflow-auto',
        'layout-header': 'flex-none bg-white shadow-sm',
        'layout-footer': 'flex-none bg-gray-100 border-t',
        'layout-header-main-footer': 'flex flex-col min-h-screen',
        'layout-header-fixed': 'fixed top-0 left-0 right-0 z-50',
        'layout-left-right': 'flex min-h-screen',
        'layout-sidebar': 'w-64 flex-none bg-gray-50 border-r',
        'layout-content': 'flex-1 overflow-auto',
        'layout-left-center-right': 'flex min-h-screen',
        'layout-left-sidebar': 'w-48 flex-none',
        'layout-right-sidebar': 'w-64 flex-none',
        'layout-holy-grail': 'grid grid-cols-1 min-h-screen md:grid-cols-[1fr_auto_1fr]',
        'layout-holy-grail-header': 'col-span-1 md:col-span-3',
        'layout-holy-grail-left': 'col-start-1',
        'layout-holy-grail-main': 'col-start-1 md:col-start-2',
        'layout-holy-grail-right': 'col-start-1 md:col-start-3',
        'layout-holy-grail-footer': 'col-span-1 md:col-span-3',
        'layout-center-horizontal': 'flex justify-center',
        'layout-center-vertical': 'flex items-center',
        'layout-center-both': 'flex items-center justify-center',
        'layout-center-text': 'text-center',
        'layout-grid-2': 'grid grid-cols-2 gap-4',
        'layout-grid-3': 'grid grid-cols-3 gap-4',
        'layout-grid-4': 'grid grid-cols-4 gap-4',
        'layout-grid-responsive': 'grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4',
        'layout-sidebar-left': 'flex min-h-screen',
        'layout-sidebar-right': 'flex min-h-screen flex-row-reverse',
        'layout-sidebar-collapsible': 'transition-all duration-300',
        'layout-card-grid': 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6',
        'layout-card-list': 'flex flex-col gap-4',
        'layout-form': 'max-w-md mx-auto space-y-4',
        'layout-form-wide': 'max-w-2xl mx-auto space-y-6',
        'layout-form-grid': 'grid grid-cols-1 md:grid-cols-2 gap-4',
        'layout-nav-horizontal': 'flex items-center space-x-4',
        'layout-nav-vertical': 'flex flex-col space-y-2',
        'layout-nav-stacked': 'flex flex-col space-y-1',
        'layout-dashboard': 'grid grid-cols-1 lg:grid-cols-4 gap-6',
        'layout-dashboard-main': 'lg:col-span-3',
        'layout-dashboard-sidebar': 'lg:col-span-1',
        'layout-mobile-only': 'block md:hidden',
        'layout-desktop-only': 'hidden md:block',
        'layout-tablet-only': 'hidden sm:block lg:hidden',
        'layout-space-y': 'space-y-4',
        'layout-space-x': 'space-x-4',
        'layout-space-y-lg': 'space-y-6',
        'layout-space-x-lg': 'space-x-6',
        'layout-split-2': 'grid grid-cols-1 md:grid-cols-2 gap-8',
        'layout-split-3': 'grid grid-cols-1 md:grid-cols-3 gap-8',
        'layout-split-4': 'grid grid-cols-1 md:grid-cols-4 gap-8',
        'layout-flow-row': 'flex flex-wrap gap-4',
        'layout-flow-col': 'flex flex-col flex-wrap gap-4',
        'layout-fixed-top': 'fixed top-0 left-0 right-0 z-50',
        'layout-fixed-bottom': 'fixed bottom-0 left-0 right-0 z-50',
        'layout-fixed-left': 'fixed left-0 top-0 bottom-0 z-40',
        'layout-fixed-right': 'fixed right-0 top-0 bottom-0 z-40',
        'layout-absolute-center': 'absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2',
        'layout-absolute-full': 'absolute inset-0',
        'layout-sticky-top': 'sticky top-0 z-30',
        'layout-sticky-bottom': 'sticky bottom-0 z-30',

        // ========== Flex 布局系统 ==========
        'flex-center': 'flex items-center justify-center',
        'flex-between': 'flex items-center justify-between',
        'flex-around': 'flex items-center justify-around',
        'flex-evenly': 'flex items-center justify-evenly',
        'flex-start': 'flex items-center justify-start',
        'flex-end': 'flex items-center justify-end',
        'flex-col': 'flex flex-col',
        'flex-col-center': 'flex flex-col items-center justify-center',
        'flex-col-between': 'flex flex-col items-center justify-between',
        'flex-col-start': 'flex flex-col items-start justify-start',
        'flex-col-end': 'flex flex-col items-end justify-end',
        'inline-flex-center': 'inline-flex items-center justify-center',

        // ========== 增强卡片系统 ==========
        'card': 'bg-white rounded-xl shadow-md hover:shadow-lg transition-all duration-300 p-6 border border-gray-100',
        'card-sm': 'bg-white rounded-lg shadow-sm hover:shadow-md transition-all duration-300 p-4 border border-gray-100',
        'card-lg': 'bg-white rounded-2xl shadow-lg hover:shadow-xl transition-all duration-300 p-8 border border-gray-100',
        'card-hover': 'transform hover:-translate-y-1 cursor-pointer',
        'card-interactive': 'card card-hover',
        'card-bordered': 'card border-2 border-gray-200',
        'card-primary': 'card border-primary-200 bg-primary-50',
        'card-success': 'card border-success-200 bg-success-50',
        'card-warning': 'card border-warning-200 bg-warning-50',
        'card-error': 'card border-error-200 bg-error-50',
        'card-info': 'card border-info-200 bg-info-50',

        // ========== 增强按钮系统 ==========
        'btn': 'inline-flex items-center justify-center px-4 py-2 rounded-lg font-medium transition-all duration-300 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transform hover:scale-105 active:scale-95',
        'btn-primary': 'btn bg-primary-500 text-white hover:bg-primary-600 focus:ring-primary-500 shadow-sm hover:shadow-md',
        'btn-secondary': 'btn bg-secondary-500 text-white hover:bg-secondary-600 focus:ring-secondary-500 shadow-sm hover:shadow-md',
        'btn-success': 'btn bg-success-500 text-white hover:bg-success-600 focus:ring-success-500 shadow-sm hover:shadow-md',
        'btn-warning': 'btn bg-warning-500 text-white hover:bg-warning-600 focus:ring-warning-500 shadow-sm hover:shadow-md',
        'btn-error': 'btn bg-error-500 text-white hover:bg-error-600 focus:ring-error-500 shadow-sm hover:shadow-md',
        'btn-info': 'btn bg-info-500 text-white hover:bg-info-600 focus:ring-info-500 shadow-sm hover:shadow-md',
        'btn-outline': 'btn bg-transparent border border-gray-300 text-gray-700 hover:bg-gray-50 focus:ring-gray-500',
        'btn-outline-primary': 'btn bg-transparent border border-primary-500 text-primary-600 hover:bg-primary-50 focus:ring-primary-500',
        'btn-outline-success': 'btn bg-transparent border border-success-500 text-success-600 hover:bg-success-50 focus:ring-success-500',
        'btn-ghost': 'btn bg-transparent text-gray-600 hover:bg-gray-100 focus:ring-gray-500',
        'btn-link': 'btn bg-transparent text-primary-600 hover:text-primary-700 hover:underline focus:ring-primary-500',
        'btn-sm': 'px-3 py-1 text-sm rounded-md',
        'btn-lg': 'px-6 py-3 text-lg rounded-xl',
        'btn-xl': 'px-8 py-4 text-xl rounded-2xl',
        'btn-icon': 'p-2 rounded-lg',
        'btn-icon-sm': 'p-1 rounded-md',
        'btn-icon-lg': 'p-3 rounded-xl',
        'btn-group': 'inline-flex rounded-lg border border-gray-200 overflow-hidden',
        'btn-group-item': 'btn-ghost rounded-none border-r border-gray-200 last:border-r-0',

        // ========== 增强表单系统 ==========
        'input': 'w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300 bg-white placeholder-gray-400',
        'input-sm': 'input px-3 py-1 text-sm',
        'input-lg': 'input px-6 py-3 text-lg',
        'input-error': 'input border-error-300 focus:ring-error-500 bg-error-50',
        'input-success': 'input border-success-300 focus:ring-success-500 bg-success-50',
        'input-warning': 'input border-warning-300 focus:ring-warning-500 bg-warning-50',
        'input-disabled': 'input bg-gray-100 cursor-not-allowed opacity-70',
        'textarea': 'input resize-vertical min-h-[100px]',
        'textarea-sm': 'textarea px-3 py-1 text-sm',
        'textarea-lg': 'textarea px-6 py-3 text-lg',
        'select': 'input pr-10 appearance-none bg-no-repeat bg-right',
        'select-sm': 'select px-3 py-1 text-sm',
        'select-lg': 'select px-6 py-3 text-lg',
        'select-multiple': 'select min-h-[100px]',
        'checkbox': 'w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500 transition-colors duration-200',
        'checkbox-sm': 'checkbox w-3 h-3',
        'checkbox-lg': 'checkbox w-5 h-5',
        'radio': 'w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500 transition-colors duration-200',
        'radio-sm': 'radio w-3 h-3',
        'radio-lg': 'radio w-5 h-5',
        'toggle': 'relative inline-flex h-6 w-11 items-center rounded-full bg-gray-200 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
        'toggle-checked': 'toggle bg-primary-600',
        'toggle-handle': 'inline-block h-4 w-4 transform rounded-full bg-white transition-transform duration-200',
        'toggle-handle-checked': 'toggle-handle translate-x-6',
        'form-group': 'space-y-2',
        'form-label': 'block text-sm font-medium text-gray-700 mb-2',
        'form-help': 'text-sm text-gray-500 mt-1',
        'form-error': 'text-sm text-error-600 mt-1',
        'form-success': 'text-sm text-success-600 mt-1',
        'input-group': 'flex rounded-lg border border-gray-300 overflow-hidden focus-within:ring-2 focus-within:ring-primary-500 focus-within:border-transparent',
        'input-group-addon': 'px-3 py-2 bg-gray-100 border-r border-gray-300 text-gray-500',
        'input-group-input': 'flex-1 border-0 focus:ring-0 bg-transparent',
        'search-input': 'input pl-10',
        'date-input': 'input',
        'time-input': 'input',
        'color-input': 'input h-10 p-1',
        'range-input': 'w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer',
        'file-input': 'input file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100',

        // ========== 增强下拉选择系统 ==========
        'dropdown': 'relative inline-block',
        'dropdown-toggle': 'btn-secondary',
        'dropdown-menu': 'absolute z-50 mt-2 w-56 origin-top-right rounded-md bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none animate-slide-in-right',
        'dropdown-item': 'block w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-100 hover:text-gray-900 transition-colors duration-200',
        'dropdown-divider': 'my-1 border-t border-gray-200',
        'dropdown-header': 'px-4 py-2 text-xs font-semibold text-gray-500 uppercase tracking-wider',

        // ========== 增强自动完成系统 ==========
        'autocomplete': 'relative',
        'autocomplete-input': 'input pr-10',
        'autocomplete-menu': 'absolute z-50 mt-1 w-full max-h-60 overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm',
        'autocomplete-option': 'relative cursor-default select-none py-2 pl-3 pr-9 text-gray-900 hover:bg-primary-600 hover:text-white transition-colors duration-200',
        'autocomplete-option-active': 'autocomplete-option bg-primary-600 text-white',

        // ========== 增强标签输入系统 ==========
        'tags-input': 'input flex flex-wrap gap-2 items-center',
        'tag': 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800',
        'tag-close': 'ml-1.5 w-3 h-3 rounded-full hover:bg-black hover:bg-opacity-10 flex-center transition-colors duration-200 cursor-pointer',

        // ========== 增强列表系统 ==========
        'list': 'divide-y divide-gray-200',
        'list-item': 'py-4 px-4 hover:bg-gray-50 transition-colors duration-200',
        'list-item-interactive': 'list-item cursor-pointer hover:bg-primary-50',
        'list-bordered': 'border border-gray-200 rounded-lg overflow-hidden',
        'list-striped': 'list-item:nth-child(even) bg-gray-50',

        // ========== 增强表格系统 ==========
        'table': 'w-full bg-white rounded-lg shadow-sm overflow-hidden',
        'table-header': 'bg-gray-50 border-b border-gray-200',
        'table-header-cell': 'px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider',
        'table-body': 'divide-y divide-gray-200',
        'table-cell': 'px-6 py-4 whitespace-nowrap text-sm text-gray-900',
        'table-row-hover': 'hover:bg-gray-50 transition-colors duration-200',
        'table-compact': 'table-cell px-3 py-2 text-sm',
        'table-striped': 'table-body table-row:nth-child(even) bg-gray-50',

        // ========== 增强徽章系统 ==========
        'badge': 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
        'badge-primary': 'badge bg-primary-100 text-primary-800',
        'badge-secondary': 'badge bg-secondary-100 text-secondary-800',
        'badge-success': 'badge bg-success-100 text-success-800',
        'badge-warning': 'badge bg-warning-100 text-warning-800',
        'badge-error': 'badge bg-error-100 text-error-800',
        'badge-info': 'badge bg-info-100 text-info-800',
        'badge-gray': 'badge bg-gray-100 text-gray-800',
        'badge-sm': 'badge px-2 py-0.5 text-xs',
        'badge-lg': 'badge px-3 py-1 text-sm',

        // ========== 增强警告系统 ==========
        'alert': 'p-4 rounded-lg border',
        'alert-info': 'alert bg-blue-50 border-blue-200 text-blue-800',
        'alert-success': 'alert bg-success-50 border-success-200 text-success-800',
        'alert-warning': 'alert bg-warning-50 border-warning-200 text-warning-800',
        'alert-error': 'alert bg-error-50 border-error-200 text-error-800',
        'alert-with-icon': 'alert flex items-start space-x-3',
        'alert-icon': 'w-5 h-5 mt-0.5 flex-shrink-0',
        'alert-content': 'flex-1',
        'alert-close': 'btn-ghost btn-sm p-1',

        // ========== 增强加载系统 ==========
        'loading': 'flex items-center justify-center',
        'loading-spinner': 'animate-spin rounded-full h-6 w-6 border-b-2 border-primary-500',
        'loading-spinner-sm': 'loading-spinner h-4 w-4',
        'loading-spinner-lg': 'loading-spinner h-8 w-8',
        'loading-dots': 'flex space-x-1',
        'loading-dot': 'w-2 h-2 bg-gray-400 rounded-full animate-bounce-subtle',
        'loading-bar': 'w-full bg-gray-200 rounded-full h-1.5 overflow-hidden',
        'loading-bar-progress': 'h-full bg-primary-500 rounded-full animate-pulse-subtle',

        // ========== 增强头像系统 ==========
        'avatar': 'inline-block rounded-full overflow-hidden bg-gray-200 flex-center',
        'avatar-sm': 'w-8 h-8 text-sm',
        'avatar-md': 'w-12 h-12 text-base',
        'avatar-lg': 'w-16 h-16 text-lg',
        'avatar-xl': 'w-20 h-20 text-xl',
        'avatar-group': 'flex -space-x-2',
        'avatar-bordered': 'avatar border-2 border-white',

        // ========== 增强进度条系统 ==========
        'progress': 'w-full bg-gray-200 rounded-full h-2 overflow-hidden',
        'progress-bar': 'h-full rounded-full transition-all duration-500',
        'progress-primary': 'progress-bar bg-primary-500',
        'progress-success': 'progress-bar bg-success-500',
        'progress-warning': 'progress-bar bg-warning-500',
        'progress-error': 'progress-bar bg-error-500',
        'progress-info': 'progress-bar bg-info-500',
        'progress-sm': 'progress h-1',
        'progress-lg': 'progress h-3',

        // ========== 增强工具提示系统 ==========
        'tooltip': 'absolute invisible group-hover:visible bg-gray-900 text-white text-sm px-2 py-1 rounded z-50 transform -translate-x-1/2 left-1/2 bottom-full mb-2 animate-fade-in',
        'tooltip-container': 'relative group',
        'tooltip-top': 'tooltip bottom-full mb-2',
        'tooltip-bottom': 'tooltip top-full mt-2',
        'tooltip-left': 'tooltip right-full mr-2 top-1/2 transform -translate-y-1/2',
        'tooltip-right': 'tooltip left-full ml-2 top-1/2 transform -translate-y-1/2',

        // ========== 增强模态框系统 ==========
        'modal-overlay': 'fixed inset-0 bg-black bg-opacity-50 flex-center z-50 animate-fade-in',
        'modal-container': 'bg-white rounded-2xl shadow-xl max-w-md w-full mx-4 max-h-[90vh] overflow-auto animate-zoom-in',
        'modal-header': 'px-6 py-4 border-b border-gray-200 flex-between',
        'modal-body': 'px-6 py-4',
        'modal-footer': 'px-6 py-4 border-t border-gray-200 flex-between',
        'modal-sm': 'modal-container max-w-sm',
        'modal-lg': 'modal-container max-w-2xl',
        'modal-xl': 'modal-container max-w-4xl',

        // ========== 增强导航系统 ==========
        'nav-item': 'px-3 py-2 rounded-lg text-gray-600 hover:text-gray-900 hover:bg-gray-100 transition-colors duration-200',
        'nav-item-active': 'nav-item bg-primary-100 text-primary-700',
        'nav-tab': 'border-b-2 border-transparent px-4 py-2 text-gray-500 hover:text-gray-700 hover:border-gray-300 transition-colors duration-200',
        'nav-tab-active': 'nav-tab border-primary-500 text-primary-600',
        'nav-pill': 'nav-item rounded-full',
        'nav-pill-active': 'nav-pill bg-primary-500 text-white hover:bg-primary-600 hover:text-white',

        // ========== 增强面包屑系统 ==========
        'breadcrumb': 'flex items-center space-x-2 text-sm text-gray-500',
        'breadcrumb-item': 'hover:text-gray-700 transition-colors duration-200',
        'breadcrumb-separator': 'text-gray-400',
        'breadcrumb-current': 'text-gray-700 font-medium',

        // ========== 增强分页系统 ==========
        'pagination': 'flex items-center justify-center space-x-1',
        'pagination-item': 'px-3 py-1 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors duration-200',
        'pagination-active': 'pagination-item bg-primary-500 text-white hover:bg-primary-600',
        'pagination-disabled': 'pagination-item opacity-50 cursor-not-allowed',
        'pagination-sm': 'pagination-item px-2 py-1 text-sm',
        'pagination-lg': 'pagination-item px-4 py-2 text-lg',

        // ========== 增强标签系统 ==========
        'tag': 'inline-flex items-center px-3 py-1 rounded-full text-sm font-medium',
        'tag-primary': 'tag bg-primary-100 text-primary-800',
        'tag-close': 'ml-1.5 w-4 h-4 rounded-full hover:bg-black hover:bg-opacity-10 flex-center transition-colors duration-200',

        // ========== 增强步骤系统 ==========
        'steps': 'flex items-center space-x-4',
        'step': 'flex items-center',
        'step-number': 'w-8 h-8 rounded-full border-2 flex-center text-sm font-medium transition-colors duration-300',
        'step-active': 'step-number border-primary-500 bg-primary-500 text-white',
        'step-completed': 'step-number border-success-500 bg-success-500 text-white',
        'step-pending': 'step-number border-gray-300 text-gray-500',
        'step-error': 'step-number border-error-500 bg-error-500 text-white',
        'step-connector': 'flex-1 h-0.5 bg-gray-300',

        // ========== 其他实用布局 ==========
        'layout-masonry': 'columns-1 sm:columns-2 lg:columns-3 gap-4',
        'layout-stack': 'flex flex-col divide-y divide-gray-200',
        'layout-cover': 'relative min-h-screen bg-cover bg-center',
        'layout-hero': 'min-h-80 flex-center bg-gradient-to-r from-blue-500 to-purple-600 text-white',
        'layout-feature': 'grid grid-cols-1 md:grid-cols-3 gap-8 items-start',
        'layout-testimonial': 'grid grid-cols-1 lg:grid-cols-2 gap-8 items-center',
        'layout-pricing': 'grid grid-cols-1 md:grid-cols-3 gap-8',
        'layout-team': 'grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-6',
        'layout-gallery': 'grid grid-cols-2 md:grid-cols-4 gap-2',
        'layout-timeline': 'relative border-l border-gray-200 ml-4 pl-8 space-y-8',
        'layout-faq': 'grid grid-cols-1 lg:grid-cols-2 gap-8',
        'layout-contact': 'grid grid-cols-1 lg:grid-cols-2 gap-12 items-start',
        'layout-blog': 'grid grid-cols-1 lg:grid-cols-4 gap-8',
        'layout-ecommerce': 'grid grid-cols-1 lg:grid-cols-12 gap-8',
        'layout-product': 'grid grid-cols-1 lg:grid-cols-2 gap-12 items-start',
        'layout-checkout': 'grid grid-cols-1 lg:grid-cols-3 gap-8',
        'layout-profile': 'grid grid-cols-1 lg:grid-cols-4 gap-8',
        'layout-settings': 'grid grid-cols-1 lg:grid-cols-12 gap-8',
        'layout-onboarding': 'min-h-screen flex-center bg-gray-50',
        'layout-error': 'min-h-screen flex-center bg-white text-center',
        'layout-success': 'min-h-96 flex-center bg-green-50 text-green-800',
        'layout-loading': 'flex-center min-h-40',
        'layout-empty': 'flex-col-center min-h-60 text-gray-500',
        'layout-modal': 'modal-overlay',
        'layout-drawer': 'fixed inset-y-0 right-0 w-96 bg-white shadow-xl z-40',
        'layout-tabs': 'border-b border-gray-200',
        'layout-accordion': 'divide-y divide-gray-200',
        'layout-breadcrumb': 'breadcrumb',
        'layout-pagination': 'pagination',
        'layout-progress': 'progress',
        'layout-stats': 'grid grid-cols-2 md:grid-cols-4 gap-4',
        'layout-chart': 'card',
        'layout-table': 'table',
        'layout-list': 'list list-bordered',
        'layout-filter': 'card-sm space-y-4',
        'layout-search': 'relative max-w-md',
        'layout-notification': 'fixed top-4 right-4 max-w-sm z-50',
        'layout-tooltip': 'tooltip',
        'layout-badge': 'badge',
        'layout-avatar-group': 'avatar-group',
        'layout-step': 'steps',
    },
})