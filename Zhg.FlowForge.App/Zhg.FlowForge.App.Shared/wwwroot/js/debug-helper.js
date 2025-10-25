// 调试辅助函数
window.debugMonaco = () => {
    console.log('Monaco Debug Info:');
    console.log('- Editor Instance:', window.monacoEditor);
    console.log('- Monaco Instance:', window.monacoInstance);
    console.log('- Is Loaded:', !!window.monaco);
    console.log('- Require Available:', !!window.require);

    if (window.monacoEditor) {
        console.log('- Current Model:', window.monacoEditor.getModel());
        console.log('- Current Value Length:', window.monacoEditor.getValue().length);
        console.log('- Current Language:', window.monacoEditor.getModel()?.getLanguageId());
    }
};

// 文件操作调试
window.debugFileOperations = () => {
    console.log('File Operations Test:');
    console.log('- prompt available:', typeof window.prompt === 'function');
    console.log('- confirm available:', typeof window.confirm === 'function');
    console.log('- alert available:', typeof window.alert === 'function');
};

// 快捷键测试
window.testKeybinding = (key, command) => {
    console.log(`Testing keybinding: ${key} -> ${command}`);
    if (window.registerMonacoKeybinding) {
        window.registerMonacoKeybinding(key, command);
        console.log('Keybinding registered successfully');
    } else {
        console.error('registerMonacoKeybinding not available');
    }
};

// 全局错误捕获
window.addEventListener('error', (event) => {
    console.error('Global Error:', {
        message: event.message,
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
        error: event.error
    });
});

// 未处理的 Promise 错误
window.addEventListener('unhandledrejection', (event) => {
    console.error('Unhandled Promise Rejection:', {
        reason: event.reason,
        promise: event.promise
    });
});

console.log('Debug Helper Loaded. Use debugMonaco() and debugFileOperations() in console.');