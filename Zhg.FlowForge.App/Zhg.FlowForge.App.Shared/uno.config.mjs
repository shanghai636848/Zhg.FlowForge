// uno.config.mjs - UnoCSS 配置文件

// 导入 UnoCSS 相关模块和函数
import {
    // 主配置函数，用于定义 UnoCSS 配置
    defineConfig,
    // UnoCSS 默认预设，提供基础的原子类
    presetUno,
    // 属性化模式预设，允许使用属性形式的类名
    presetAttributify,
    // 图标预设，支持图标类名
    presetIcons,
    // 排版预设，提供排版相关的样式
    presetTypography,
    // 指令转换器，支持 CSS 指令
    transformerDirectives,
    // 变体组转换器，支持变体组合语法
    transformerVariantGroup,
} from 'unocss'

// 导出配置对象，使用 defineConfig 函数定义
export default defineConfig({
    // 内容配置，指定需要扫描的文件路径
    content: {
        // 文件系统路径配置
        filesystem: [
            // 扫描所有 .razor 文件
            './**/*.razor',
            // 扫描 wwwroot 目录下的所有 .html 文件
            './wwwroot/**/*.html',
        ],
    },

    // 预设配置数组
    presets: [
        // UnoCSS 默认预设配置
        presetUno({
            // 暗色主题模式设置为 class 模式
            dark: 'class',
        }),
        // 属性化模式预设（当前被注释掉）
        //presetAttributify(),
        // 图标预设配置
        presetIcons({
            // 图标缩放比例
            scale: 1.2,
            // 启用警告提示
            warn: true,
        }),
        // 排版预设（当前被注释掉）
        //presetTypography(),
    ],

    // 转换器配置数组（当前都被注释掉）
    transformers: [
        // 指令转换器（当前被注释掉）
        //transformerDirectives(),
        // 变体组转换器（当前被注释掉）
        //transformerVariantGroup(),
    ],

    // 主题配置对象
    theme: {
        // 容器配置
        container: {
            // 容器居中显示
            center: true,
            // 容器内边距
            padding: '1rem',
        },
        breakpoints: {
            'xs': '320px',    // iPhone 5/SE 等小屏设备
            // 标准移动设备断点
            'sm': '640px',    // 小屏幕设备
            'md': '768px',    // 平板设备
            'lg': '1024px',   // 小桌面设备
            'xl': '1280px',   // 中等桌面设备
            '2xl': '1536px',  // 大桌面设备
            '3xl': '1920px',  // 全高清设备
            // 超大屏幕和特殊设备
            '4xl': '2048px',  // QWXGA, iPad Pro 12.9"
            '5xl': '2560px',  // 2K/WQHD 显示器
            '6xl': '2880px',  // Retina 5K iMac
            '7xl': '3440px',  // 超宽 2K 显示器
            '8xl': '3840px',  // 4K UHD 显示器
            '9xl': '4096px',  // 4K DCI 显示器
        },
        // 颜色配置
        colors: {
            // 专业蓝色系 - 基于提供的色彩
            primary: {
                // 最浅的主色
                50: '#F5F9FF',
                // 浅主色
                100: '#E6F0FF',
                // 较浅主色
                200: '#CDE0FF',
                // 中等浅主色
                300: '#B0CBFF',
                // 中等主色
                400: '#94B3FF',
                // 主色
                500: '#7D9EFF',
                // 深主色
                600: '#6E8CFB',
                // 较深主色
                700: '#636CCB',
                // 深主色
                800: '#50589C',
                // 最深主色
                900: '#3C467B',
            },
            // 中性色调
            neutral: {
                // 最浅中性色
                50: '#fafafa',
                // 浅中性色
                100: '#f5f5f5',
                // 较浅中性色
                200: '#e5e5e5',
                // 中等浅中性色
                300: '#d4d4d4',
                // 中等中性色
                400: '#a3a3a3',
                // 中性色
                500: '#737373',
                // 深中性色
                600: '#525252',
                // 较深中性色
                700: '#404040',
                // 深中性色
                800: '#262626',
                // 最深中性色
                900: '#171717',
            },
            // 功能色调 - 成功色
            success: {
                // 最浅成功色
                50: '#f0fdf4',
                // 浅成功色
                200: '#bbf7d0',
                // 成功色
                500: '#22c55e',
                // 深成功色
                700: '#15803d',
            },
            // 功能色调 - 警告色
            warning: {
                // 最浅警告色
                50: '#fffbeb',
                // 浅警告色
                200: '#fde68a',
                // 警告色
                500: '#f59e0b',
                // 深警告色
                700: '#b45309',
            },
            // 功能色调 - 错误色
            error: {
                // 最浅错误色
                50: '#fef2f2',
                // 浅错误色
                200: '#fecaca',
                // 错误色
                500: '#ef4444',
                // 深错误色
                700: '#b91c1c',
            },
            // 功能色调 - 信息色
            info: {
                // 最浅信息色
                50: '#eff6ff',
                // 浅信息色
                200: '#bfdbfe',
                // 信息色
                500: '#3b82f6',
                // 深信息色
                700: '#1d4ed8',
            }
        },
        // 字体族配置
        fontFamily: {
            // 无衬线字体
            sans: ['-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
            // 等宽字体
            mono: ['SF Mono', 'Roboto Mono', 'Consolas', 'monospace'],
        },
        // 字体大小配置
        fontSize: {
            // 超小字体
            'xs': '0.75rem',
            // 小字体
            'sm': '0.875rem',
            // 基础字体
            'base': '1rem',
            // 大字体
            'lg': '1.125rem',
            // 加大字体
            'xl': '1.25rem',
            // 2倍加大字体
            '2xl': '1.5rem',
            // 3倍加大字体
            '3xl': '1.875rem',
            // 4倍加大字体
            '4xl': '2.25rem',
            // 5倍加大字体
            '5xl': '3rem',
        },
        // 字体粗细配置
        fontWeight: {
            // 正常粗细
            normal: '400',
            // 中等粗细
            medium: '500',
            // 半粗体
            semibold: '600',
            // 粗体
            bold: '700',
        },
        // 行高配置
        lineHeight: {
            // 紧凑行高
            tight: '1.25',
            // 正常行高
            normal: '1.6',
            // 宽松行高
            relaxed: '1.75',
        },
        // 间距配置
        spacing: {
            // 无间距
            '0': '0',
            // 1单位间距
            '1': '0.25rem',
            // 2单位间距
            '2': '0.5rem',
            // 3单位间距
            '3': '0.75rem',
            // 4单位间距
            '4': '1rem',
            // 5单位间距
            '5': '1.25rem',
            // 6单位间距
            '6': '1.5rem',
            // 8单位间距
            '8': '2rem',
            // 10单位间距
            '10': '2.5rem',
            // 12单位间距
            '12': '3rem',
            // 16单位间距
            '16': '4rem',
            // 20单位间距
            '20': '5rem',
            // 24单位间距
            '24': '6rem',
        },
        // 圆角配置
        borderRadius: {
            // 无圆角
            'none': '0',
            // 小圆角
            'sm': '0.125rem',
            // 基础圆角
            'base': '0.25rem',
            // 中等圆角
            'md': '0.375rem',
            // 大圆角
            'lg': '0.5rem',
            // 加大圆角
            'xl': '0.75rem',
            // 2倍加大圆角
            '2xl': '1rem',
            // 3倍加大圆角
            '3xl': '1.5rem',
            // 全圆角
            'full': '9999px',
        },
        // 阴影配置
        boxShadow: {
            // 小阴影
            'sm': '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
            // 基础阴影
            'base': '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
            // 中等阴影
            'md': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
            // 大阴影
            'lg': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
            // 加大阴影
            'xl': '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
            // 2倍加大阴影
            '2xl': '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
            // 内阴影
            'inner': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.06)',
        },
        // 动画配置
        animation: {
            // 淡入动画
            'fade-in': 'fadeIn 0.5s ease',
            // 上滑动画
            'slide-up': 'slideUp 0.5s ease',
            // 下滑动画
            'slide-down': 'slideDown 0.5s ease',
            // 右侧滑入动画
            'slide-in-right': 'slideInRight 0.3s ease',
            // 右侧滑出动画
            'slide-out-right': 'slideOutRight 0.3s ease',
            // 旋转动画
            'spin': 'spin 1s linear infinite',
            // 脉冲动画
            'pulse': 'pulse 2s infinite',
            // 弹跳动画
            'bounce': 'bounce 1s infinite',
        },
        // 关键帧配置
        keyframes: {
            // 淡入关键帧
            fadeIn: {
                // 初始状态：完全透明
                '0%': { opacity: '0' },
                // 结束状态：完全不透明
                '100%': { opacity: '1' },
            },
            // 上滑关键帧
            slideUp: {
                // 初始状态：向下偏移20px并透明
                '0%': { transform: 'translateY(20px)', opacity: '0' },
                // 结束状态：无偏移且不透明
                '100%': { transform: 'translateY(0)', opacity: '1' },
            },
            // 下滑关键帧
            slideDown: {
                // 初始状态：向上偏移20px并透明
                '0%': { transform: 'translateY(-20px)', opacity: '0' },
                // 结束状态：无偏移且不透明
                '100%': { transform: 'translateY(0)', opacity: '1' },
            },
            // 右侧滑入关键帧
            slideInRight: {
                // 初始状态：向右偏移100%
                '0%': { transform: 'translateX(100%)' },
                // 结束状态：无偏移
                '100%': { transform: 'translateX(0)' },
            },
            // 右侧滑出关键帧
            slideOutRight: {
                // 初始状态：无偏移
                '0%': { transform: 'translateX(0)' },
                // 结束状态：向右偏移100%
                '100%': { transform: 'translateX(100%)' },
            },
            // 旋转关键帧
            spin: {
                // 初始状态：无旋转
                '0%': { transform: 'rotate(0deg)' },
                // 结束状态：旋转360度
                '100%': { transform: 'rotate(360deg)' },
            },
            // 脉冲关键帧
            pulse: {
                // 开始和结束状态：完全不透明
                '0%, 100%': { opacity: '1' },
                // 中间状态：半透明
                '50%': { opacity: '0.5' },
            },
            // 弹跳关键帧
            bounce: {
                // 开始和结束状态：向上偏移25%，使用特定缓动函数
                '0%, 100%': {
                    transform: 'translateY(-25%)',
                    animationTimingFunction: 'cubic-bezier(0.8,0,1,1)'
                },
                // 中间状态：无偏移，使用另一种缓动函数
                '50%': {
                    transform: 'none',
                    animationTimingFunction: 'cubic-bezier(0,0,0.2,1)'
                },
            },
        },
        // 层级配置
        zIndex: {
            // 最底层
            '0': '0',
            // 低层
            '10': '10',
            // 中低层
            '20': '20',
            // 中层
            '30': '30',
            // 中高层
            '40': '40',
            // 高层
            '50': '50',
            // 很高层
            '100': '100',
            // 极高层
            '1000': '1000',
            // 最高层
            '1100': '1100',
        },
    },

    // 预定义样式配置
    preflights: [
        {
            // 获取CSS样式函数
            getCSS: () => `

/* 导入 Inter 字体 */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');

/* 重置所有元素的内外边距并设置盒模型 */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

/* HTML和body元素样式 */
html {
  font-size: 13.5px;
}

/* ========== 动态根字体（从小屏到大屏，基准 16px） ========== */

/* xs: ≥320px — iPhone 5/SE 等小屏手机 */
@media (min-width: 320px) {
  html { font-size: 14px; }
}

/* sm: ≥640px — 大屏手机 / 小平板（如 iPhone Plus、Galaxy S 系列） */
@media (min-width: 640px) {
  html { font-size: 14.5px; }
}

/* md: ≥768px — 平板竖屏（如 iPad、Android 平板） */
@media (min-width: 768px) {
  html { font-size: 15px; }
}

/* lg: ≥1024px — 小桌面 / 平板横屏（如 iPad Air 横屏、13" 笔记本） */
@media (min-width: 1024px) {
  html { font-size: 15.5px; }
}

/* xl: ≥1280px — 中等桌面（主流笔记本、台式机，默认基准区间） */
@media (min-width: 1280px) {
  html { font-size: 16px; } /* ← 基准字号 */
}

/* 2xl: ≥1536px — 大桌面（如 MacBook Pro 16"、Surface Studio） */
@media (min-width: 1536px) {
  html { font-size: 16.5px; }
}

/* 3xl: ≥1920px — 全高清桌面（FHD，1920×1080 主流显示器） */
@media (min-width: 1920px) {
  html { font-size: 17px; }
}

/* 4xl: ≥2048px — QWXGA / iPad Pro 12.9" 横屏、高端平板 */
@media (min-width: 2048px) {
  html { font-size: 17.5px; }
}

/* 5xl: ≥2560px — 2K / WQHD 显示器（如 27" 2K 屏） */
@media (min-width: 2560px) {
  html { font-size: 18px; }
}

/* 6xl: ≥2880px — Retina 5K iMac（5120×2880 的一半宽度） */
@media (min-width: 2880px) {
  html { font-size: 19px; }
}

/* 7xl: ≥3440px — 超宽 2K 带鱼屏（如 3440×1440） */
@media (min-width: 3440px) {
  html { font-size: 20px; }
}

/* 8xl: ≥3840px — 4K UHD 显示器（3840×2160） */
@media (min-width: 3840px) {
  html { font-size: 21px; }
}

/* 9xl: ≥4096px — 4K DCI 电影级分辨率（4096×2160） */
@media (min-width: 4096px) {
  html { font-size: 22px; }
}

html, body {
    width: 100%;
    height: 100%;
    line-height: 1.6;
    color: #171717;
    background-color: #fafafa;
    /* 隐藏水平方向的滚动条 */
    overflow-x: hidden;
    /* 页面滚动为平滑滚动 */
    scroll-behavior: smooth;
    /* 系统默认的无衬线字体栈 */
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}
      `,
        },
    ],

    // 快捷方式配置
    shortcuts: {
        // ========== Flex 容器快捷类 ==========
        // 弹性布局
        'flex': 'flex',
        // 行内弹性布局
        'inline-flex': 'inline-flex',
        // 水平方向弹性布局
        'flex-row': 'flex flex-row',
        // 垂直方向弹性布局
        'flex-col': 'flex flex-col',
        // 水平反向弹性布局
        'flex-row-reverse': 'flex flex-row-reverse',
        // 垂直反向弹性布局
        'flex-col-reverse': 'flex flex-col-reverse',
        // 弹性换行布局
        'flex-wrap': 'flex flex-wrap',
        // 弹性不换行布局
        'flex-nowrap': 'flex flex-nowrap',
        // 弹性反向换行布局
        'flex-wrap-reverse': 'flex flex-wrap-reverse',

        // ========== Flex 对齐快捷类 ==========
        // 主轴对齐 (justify-content)
        // 主轴起始对齐
        'justify-start': 'flex justify-start',
        // 主轴末尾对齐
        'justify-end': 'flex justify-end',
        // 主轴居中对齐
        'justify-center': 'flex justify-center',
        // 主轴两端对齐
        'justify-between': 'flex justify-between',
        // 主轴环绕对齐
        'justify-around': 'flex justify-around',
        // 主轴均匀对齐
        'justify-evenly': 'flex justify-evenly',

        // 交叉轴对齐 (align-items)
        // 交叉轴起始对齐
        'items-start': 'flex items-start',
        // 交叉轴末尾对齐
        'items-end': 'flex items-end',
        // 交叉轴居中对齐
        'items-center': 'flex items-center',
        // 交叉轴基线对齐
        'items-baseline': 'flex items-baseline',
        // 交叉轴拉伸对齐
        'items-stretch': 'flex items-stretch',

        // 多行对齐 (align-content)
        // 多行内容起始对齐
        'content-start': 'flex content-start',
        // 多行内容末尾对齐
        'content-end': 'flex content-end',
        // 多行内容居中对齐
        'content-center': 'flex content-center',
        // 多行内容两端对齐
        'content-between': 'flex content-between',
        // 多行内容环绕对齐
        'content-around': 'flex content-around',
        // 多行内容均匀对齐
        'content-evenly': 'flex content-evenly',

        // ========== Flex 项目快捷类 ==========
        // 项目排序 (order)
        // 项目排在最前
        'order-first': 'order-first',
        // 项目排在最后
        'order-last': 'order-last',
        // 项目不排序
        'order-none': 'order-none',

        // 项目伸缩 (flex-grow/flex-shrink)
        // 项目可伸展
        'flex-grow': 'flex-grow',
        // 项目不伸展
        'flex-grow-0': 'flex-grow-0',
        // 项目可收缩
        'flex-shrink': 'flex-shrink',
        // 项目不收缩
        'flex-shrink-0': 'flex-shrink-0',

        // 项目基准大小 (flex-basis)
        // 项目自动伸缩
        'flex-auto': 'flex-auto',
        // 项目初始大小
        'flex-initial': 'flex-initial',
        // 项目占据剩余空间
        'flex-1': 'flex-1',
        // 项目不伸缩
        'flex-none': 'flex-none',

        // 项目自对齐 (align-self)
        // 项目自动对齐
        'self-auto': 'self-auto',
        // 项目起始对齐
        'self-start': 'self-start',
        // 项目末尾对齐
        'self-end': 'self-end',
        // 项目居中对齐
        'self-center': 'self-center',
        // 项目拉伸对齐
        'self-stretch': 'self-stretch',
        // 项目基线对齐
        'self-baseline': 'self-baseline',

        // ========== Flex 复合快捷类 ==========
        // 水平垂直居中弹性布局
        'flex-center': 'flex items-center justify-center',
        // 两端对齐弹性布局
        'flex-between': 'flex items-center justify-between',
        // 环绕对齐弹性布局
        'flex-around': 'flex items-center justify-around',
        // 起始对齐弹性布局
        'flex-start': 'flex items-center justify-start',
        // 末尾对齐弹性布局
        'flex-end': 'flex items-center justify-end',
        // 垂直居中弹性布局
        'flex-col-center': 'flex flex-col items-center justify-center',
        // 垂直两端对齐弹性布局
        'flex-col-between': 'flex flex-col items-center justify-between',
        // 垂直环绕对齐弹性布局
        'flex-col-around': 'flex flex-col items-center justify-around',
        // 垂直起始对齐弹性布局
        'flex-col-start': 'flex flex-col items-center justify-start',
        // 垂直末尾对齐弹性布局
        'flex-col-end': 'flex flex-col items-center justify-end',
        // 行内水平垂直居中弹性布局
        'inline-flex-center': 'inline-flex items-center justify-center',
        // 拉伸对齐弹性布局
        'flex-stretch': 'flex items-stretch justify-start',
        // 基线对齐弹性布局
        'flex-baseline': 'flex items-baseline justify-start',

        // ========== Grid 容器快捷类 ==========
        // 网格布局
        'grid': 'grid',
        // 行内网格布局
        'inline-grid': 'inline-grid',

        // 列模板
        // 1列网格
        'grid-cols-1': 'grid grid-cols-1',
        // 2列网格
        'grid-cols-2': 'grid grid-cols-2',
        // 3列网格
        'grid-cols-3': 'grid grid-cols-3',
        // 4列网格
        'grid-cols-4': 'grid grid-cols-4',
        // 5列网格
        'grid-cols-5': 'grid grid-cols-5',
        // 6列网格
        'grid-cols-6': 'grid grid-cols-6',
        // 7列网格
        'grid-cols-7': 'grid grid-cols-7',
        // 8列网格
        'grid-cols-8': 'grid grid-cols-8',
        // 9列网格
        'grid-cols-9': 'grid grid-cols-9',
        // 10列网格
        'grid-cols-10': 'grid grid-cols-10',
        // 11列网格
        'grid-cols-11': 'grid grid-cols-11',
        // 12列网格
        'grid-cols-12': 'grid grid-cols-12',
        // 无列模板
        'grid-cols-none': 'grid grid-cols-none',

        // 行模板
        // 1行网格
        'grid-rows-1': 'grid grid-rows-1',
        // 2行网格
        'grid-rows-2': 'grid grid-rows-2',
        // 3行网格
        'grid-rows-3': 'grid grid-rows-3',
        // 4行网格
        'grid-rows-4': 'grid grid-rows-4',
        // 5行网格
        'grid-rows-5': 'grid grid-rows-5',
        // 6行网格
        'grid-rows-6': 'grid grid-rows-6',
        // 无行模板
        'grid-rows-none': 'grid grid-rows-none',

        // 自动列
        // 自动列宽
        'grid-cols-auto': 'grid grid-cols-auto',
        // 最小列宽
        'grid-cols-min': 'grid grid-cols-min',
        // 最大列宽
        'grid-cols-max': 'grid grid-cols-max',
        // 最小最大列宽
        'grid-cols-minmax': 'grid grid-cols-minmax',
        // 自适应列宽
        'grid-cols-fit': 'grid grid-cols-[repeat(auto-fit,minmax(250px,1fr))]',
        // 填充列宽
        'grid-cols-fill': 'grid grid-cols-[repeat(auto-fill,minmax(200px,1fr))]',

        // 流方向
        // 行流方向
        'grid-flow-row': 'grid grid-flow-row',
        // 列流方向
        'grid-flow-col': 'grid grid-flow-col',
        // 密集流方向
        'grid-flow-dense': 'grid grid-flow-dense',
        // 行密集流方向
        'grid-flow-row-dense': 'grid grid-flow-row dense',
        // 列密集流方向
        'grid-flow-col-dense': 'grid grid-flow-col dense',

        // ========== Grid 间隙快捷类 ==========
        // 统一间隙 - 设置网格项目之间的统一间距
        'gap-0': 'gap-0',           // 无间隙
        'gap-1': 'gap-1',           // 0.25rem间隙
        'gap-2': 'gap-2',           // 0.5rem间隙
        'gap-3': 'gap-3',           // 0.75rem间隙
        'gap-4': 'gap-4',           // 1rem间隙
        'gap-5': 'gap-5',           // 1.25rem间隙
        'gap-6': 'gap-6',           // 1.5rem间隙
        'gap-8': 'gap-8',           // 2rem间隙
        'gap-10': 'gap-10',         // 2.5rem间隙
        'gap-12': 'gap-12',         // 3rem间隙
        'gap-16': 'gap-16',         // 4rem间隙
        'gap-20': 'gap-20',         // 5rem间隙
        'gap-24': 'gap-24',         // 6rem间隙
        'gap-32': 'gap-32',         // 8rem间隙
        'gap-40': 'gap-40',         // 10rem间隙
        'gap-48': 'gap-48',         // 12rem间隙
        'gap-56': 'gap-56',         // 14rem间隙
        'gap-64': 'gap-64',         // 16rem间隙
        'gap-px': 'gap-px',         // 1px间隙

        // 水平间隙 - 仅设置网格项目之间的水平间距
        'gap-x-0': 'gap-x-0',       // 无水平间隙
        'gap-x-1': 'gap-x-1',       // 0.25rem水平间隙
        'gap-x-2': 'gap-x-2',       // 0.5rem水平间隙
        'gap-x-3': 'gap-x-3',       // 0.75rem水平间隙
        'gap-x-4': 'gap-x-4',       // 1rem水平间隙
        'gap-x-5': 'gap-x-5',       // 1.25rem水平间隙
        'gap-x-6': 'gap-x-6',       // 1.5rem水平间隙
        'gap-x-8': 'gap-x-8',       // 2rem水平间隙
        'gap-x-10': 'gap-x-10',     // 2.5rem水平间隙
        'gap-x-12': 'gap-x-12',     // 3rem水平间隙
        'gap-x-16': 'gap-x-16',     // 4rem水平间隙
        'gap-x-20': 'gap-x-20',     // 5rem水平间隙
        'gap-x-24': 'gap-x-24',     // 6rem水平间隙
        'gap-x-32': 'gap-x-32',     // 8rem水平间隙
        'gap-x-40': 'gap-x-40',     // 10rem水平间隙
        'gap-x-48': 'gap-x-48',     // 12rem水平间隙
        'gap-x-56': 'gap-x-56',     // 14rem水平间隙
        'gap-x-64': 'gap-x-64',     // 16rem水平间隙
        'gap-x-px': 'gap-x-px',     // 1px水平间隙

        // 垂直间隙 - 仅设置网格项目之间的垂直间距
        'gap-y-0': 'gap-y-0',       // 无垂直间隙
        'gap-y-1': 'gap-y-1',       // 0.25rem垂直间隙
        'gap-y-2': 'gap-y-2',       // 0.5rem垂直间隙
        'gap-y-3': 'gap-y-3',       // 0.75rem垂直间隙
        'gap-y-4': 'gap-y-4',       // 1rem垂直间隙
        'gap-y-5': 'gap-y-5',       // 1.25rem垂直间隙
        'gap-y-6': 'gap-y-6',       // 1.5rem垂直间隙
        'gap-y-8': 'gap-y-8',       // 2rem垂直间隙
        'gap-y-10': 'gap-y-10',     // 2.5rem垂直间隙
        'gap-y-12': 'gap-y-12',     // 3rem垂直间隙
        'gap-y-16': 'gap-y-16',     // 4rem垂直间隙
        'gap-y-20': 'gap-y-20',     // 5rem垂直间隙
        'gap-y-24': 'gap-y-24',     // 6rem垂直间隙
        'gap-y-32': 'gap-y-32',     // 8rem垂直间隙
        'gap-y-40': 'gap-y-40',     // 10rem垂直间隙
        'gap-y-48': 'gap-y-48',     // 12rem垂直间隙
        'gap-y-56': 'gap-y-56',     // 14rem垂直间隙
        'gap-y-64': 'gap-y-64',     // 16rem垂直间隙
        'gap-y-px': 'gap-y-px',     // 1px垂直间隙

        // ========== Grid 项目放置快捷类 ==========
        // 列放置 - 控制网格项目跨越的列数
        'col-auto': 'col-auto',                 // 浏览器自动决定列宽
        'col-span-1': 'col-span-1',             // 跨越1列
        'col-span-2': 'col-span-2',             // 跨越2列
        'col-span-3': 'col-span-3',             // 跨越3列
        'col-span-4': 'col-span-4',             // 跨越4列
        'col-span-5': 'col-span-5',             // 跨越5列
        'col-span-6': 'col-span-6',             // 跨越6列
        'col-span-7': 'col-span-7',             // 跨越7列
        'col-span-8': 'col-span-8',             // 跨越8列
        'col-span-9': 'col-span-9',             // 跨越9列
        'col-span-10': 'col-span-10',           // 跨越10列
        'col-span-11': 'col-span-11',           // 跨越11列
        'col-span-12': 'col-span-12',           // 跨越12列
        'col-span-full': 'col-span-full',       // 跨越所有列

        // 列起始线 - 控制网格项目开始的列线
        'col-start-1': 'col-start-1',           // 从第1条列线开始
        'col-start-2': 'col-start-2',           // 从第2条列线开始
        'col-start-3': 'col-start-3',           // 从第3条列线开始
        'col-start-4': 'col-start-4',           // 从第4条列线开始
        'col-start-5': 'col-start-5',           // 从第5条列线开始
        'col-start-6': 'col-start-6',           // 从第6条列线开始
        'col-start-7': 'col-start-7',           // 从第7条列线开始
        'col-start-8': 'col-start-8',           // 从第8条列线开始
        'col-start-9': 'col-start-9',           // 从第9条列线开始
        'col-start-10': 'col-start-10',         // 从第10条列线开始
        'col-start-11': 'col-start-11',         // 从第11条列线开始
        'col-start-12': 'col-start-12',         // 从第12条列线开始
        'col-start-13': 'col-start-13',         // 从第13条列线开始
        'col-start-auto': 'col-start-auto',     // 浏览器自动决定起始列线

        // 列结束线 - 控制网格项目结束的列线
        'col-end-1': 'col-end-1',               // 在第1条列线结束
        'col-end-2': 'col-end-2',               // 在第2条列线结束
        'col-end-3': 'col-end-3',               // 在第3条列线结束
        'col-end-4': 'col-end-4',               // 在第4条列线结束
        'col-end-5': 'col-end-5',               // 在第5条列线结束
        'col-end-6': 'col-end-6',               // 在第6条列线结束
        'col-end-7': 'col-end-7',               // 在第7条列线结束
        'col-end-8': 'col-end-8',               // 在第8条列线结束
        'col-end-9': 'col-end-9',               // 在第9条列线结束
        'col-end-10': 'col-end-10',             // 在第10条列线结束
        'col-end-11': 'col-end-11',             // 在第11条列线结束
        'col-end-12': 'col-end-12',             // 在第12条列线结束
        'col-end-13': 'col-end-13',             // 在第13条列线结束
        'col-end-auto': 'col-end-auto',         // 浏览器自动决定结束列线

        // 行放置 - 控制网格项目跨越的行数
        'row-auto': 'row-auto',                 // 浏览器自动决定行高
        'row-span-1': 'row-span-1',             // 跨越1行
        'row-span-2': 'row-span-2',             // 跨越2行
        'row-span-3': 'row-span-3',             // 跨越3行
        'row-span-4': 'row-span-4',             // 跨越4行
        'row-span-5': 'row-span-5',             // 跨越5行
        'row-span-6': 'row-span-6',             // 跨越6行
        'row-span-full': 'row-span-full',       // 跨越所有行

        // 行起始线 - 控制网格项目开始的行线
        'row-start-1': 'row-start-1',           // 从第1条行线开始
        'row-start-2': 'row-start-2',           // 从第2条行线开始
        'row-start-3': 'row-start-3',           // 从第3条行线开始
        'row-start-4': 'row-start-4',           // 从第4条行线开始
        'row-start-5': 'row-start-5',           // 从第5条行线开始
        'row-start-6': 'row-start-6',           // 从第6条行线开始
        'row-start-7': 'row-start-7',           // 从第7条行线开始
        'row-start-auto': 'row-start-auto',     // 浏览器自动决定起始行线

        // 行结束线 - 控制网格项目结束的行线
        'row-end-1': 'row-end-1',               // 在第1条行线结束
        'row-end-2': 'row-end-2',               // 在第2条行线结束
        'row-end-3': 'row-end-3',               // 在第3条行线结束
        'row-end-4': 'row-end-4',               // 在第4条行线结束
        'row-end-5': 'row-end-5',               // 在第5条行线结束
        'row-end-6': 'row-end-6',               // 在第6条行线结束
        'row-end-7': 'row-end-7',               // 在第7条行线结束
        'row-end-auto': 'row-end-auto',         // 浏览器自动决定结束行线

        // ========== Grid 对齐快捷类 ==========
        // 容器内项目对齐 - 控制网格容器内所有项目的对齐方式
        'justify-items-start': 'justify-items-start',       // 网格项目在网格区域中水平起始对齐
        'justify-items-end': 'justify-items-end',           // 网格项目在网格区域中水平末尾对齐
        'justify-items-center': 'justify-items-center',     // 网格项目在网格区域中水平居中对齐
        'justify-items-stretch': 'justify-items-stretch',   // 网格项目在网格区域中水平拉伸对齐

        'align-items-start': 'align-items-start',           // 网格项目在网格区域中垂直起始对齐
        'align-items-end': 'align-items-end',               // 网格项目在网格区域中垂直末尾对齐
        'align-items-center': 'align-items-center',         // 网格项目在网格区域中垂直居中对齐
        'align-items-stretch': 'align-items-stretch',       // 网格项目在网格区域中垂直拉伸对齐

        'place-items-start': 'place-items-start',           // 网格项目在网格区域中水平和垂直都起始对齐
        'place-items-end': 'place-items-end',               // 网格项目在网格区域中水平和垂直都末尾对齐
        'place-items-center': 'place-items-center',         // 网格项目在网格区域中水平和垂直都居中对齐
        'place-items-stretch': 'place-items-stretch',       // 网格项目在网格区域中水平和垂直都拉伸对齐

        // 容器本身对齐 - 控制单个网格项目在其网格区域内的对齐方式
        'justify-self-auto': 'justify-self-auto',           // 网格项目水平对齐方式由justify-items属性决定
        'justify-self-start': 'justify-self-start',         // 网格项目在网格区域中水平起始对齐
        'justify-self-end': 'justify-self-end',             // 网格项目在网格区域中水平末尾对齐
        'justify-self-center': 'justify-self-center',       // 网格项目在网格区域中水平居中对齐
        'justify-self-stretch': 'justify-self-stretch',     // 网格项目在网格区域中水平拉伸对齐

        'align-self-auto': 'align-self-auto',               // 网格项目垂直对齐方式由align-items属性决定
        'align-self-start': 'align-self-start',             // 网格项目在网格区域中垂直起始对齐
        'align-self-end': 'align-self-end',                 // 网格项目在网格区域中垂直末尾对齐
        'align-self-center': 'align-self-center',           // 网格项目在网格区域中垂直居中对齐
        'align-self-stretch': 'align-self-stretch',         // 网格项目在网格区域中垂直拉伸对齐

        'place-self-auto': 'place-self-auto',               // 网格项目对齐方式由place-items属性决定
        'place-self-start': 'place-self-start',             // 网格项目在网格区域中水平和垂直都起始对齐
        'place-self-end': 'place-self-end',                 // 网格项目在网格区域中水平和垂直都末尾对齐
        'place-self-center': 'place-self-center',           // 网格项目在网格区域中水平和垂直都居中对齐
        'place-self-stretch': 'place-self-stretch',         // 网格项目在网格区域中水平和垂直都拉伸对齐

        // ========== Grid 复合快捷类 ==========
        'grid-center': 'grid place-items-center',           // 创建网格容器并使所有项目居中对齐
        'grid-start': 'grid place-items-start',             // 创建网格容器并使所有项目起始对齐
        'grid-end': 'grid place-items-end',                 // 创建网格容器并使所有项目末尾对齐
        'grid-stretch': 'grid place-items-stretch',         // 创建网格容器并使所有项目拉伸对齐
        'grid-area-center': 'grid place-items-center',      // 创建网格容器并使所有项目居中对齐（重复定义）
        'grid-template-areas': 'grid grid-template-areas',  // 创建网格容器并应用网格区域模板
        'grid-area-header': 'grid-area-header',             // 应用头部网格区域
        'grid-area-sidebar': 'grid-area-sidebar',           // 应用侧边栏网格区域
        'grid-area-main': 'grid-area-main',                 // 应用主内容网格区域
        'grid-area-footer': 'grid-area-footer',             // 应用底部网格区域

        // ========== 响应式网格快捷类 ==========
        'grid-responsive': 'grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6',  // 响应式网格，随屏幕尺寸增加列数
        'grid-responsive-2': 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',  // 响应式网格，最多4列
        'grid-responsive-3': 'grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4', // 响应式网格，最多4列
        'grid-card': 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6',    // 卡片网格布局，设置项目间距
        'grid-dashboard': 'grid grid-cols-1 lg:grid-cols-4 gap-6',                            // 仪表板网格布局，设置项目间距
        'grid-form': 'grid grid-cols-1 md:grid-cols-2 gap-4',                                 // 表单网格布局，设置项目间距
        'grid-form-3': 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4',                // 三列表单网格布局，设置项目间距

        // 全宽容器 - 移动端全宽，大屏固定宽度
        'container-full': 'w-full mx-auto px-1',
        // 响应式全宽容器变体
        'container-full-xs': 'container-full xs:max-w-screen-xs',
        'container-full-sm': 'container-full sm:max-w-screen-sm',
        'container-full-md': 'container-full md:max-w-screen-md',
        'container-full-lg': 'container-full lg:max-w-screen-lg',
        'container-full-xl': 'container-full xl:max-w-screen-xl',
        'container-full-2xl': 'container-full 2xl:max-w-screen-2xl',
        'container-full-3xl': 'container-full 3xl:max-w-screen-3xl',
        'container-full-4xl': 'container-full 4xl:max-w-screen-4xl',    


    }
})