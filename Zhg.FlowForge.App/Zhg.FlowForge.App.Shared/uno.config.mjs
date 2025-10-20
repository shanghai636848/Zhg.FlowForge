// uno.config.mjs
import { defineConfig, presetUno } from 'unocss'

export default defineConfig({
    content: {
        filesystem: [
            './**/*.razor',
            './wwwroot/**/*.html',
            // 如果有 JS/TS 也加上
            './wwwroot/**/*.js',
        ],
    },
    presets: [
        presetUno({
            dark: 'class', // 启用 class 模式暗色主题（如 class="dark"）
        }),
    ],
    // 扩展主题：颜色、断点、字体大小
    extendTheme: (theme) => ({
        ...theme,
        colors: {
            ...theme.colors,
            // 自定义语义化颜色（与 global.css 变量对齐）
            primary: '#3b82f6',
            secondary: '#6b7280',
            success: '#10b981',
            warning: '#f59e0b',
            error: '#ef4444',
            danger: '#ef4444', // UnoCSS 常用 danger
        },
        // 响应式断点（单位：px）
        breakpoints: {
            ...theme.breakpoints, // 保留默认 sm(640), md(768), lg(1024), xl(1280), 2xl(1536)
            xs: '320px',          // 超小屏
            '3xl': '1920px',      // 2K
            '4xl': '2560px',      // 2.5K
            '5xl': '3840px',      // 4K+
        },
        // 字体大小体系（可选，UnoCSS 默认已有，这里显式定义便于维护）
        fontSize: {
            xs: ['0.75rem', { lineHeight: '1rem' }],
            sm: ['0.875rem', { lineHeight: '1.25rem' }],
            base: ['1rem', { lineHeight: '1.5rem' }],
            lg: ['1.125rem', { lineHeight: '1.75rem' }],
            xl: ['1.25rem', { lineHeight: '1.75rem' }],
            '2xl': ['1.5rem', { lineHeight: '2rem' }],
            '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
            '4xl': ['2.25rem', { lineHeight: '2.5rem' }],
            '5xl': ['3rem', { lineHeight: '1' }],
        },
    })   
})