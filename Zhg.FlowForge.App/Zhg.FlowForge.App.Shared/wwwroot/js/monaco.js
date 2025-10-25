// wwwroot/js/monaco.js

let monacoEditor = null;
let monacoInstance = null;
let dotNetHelper = null;
let isMonacoLoaded = false;

// 加载 Monaco Editor 的脚本
function loadMonacoLoader() {
    return new Promise((resolve, reject) => {
        if (window.require) {
            resolve();
            return;
        }

        const script = document.createElement('script');
        script.src = 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs/loader.min.js';
        script.onload = resolve;
        script.onerror = reject;
        document.head.appendChild(script);
    });
}

window.initMonacoEditor = async (options) => {
    try {
        // 先加载 Monaco loader
        await loadMonacoLoader();

        // 配置 Monaco
        window.require.config({
            paths: {
                'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs'
            }
        });

        return new Promise((resolve, reject) => {
            window.require(['vs/editor/editor.main'], () => {
                try {
                    const container = document.getElementById(options.containerId);
                    if (!container) {
                        reject(new Error('Editor container not found'));
                        return;
                    }

                    // 销毁旧实例
                    if (monacoEditor) {
                        monacoEditor.dispose();
                    }

                    // 创建编辑器实例
                    monacoEditor = monaco.editor.create(container, {
                        value: '',
                        language: 'csharp',
                        theme: options.theme || 'vs-dark',
                        fontSize: options.fontSize || 14,
                        tabSize: options.tabSize || 4,
                        automaticLayout: options.automaticLayout !== false,
                        minimap: options.minimap || { enabled: true },
                        scrollBeyondLastLine: false,
                        wordWrap: options.wordWrap || 'off',
                        renderWhitespace: 'selection',
                        folding: options.folding !== false,
                        lineNumbers: options.lineNumbers || 'on',
                        roundedSelection: false,
                        scrollbar: {
                            vertical: 'auto',
                            horizontal: 'auto',
                            useShadows: false,
                            verticalScrollbarSize: 10,
                            horizontalScrollbarSize: 10
                        },
                        suggestOnTriggerCharacters: true,
                        quickSuggestions: true,
                        parameterHints: { enabled: true }
                    });

                    // 配置 C# 语言支持
                    monaco.languages.registerCompletionItemProvider('csharp', {
                        provideCompletionItems: (model, position) => {
                            return {
                                suggestions: getCSharpCompletions(model, position)
                            };
                        }
                    });

                    // 监听内容变化
                    monacoEditor.onDidChangeModelContent(() => {
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnContentChanged');
                        }
                    });

                    // 监听光标位置变化
                    monacoEditor.onDidChangeCursorPosition((e) => {
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnCursorPositionChanged',
                                e.position.lineNumber,
                                e.position.column);
                        }
                    });

                    // 监听选择变化
                    monacoEditor.onDidChangeCursorSelection((e) => {
                        if (dotNetHelper) {
                            const selection = monacoEditor.getSelection();
                            const selectedText = monacoEditor.getModel().getValueInRange(selection);
                            dotNetHelper.invokeMethodAsync('OnSelectionChanged', selectedText.length);
                        }
                    });

                    monacoInstance = monaco;
                    isMonacoLoaded = true;
                    console.log('Monaco Editor 初始化成功');
                    resolve();
                } catch (error) {
                    console.error('Monaco Editor 创建失败:', error);
                    reject(error);
                }
            }, (error) => {
                console.error('Monaco Editor 加载失败:', error);
                reject(error);
            });
        });
    } catch (error) {
        console.error('Monaco Loader 加载失败:', error);
        throw error;
    }
};

window.registerMonacoCallbacks = (helper) => {
    dotNetHelper = helper;
    console.log('Monaco 回调已注册');
};

window.registerMonacoKeybinding = (key, command) => {
    if (!monacoEditor) return;

    // 解析快捷键
    const keys = key.toLowerCase().split('+');
    let keyCode = 0;
    let keyMod = 0;

    keys.forEach(k => {
        switch (k) {
            case 'ctrl':
            case 'cmd':
                keyMod |= monaco.KeyMod.CtrlCmd;
                break;
            case 'shift':
                keyMod |= monaco.KeyMod.Shift;
                break;
            case 'alt':
                keyMod |= monaco.KeyMod.Alt;
                break;
            case 's':
                keyCode = monaco.KeyCode.KeyS;
                break;
            case 'w':
                keyCode = monaco.KeyCode.KeyW;
                break;
            case 'n':
                keyCode = monaco.KeyCode.KeyN;
                break;
            case 'f':
                keyCode = monaco.KeyCode.KeyF;
                break;
            case 'z':
                keyCode = monaco.KeyCode.KeyZ;
                break;
            case 'y':
                keyCode = monaco.KeyCode.KeyY;
                break;
            case 'f5':
                keyCode = monaco.KeyCode.F5;
                break;
        }
    });

    if (keyCode) {
        monacoEditor.addCommand(keyMod | keyCode, () => {
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('HandleKeybinding', command);
            }
        });
    }
};

window.setMonacoContent = (content, language) => {
    if (!monacoEditor) {
        console.warn('Monaco Editor 未初始化');
        return;
    }

    try {
        const model = monacoEditor.getModel();
        if (language && model) {
            monaco.editor.setModelLanguage(model, language);
        }
        monacoEditor.setValue(content || '');
        console.log('内容已设置, 语言:', language);
    } catch (error) {
        console.error('设置内容失败:', error);
    }
};

window.getMonacoContent = () => {
    if (!monacoEditor) {
        console.warn('Monaco Editor 未初始化');
        return '';
    }
    return monacoEditor.getValue();
};

window.clearMonacoContent = () => {
    if (monacoEditor) {
        monacoEditor.setValue('');
    }
};

window.formatMonacoDocument = () => {
    if (monacoEditor) {
        monacoEditor.getAction('editor.action.formatDocument').run();
    }
};

window.showMonacoFind = () => {
    if (monacoEditor) {
        monacoEditor.getAction('actions.find').run();
    }
};

window.monacoUndo = () => {
    if (monacoEditor) {
        monacoEditor.trigger('keyboard', 'undo');
    }
};

window.monacoRedo = () => {
    if (monacoEditor) {
        monacoEditor.trigger('keyboard', 'redo');
    }
};

window.setMonacoTheme = (theme) => {
    if (monacoInstance) {
        monaco.editor.setTheme(theme);
    }
};

window.jumpToLine = (line) => {
    if (monacoEditor) {
        monacoEditor.revealLineInCenter(line);
        monacoEditor.setPosition({ lineNumber: line, column: 1 });
        monacoEditor.focus();
    }
};

window.jumpToPosition = (line, column) => {
    if (monacoEditor) {
        monacoEditor.revealPositionInCenter({ lineNumber: line, column: column });
        monacoEditor.setPosition({ lineNumber: line, column: column });
        monacoEditor.focus();
    }
};

window.setMonacoDiagnostics = (diagnostics) => {
    if (!monacoInstance || !monacoEditor) return;

    try {
        const model = monacoEditor.getModel();
        const markers = diagnostics.map(d => ({
            severity: d.severity === 'Error'
                ? monaco.MarkerSeverity.Error
                : d.severity === 'Warning'
                    ? monaco.MarkerSeverity.Warning
                    : monaco.MarkerSeverity.Info,
            startLineNumber: d.line,
            startColumn: d.column,
            endLineNumber: d.line,
            endColumn: d.column + 100,
            message: `${d.code}: ${d.message}`
        }));

        monaco.editor.setModelMarkers(model, 'csharp', markers);
    } catch (error) {
        console.error('设置诊断信息失败:', error);
    }
};

window.searchInMonaco = (query) => {
    if (monacoEditor) {
        monacoEditor.getAction('actions.find').run();
        // 自动填充搜索框
        setTimeout(() => {
            const findController = monacoEditor.getContribution('editor.contrib.findController');
            if (findController) {
                findController.start({
                    forceRevealReplace: false,
                    seedSearchStringFromSelection: false,
                    seedSearchStringFromGlobalClipboard: false,
                    shouldFocus: 1,
                    shouldAnimate: true
                });
            }
        }, 100);
    }
};

// C# 代码补全
function getCSharpCompletions(model, position) {
    const word = model.getWordUntilPosition(position);
    const range = {
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        endColumn: word.endColumn
    };

    const suggestions = [
        // 关键字
        ...['class', 'interface', 'namespace', 'using', 'public', 'private',
            'protected', 'internal', 'static', 'async', 'await', 'var',
            'return', 'if', 'else', 'for', 'foreach', 'while', 'switch',
            'case', 'break', 'continue', 'try', 'catch', 'finally', 'throw',
            'new', 'this', 'base', 'null', 'true', 'false', 'string', 'int',
            'bool', 'void', 'double', 'float', 'decimal', 'long', 'byte'].map(kw => ({
                label: kw,
                kind: monaco.languages.CompletionItemKind.Keyword,
                insertText: kw,
                range: range
            })),

        // 代码片段
        {
            label: 'prop',
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: 'public ${1:string} ${2:PropertyName} { get; set; }',
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            documentation: '自动属性',
            range: range
        },
        {
            label: 'propfull',
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: [
                'private ${1:string} _${2:fieldName};',
                'public ${1:string} ${3:PropertyName}',
                '{',
                '\tget { return _${2:fieldName}; }',
                '\tset { _${2:fieldName} = value; }',
                '}'
            ].join('\n'),
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            documentation: '完整属性',
            range: range
        },
        {
            label: 'ctor',
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: 'public ${1:ClassName}()\n{\n\t$0\n}',
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            documentation: '构造函数',
            range: range
        },
        {
            label: 'method',
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: 'public ${1:void} ${2:MethodName}()\n{\n\t$0\n}',
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            documentation: '方法',
            range: range
        },
        {
            label: 'cw',
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: 'Console.WriteLine($"$0");',
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            documentation: 'Console.WriteLine',
            range: range
        },
        {
            label: 'class',
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: [
                'public class ${1:ClassName}',
                '{',
                '\t$0',
                '}'
            ].join('\n'),
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            documentation: '类定义',
            range: range
        }
    ];

    return { suggestions };
}

// 清理函数
window.disposeMonacoEditor = () => {
    if (monacoEditor) {
        monacoEditor.dispose();
        monacoEditor = null;
    }
    dotNetHelper = null;
    isMonacoLoaded = false;
};