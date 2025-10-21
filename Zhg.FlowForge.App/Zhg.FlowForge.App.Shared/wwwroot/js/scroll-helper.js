// 使用 ES6 模块导出
export function initScrollService(dotnetHelper) {
    const handler = () => {
        // 节流：避免高频调用
        if (!window._scrollThrottle) {
            window._scrollThrottle = true;
            requestAnimationFrame(() => {
                dotnetHelper.invokeMethodAsync('OnScroll', window.scrollY)
                    .finally(() => window._scrollThrottle = false);
            });
        }
    };

    window.addEventListener('scroll', handler, { passive: true });
    handler(); // 初始化

    // 返回清理函数
    return () => {
        window.removeEventListener('scroll', handler);
        delete window._scrollThrottle;
    };
}