// wwwroot/js/index.js - 主入口文件

// 导入所有模块
import * as monaco from './monaco.js';
import * as fileSystem from './fileSystem.js';
import * as bpmn from './bpmn-interop.js';
import * as deployment from './deployment.js';
import * as communication from './interop-communication.js';
import * as scroll from './scroll-service.js';
import * as debug from './debug-helper.js';

// 全局命名空间
window.FlowForge = {
    Monaco: monaco,
    FileSystem: fileSystem,
    Bpmn: bpmn,
    Deployment: deployment,
    Communication: communication,
    Scroll: scroll,
    Debug: debug,

    // 版本信息
    version: '1.0.0',

    // 初始化状态
    isInitialized: false,

    // 全局初始化
    initialize() {
        if (this.isInitialized) {
            console.warn('FlowForge already initialized');
            return;
        }

        console.log(`%c
╔═══════════════════════════════════════╗
║                                                                              ║
║         FlowForge v${this.version}                                           ║
║     工作流可视化开发平台                                                     ║
║                                                                              ║
╚═══════════════════════════════════════╝
        `, 'color: #4F46E5; font-weight: bold;');

        this.isInitialized = true;
        console.log('✅ FlowForge initialized successfully');
    }
};

// 向后兼容：将函数挂载到 window
Object.assign(window, {
    // Monaco
    initMonacoEditor: monaco.initMonacoEditor,
    registerMonacoCallbacks: monaco.registerMonacoCallbacks,
    registerMonacoKeybinding: monaco.registerMonacoKeybinding,
    setMonacoContent: monaco.setMonacoContent,
    getMonacoContent: monaco.getMonacoContent,
    formatMonacoDocument: monaco.formatMonacoDocument,
    showMonacoFind: monaco.showMonacoFind,
    monacoUndo: monaco.monacoUndo,
    monacoRedo: monaco.monacoRedo,
    setMonacoTheme: monaco.setMonacoTheme,
    jumpToLine: monaco.jumpToLine,
    jumpToPosition: monaco.jumpToPosition,
    setMonacoDiagnostics: monaco.setMonacoDiagnostics,
    searchInMonaco: monaco.searchInMonaco,
    disposeMonacoEditor: monaco.disposeMonacoEditor,

    // File System
    promptDirectory: fileSystem.promptDirectory,
    openInExplorer: fileSystem.openInExplorer,
    checkFileSystemPermission: fileSystem.checkFileSystemPermission,
    downloadProjectAsZip: fileSystem.downloadProjectAsZip,

    // BPMN
    initBpmnModeler: bpmn.initBpmnModeler,
    exportBpmn: bpmn.exportBpmn,
    importBpmn: bpmn.importBpmn,
    updateProperty: bpmn.updateProperty,
    disposeBpmnModeler: bpmn.disposeBpmnModeler,

    // Deployment
    downloadFile: deployment.downloadFile,
    scrollToBottom: deployment.scrollToBottom,
    scrollToTop: deployment.scrollToTop,
    scrollToElement: deployment.scrollToElement,
    openFolder: deployment.openFolder,
    copyToClipboard: deployment.copyToClipboard,

    // Communication
    publishEvent: communication.publishEvent,
    subscribeEvent: communication.subscribeEvent,

    // Scroll
    initScrollService: scroll.initScrollService,
    getScrollPosition: scroll.getScrollPosition,
    setScrollPosition: scroll.setScrollPosition,

    // Debug
    debugMonaco: debug.debugMonaco,
    debugFileOperations: debug.debugFileOperations
});

// 自动初始化
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.FlowForge.initialize();
    });
} else {
    window.FlowForge.initialize();
}

// 导出模块
export {
    monaco,
    fileSystem,
    bpmn,
    deployment,
    communication,
    scroll,
    debug
};

export default window.FlowForge;