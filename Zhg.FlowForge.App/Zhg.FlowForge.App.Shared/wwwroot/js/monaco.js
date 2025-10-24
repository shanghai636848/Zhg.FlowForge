// Monaco Editor 初始化和管理

let monacoEditor = null;
let monacoInstance = null;
let dotNetHelper = null;

window.initMonacoEditor = async (options) => {
    require.config({
        paths: {
            'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs'
        }
    });

    return new Promise((resolve) => {
        require(['vs/editor/editor.main'], () => {
            monacoEditor = monaco.editor.create(
                document.getElementById(options.containerId),
                {
                    value: '',
                    language: 'csharp',
                    theme: options.theme || 'vs-dark',
                    fontSize: options.fontSize || 14,
                    tabSize: options.tabSize || 4,
                    automaticLayout: options.automaticLayout !== false,
                    minimap: options.minimap || { enabled: true },
                    scrollBeyondLastLine: false,
                    renderWhitespace: 'selection',
                    suggestOnTriggerCharacters: true,
                    quickSuggestions: true,
                    parameterHints: { enabled: true },
                    folding: true,
                    lineNumbers: 'on',
                    roundedSelection: false,
                    scrollbar: {
                        vertical: 'auto',
                        horizontal: 'auto'
                    }
                }
            );

            // 配置 C# 语言支持
            monaco.languages.registerCompletionItemProvider('csharp', {
                provideCompletionItems: (model, position) => {
                    return {
                        suggestions: getCSharpCompletions(model, position)
                    };
                }
            });

            // 注册快捷键
            monacoEditor.addCommand(
                monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS,
                () => {
                    if (dotNetHelper) {
                        dotNetHelper.invokeMethodAsync('OnSaveRequested');
                    }
                }
            );

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

            monacoInstance = monaco;
            resolve();
        });
    });
};

window.registerMonacoCallbacks = (helper) => {
    dotNetHelper = helper;
};

window.setMonacoContent = (content, language) => {
    if (monacoEditor) {
        const model = monacoEditor.getModel();
        if (language && model) {
            monaco.editor.setModelLanguage(model, language);
        }
        monacoEditor.setValue(content || '');
    }
};

window.getMonacoContent = () => {
    return monacoEditor ? monacoEditor.getValue() : '';
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

    const model = monacoEditor.getModel();
    const markers = diagnostics.map(d => ({
        severity: d.severity === 'Error'
            ? monacoInstance.MarkerSeverity.Error
            : d.severity === 'Warning'
                ? monacoInstance.MarkerSeverity.Warning
                : monacoInstance.MarkerSeverity.Info,
        startLineNumber: d.line,
        startColumn: d.column,
        endLineNumber: d.line,
        endColumn: d.column + 100,
        message: `${d.code}: ${d.message}`
    }));

    monacoInstance.editor.setModelMarkers(model, 'csharp', markers);
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
            'new', 'this', 'base', 'null', 'true', 'false'].map(kw => ({
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
        }
    ];

    return { suggestions };
}