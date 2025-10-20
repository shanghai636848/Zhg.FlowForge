// uno.config.ts
import {
    defineConfig,
    presetUno,
    presetAttributify,
    presetIcons,
    presetTypography,
    presetWebFonts,
    transformerDirectives,
    transformerVariantGroup,
} from 'unocss'

export default defineConfig({
    // ========== 内容扫描 ==========
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

    // ========== 预设 ==========
    presets: [
        presetUno({
            dark: 'class', // 启用 class 模式暗色主题
            // 注意：不配置 container，避免冲突
        }),
        presetAttributify(), // 启用属性模式（如 btn="primary"）
        presetIcons({
            scale: 1.2,
            warn: true,
        }),
        presetTypography(), // 为 prose 类提供排版样式
        presetWebFonts({
            fonts: {
                sans: 'Inter:400,500,600,700',
                // 可选：自动从 Google Fonts 加载
            },
        }),
    ],

    // ========== 转换器 ==========
    transformers: [
        transformerDirectives(), // 支持 @apply
        transformerVariantGroup(), // 支持 class="hover:focus:(bg-red-500 text-white)"
    ],

    // ========== 主题扩展 ==========
    theme: {
        container: {
        center: true, // 自动 margin: 0 auto
            padding: '1rem', // 默认 padding
        },
        colors: {
        },
        breakpoints: {
            xs: '320px',
            sm: '375px',
            md: '480px',
            lg: '640px',
            xl: '768px',
            '2xl': '900px',
            '3xl': '1024px',
            '4xl': '1280px',
            '5xl': '1440px',
            '6xl': '1920px',
            '7xl': '2560px',
            '8xl': '3840px',
        },
        fontSize: {
        },
    },

    // ========== 全局样式（替代所有全局 CSS）==========
    preflights: [
        {
            getCSS: () => `

/* ========== 全局 reset ========== */
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

/* ========== 动态根字体（响应式字体大小） ========== */
/* 基准字体大小：16px → 适用于 1441px ~ 1919px（即 5xl < width < 6xl） */
html { font-size: 16px; }

/* 小于等于各断点时，逐步缩小字体 */
@media (max-width: 1440px) { html { font-size: 15.5px; } }
@media (max-width: 1280px) { html { font-size: 15px; } }
@media (max-width: 1024px) { html { font-size: 14.5px; } }
@media (max-width: 900px)  { html { font-size: 14px; } }
@media (max-width: 768px)  { html { font-size: 13.5px; } }
@media (max-width: 640px)  { html { font-size: 13px; } }
@media (max-width: 480px)  { html { font-size: 12.5px; } }
@media (max-width: 375px)  { html { font-size: 12px; } }
@media (max-width: 320px)  { html { font-size: 11.5px; } }

/* 大于等于大屏断点时，逐步放大字体 */
@media (min-width: 1920px) { html { font-size: 17px; } }
@media (min-width: 2560px) { html { font-size: 18px; } }
@media (min-width: 3840px) { html { font-size: 20px; } }
      `,
        },
    ],

    // ========== 快捷类（Shortcuts）==========
    shortcuts: {
        // ========== Flex 布局 ==========
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

        // 快速垂直居中（常用于图标+文字）
        'inline-flex-center': 'inline-flex items-center justify-center',

        // ========== Grid 布局 ==========
        'grid-cols-2': 'grid grid-cols-2',
        'grid-cols-3': 'grid grid-cols-3',
        'grid-cols-4': 'grid grid-cols-4',
        'grid-cols-5': 'grid grid-cols-5',
        'grid-cols-6': 'grid grid-cols-6',

        // 响应式常用网格
        'grid-cols-responsive': 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4',

        // 简易 Masonry / 自适应列（需配合 gap）
        'grid-auto-cols': 'grid grid-auto-flow: dense grid-template-columns: repeat(auto-fill, minmax(250px, 1fr))',

        // 居中网格
        'grid-center': 'grid place-items-center',
        'grid-col-center': 'grid place-items-center place-content-center',

        // ========== 通用布局 ==========
        // 全屏居中（登录页、404 等）
        'layout-center': 'w-screen h-screen flex-center',

        // 内容区域（带 gap 的 flex 或 grid 容器）
        'layout-row': 'flex flex-wrap gap-4',
        'layout-col': 'flex flex-col gap-4',
        'layout-grid': 'grid gap-4',

        // 卡片容器（带 padding、圆角、阴影）
        'card': 'bg-white rounded-lg shadow p-4',
        'card-sm': 'bg-white rounded shadow-sm p-3',
        'card-lg': 'bg-white rounded-xl shadow-lg p-6',

        // 分割线
        'divider': 'border-t border-gray-200 my-4',

        // 隐藏但可访问（无障碍）
        'sr-only': 'absolute w-1 h-1 overflow-hidden opacity-0',

        // 清除浮动（现代布局较少用，但兼容旧需求）
        'clearfix': 'after:(content-[""] block clear-both)',
    },
})