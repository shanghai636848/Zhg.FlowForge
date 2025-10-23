// bpmn-js-interop.js
let bpmnModeler = null;
let currentElement = null;
let dotnetHelper = null;

// 创建全局函数以便从 Blazor 直接调用
window.createBpmnElement = function (type, event) {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }

    try {
        const elementFactory = bpmnModeler.get('elementFactory');
        const create = bpmnModeler.get('create');
        const shape = elementFactory.createShape({ type: type });

        // 创建模拟事件对象，包含必要的属性
        const simulatedEvent = {
            clientX: event.clientX || 100,
            clientY: event.clientY || 100,
            pageX: event.pageX || 100,
            pageY: event.pageY || 100,
            target: document.getElementById('canvas'),
            preventDefault: function () { },
            stopPropagation: function () { }
        };

        create.start(simulatedEvent, shape);
        console.log(`Created BPMN element: ${type}`);
    } catch (error) {
        console.error('Error creating BPMN element:', error);
    }
};

export function initBpmnModeler(dotnetRef, containerId) {
    if (bpmnModeler) {
        console.warn('BPMN Modeler already initialized');
        return true;
    }

    // 检查 BPMN.js 是否可用
    if (typeof BpmnJS === 'undefined') {
        console.error('BPMN.js is not available. Make sure the script is loaded.');
        return false;
    }

    dotnetHelper = dotnetRef;

    try {
        const customModdle = {
            name: 'zhg',
            uri: 'https://www.kswitvalley.net/bpmn',
            prefix: 'zhg',
            types: [
                {
                    name: 'inputOutput',
                    superClass: ['Element'],
                    properties: [
                        {
                            name: 'inputParameters',
                            type: 'zhg:inputParameter',
                            isMany: true
                        },
                        {
                            name: 'outputParameters',
                            type: 'zhg:outputParameter',
                            isMany: true
                        }
                    ]
                },
                {
                    name: 'inputParameter',
                    superClass: ['Element'],
                    properties: [
                        {
                            name: 'name',
                            type: 'String',
                            isAttr: true
                        },
                        {
                            name: 'type',
                            type: 'String',
                            isAttr: true
                        },
                        {
                            name: 'value',
                            type: 'String'
                        }
                    ]
                },
                {
                    name: 'outputParameter',
                    superClass: ['Element'],
                    properties: [
                        {
                            name: 'name',
                            type: 'String',
                            isAttr: true
                        },
                        {
                            name: 'type',
                            type: 'String',
                            isAttr: true
                        },
                        {
                            name: 'value',
                            type: 'String'
                        }
                    ]
                },
                {
                    name: 'class',
                    superClass: ['Element'],
                    properties: [
                        {
                            name: 'class',
                            type: 'String',
                            isAttr: true
                        }
                    ]
                }
            ]
        };

        bpmnModeler = new BpmnJS({
            container: '#' + containerId,
            keyboard: { bindTo: window },
            moddleExtensions: {
                zhg: customModdle
            },
            grid: {
                enabled: true,
                size: 20,
                snapToGrid: true
            }
        });

        const emptyXml = `<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
                  xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"
                  xmlns:dc="http://www.omg.org/spec/DD/20100524/DC"
                  xmlns:di="http://www.omg.org/spec/DD/20100524/DI"
                  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xmlns:zhg="https://www.kswitvalley.net/bpmn"
                  id="Definitions_1"
                  targetNamespace="http://bpmn.io/schema/bpmn">
  <bpmn:process id="Process_1" isExecutable="true"/>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_1"/>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>`;

        return bpmnModeler.importXML(emptyXml)
            .then(() => {
                setupEventListeners();
                console.log('BPMN Modeler initialized successfully');
                return true;
            })
            .catch(error => {
                console.error('Failed to initialize BPMN Modeler:', error);
                return false;
            });

    } catch (error) {
        console.error('Error initializing BPMN Modeler:', error);
        return false;
    }
}

//function setupEventListeners() {
//    if (!bpmnModeler) return;

//    bpmnModeler.on('selection.changed', ({ newSelection }) => {
//        if (newSelection.length === 1) {
//            const element = newSelection[0];
//            currentElement = element;
//            if (dotnetHelper) {
//                dotnetHelper.invokeMethodAsync('OnElementSelected',
//                    element.id,
//                    element.type,
//                    element.businessObject.name || '');
//            }
//        } else {
//            currentElement = null;
//            if (dotnetHelper) {
//                dotnetHelper.invokeMethodAsync('OnElementDeselected');
//            }
//        }
//    });

//    bpmnModeler.on('element.changed', (event) => {
//        if (dotnetHelper) {
//            dotnetHelper.invokeMethodAsync('OnElementChanged',
//                event.element.id,
//                event.element.type);
//        }
//    });

//    bpmnModeler.on('canvas.viewbox.changed', ({ viewbox }) => {
//        if (dotnetHelper) {
//            dotnetHelper.invokeMethodAsync('OnZoomChanged', viewbox.scale);
//        }
//    });

//    bpmnModeler.on('import.done', () => {
//        console.log('BPMN diagram imported successfully');
//    });

//    bpmnModeler.on('saveXML.done', (event) => {
//        console.log('BPMN XML saved successfully');
//    });
//}

function setupEventListeners() {
    // 监听元素选择变化
    bpmnModeler.on('selection.changed', (event) => {
        console.log('Selection changed:', event.newSelection);

        if (event.newSelection && event.newSelection.length === 1) {
            const element = event.newSelection[0];
            currentElement = element;

            // 获取元素业务对象
            const bo = element.businessObject;
            const elementName = bo.name || '';
            const elementType = element.type || '';
            const elementId = element.id || '';

            console.log('Selected element:', { id: elementId, type: elementType, name: elementName });

            // 调用 Blazor 方法
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnElementSelected', elementId, elementType, elementName)
                    .catch(err => console.error('Failed to invoke OnElementSelected:', err));
            }
        } else {
            currentElement = null;
            console.log('Selection cleared');

            // 调用 Blazor 方法清除选择
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnElementDeselected')
                    .catch(err => console.error('Failed to invoke OnElementDeselected:', err));
            }
        }
    });

    // 监听元素变化
    bpmnModeler.on('element.changed', (event) => {
        const element = event.element;
        console.log('Element changed:', element.id, element.type);

        if (dotnetHelper && element) {
            dotnetHelper.invokeMethodAsync('OnElementChanged', element.id, element.type)
                .catch(err => console.error('Failed to invoke OnElementChanged:', err));
        }
    });

    // 监听画布视图变化
    bpmnModeler.on('canvas.viewbox.changed', (event) => {
        if (dotnetHelper && event.viewbox) {
            dotnetHelper.invokeMethodAsync('OnZoomChanged', event.viewbox.scale)
                .catch(err => console.error('Failed to invoke OnZoomChanged:', err));
        }
    });

    // 监听元素点击事件（备用）
    bpmnModeler.on('element.click', (event) => {
        console.log('Element clicked:', event.element);
    });
}

function initToolboxDragAndDrop() {
    document.addEventListener('mousedown', function (ev) {
        const toolItem = ev.target.closest('.tool-item');
        if (!toolItem) return;

        const type = toolItem.dataset.type;
        if (!type) return;

        const elementFactory = bpmnModeler.get('elementFactory');
        const create = bpmnModeler.get('create');
        const shape = elementFactory.createShape({ type });

        create.start(ev, shape);
    });
}

// 文件操作功能
// 文件操作功能 - 使用 Promise 版本
export function exportBpmn() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        alert('BPMN 编辑器未初始化，无法导出');
        return Promise.reject('BPMN Modeler not initialized');
    }

    console.log('开始导出 BPMN 文件...');

    try {
        // 直接调用 saveXML，它返回一个 Promise
        const savePromise = bpmnModeler.saveXML({ format: true });

        // 设置超时
        const timeoutPromise = new Promise((resolve, reject) => {
            setTimeout(() => {
                reject(new Error('导出操作超时，请重试'));
            }, 10000);
        });

        return Promise.race([savePromise, timeoutPromise])
            .then(result => {
                // 注意：BPMN.js 的 saveXML 返回的是 { xml: string }，但不同版本可能直接返回字符串
                let xml = result;
                if (typeof result === 'object' && result.xml) {
                    xml = result.xml;
                }

                if (!xml || xml.trim().length === 0) {
                    throw new Error('导出的 XML 内容为空');
                }

                console.log('成功生成 BPMN XML，长度:', xml.length);
                downloadXmlFile(xml);
                console.log('BPMN 文件导出完成');
                return xml;
            });
    } catch (error) {
        console.error('导出过程中发生错误:', error);
        return Promise.reject(error);
    }
}

// 独立的下载函数
function downloadXmlFile(xml) {
    // 创建 Blob
    const blob = new Blob([xml], {
        type: 'application/bpmn+xml;charset=utf-8'
    });

    // 创建下载链接
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `bpmn-diagram-${new Date().getTime()}.bpmn`;

    // 使用更可靠的方式触发下载
    document.body.appendChild(a);

    // 创建点击事件
    const clickEvent = new MouseEvent('click', {
        view: window,
        bubbles: true,
        cancelable: true
    });

    a.dispatchEvent(clickEvent);
    document.body.removeChild(a);

    // 延迟清理 URL
    setTimeout(() => {
        URL.revokeObjectURL(url);
    }, 1000);
}


export function importBpmn() {
    return new Promise((resolve, reject) => {
        if (!bpmnModeler) {
            reject('BPMN Modeler not initialized');
            return;
        }

        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.bpmn,.xml';

        input.onchange = e => {
            const file = e.target.files[0];
            if (!file) {
                reject('No file selected');
                return;
            }

            const reader = new FileReader();
            reader.onload = ev => {
                bpmnModeler.importXML(ev.target.result)
                    .then(() => {
                        fitViewport();
                        if (dotnetHelper) {
                            dotnetHelper.invokeMethodAsync('OnDiagramImported');
                        }
                        resolve('Import successful');
                    })
                    .catch(err => reject('Import failed: ' + err.message));
            };
            reader.onerror = () => reject('File reading error');
            reader.readAsText(file);
        };

        input.click();
    });
}

export function saveBpmn() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }

    bpmnModeler.saveXML({ format: true }, (err, xml) => {
        if (err) {
            alert('保存失败: ' + err.message);
            return;
        }

        const saved = {
            xml,
            timestamp: new Date().toISOString(),
            version: '1.0'
        };
        const data = JSON.stringify(saved);
        const blob = new Blob([data], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'bpmn-save.json';
        a.click();
        URL.revokeObjectURL(url);

        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnDiagramSaved');
        }
    });
}

export function loadBpmn() {
    return new Promise((resolve, reject) => {
        if (!bpmnModeler) {
            reject('BPMN Modeler not initialized');
            return;
        }

        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.json,.bpmn,.xml';

        input.onchange = e => {
            const file = e.target.files[0];
            if (!file) {
                reject('No file selected');
                return;
            }

            const reader = new FileReader();
            reader.onload = ev => {
                try {
                    const content = ev.target.result;
                    let xml = content;

                    if (file.name.endsWith('.json')) {
                        const data = JSON.parse(content);
                        xml = data.xml;
                    }

                    bpmnModeler.importXML(xml)
                        .then(() => {
                            fitViewport();
                            if (dotnetHelper) {
                                dotnetHelper.invokeMethodAsync('OnDiagramLoaded');
                            }
                            resolve('Load successful');
                        })
                        .catch(err => reject('Load failed: ' + err.message));
                } catch (err) {
                    reject('File format error: ' + err.message);
                }
            };
            reader.onerror = () => reject('File reading error');
            reader.readAsText(file);
        };

        input.click();
    });
}

// 编辑操作
export function undo() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }
    bpmnModeler.get('commandStack').undo();
}

export function redo() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }
    bpmnModeler.get('commandStack').redo();
}

// 视图控制
export function zoomIn() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }
    const canvas = bpmnModeler.get('canvas');
    const viewbox = canvas.viewbox();
    canvas.zoom(viewbox.scale + 0.1);
}

export function zoomOut() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }
    const canvas = bpmnModeler.get('canvas');
    const viewbox = canvas.viewbox();
    canvas.zoom(Math.max(0.2, viewbox.scale - 0.1));
}

export function resetZoom() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }
    const canvas = bpmnModeler.get('canvas');
    canvas.zoom('fit-viewport');
}

export function fitViewport() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }
    const canvas = bpmnModeler.get('canvas');
    canvas.zoom('fit-viewport', 'auto');
}

// 属性更新
export function updateProp(key, value) {
    if (!bpmnModeler || !currentElement) {
        console.error('BPMN Modeler not initialized or no element selected');
        return;
    }

    const modeling = bpmnModeler.get('modeling');
    modeling.updateProperties(currentElement, { [key]: value });
}

export function updateDocumentation(text) {
    if (!bpmnModeler || !currentElement) {
        console.error('BPMN Modeler not initialized or no element selected');
        return;
    }

    const modeling = bpmnModeler.get('modeling');
    const moddle = bpmnModeler.get('moddle');

    const documentation = moddle.create('bpmn:Documentation', {
        text: text
    });

    modeling.updateProperties(currentElement, {
        documentation: [documentation]
    });
}

export function updateServiceClass(className) {
    if (!bpmnModeler || !currentElement) {
        console.error('BPMN Modeler not initialized or no element selected');
        return;
    }

    const modeling = bpmnModeler.get('modeling');
    const moddle = bpmnModeler.get('moddle');
    const bo = currentElement.businessObject;

    let extensionElements = bo.extensionElements;
    if (!extensionElements) {
        extensionElements = moddle.create('bpmn:ExtensionElements');
    }

    let classElement = extensionElements.values?.find(v => v.$type === 'zhg:class');
    if (!classElement) {
        classElement = moddle.create('zhg:class');
        extensionElements.values = extensionElements.values || [];
        extensionElements.values.push(classElement);
    }

    classElement.$body = className;

    modeling.updateProperties(currentElement, {
        extensionElements: extensionElements
    });
}

// 参数管理
export function addInputParam() {
    if (!bpmnModeler || !currentElement) {
        console.error('Cannot add input parameter: BPMN Modeler not initialized or no element selected');
        return;
    }

    try {
        const moddle = bpmnModeler.get('moddle');
        const modeling = bpmnModeler.get('modeling');
        const { extensionElements, inputOutput } = getInputOutputElement();

        const param = moddle.create('zhg:inputParameter', {
            name: `param_${Date.now()}`,
            type: 'string',
            $body: ''
        });

        inputOutput.inputParameters.push(param);

        modeling.updateProperties(currentElement, {
            extensionElements: extensionElements
        });

        console.log('Input parameter added successfully');

        // 通知 Blazor 参数已更新
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnParametersUpdated')
                .catch(err => console.error('Failed to invoke OnParametersUpdated:', err));
        }
    } catch (error) {
        console.error('Error adding input parameter:', error);
    }
}

export function addOutputParam() {
    if (!bpmnModeler || !currentElement) {
        console.error('Cannot add output parameter: BPMN Modeler not initialized or no element selected');
        return;
    }

    try {
        const moddle = bpmnModeler.get('moddle');
        const modeling = bpmnModeler.get('modeling');
        const { extensionElements, inputOutput } = getInputOutputElement();

        const param = moddle.create('zhg:outputParameter', {
            name: `param_${Date.now()}`,
            type: 'string',
            $body: ''
        });

        inputOutput.outputParameters.push(param);

        modeling.updateProperties(currentElement, {
            extensionElements: extensionElements
        });

        console.log('Output parameter added successfully');

        // 通知 Blazor 参数已更新
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnParametersUpdated')
                .catch(err => console.error('Failed to invoke OnParametersUpdated:', err));
        }
    } catch (error) {
        console.error('Error adding output parameter:', error);
    }
}

// 添加获取参数的方法
export function getInputParameters() {
    if (!bpmnModeler || !currentElement) return [];

    try {
        const { inputOutput } = getInputOutputElement();
        return inputOutput.inputParameters || [];
    } catch (error) {
        console.error('Error getting input parameters:', error);
        return [];
    }
}

export function getOutputParameters() {
    if (!bpmnModeler || !currentElement) return [];

    try {
        const { inputOutput } = getInputOutputElement();
        return inputOutput.outputParameters || [];
    } catch (error) {
        console.error('Error getting output parameters:', error);
        return [];
    }
}

// 辅助函数
function getInputOutputElement() {
    const moddle = bpmnModeler.get('moddle');
    const bo = currentElement.businessObject;

    let extensionElements = bo.extensionElements;
    if (!extensionElements) {
        extensionElements = moddle.create('bpmn:ExtensionElements');
    }

    let inputOutput = extensionElements.values?.find(v => v.$type === 'zhg:inputOutput');
    if (!inputOutput) {
        inputOutput = moddle.create('zhg:inputOutput');
        extensionElements.values = extensionElements.values || [];
        extensionElements.values.push(inputOutput);
    }

    inputOutput.inputParameters = inputOutput.inputParameters || [];
    inputOutput.outputParameters = inputOutput.outputParameters || [];

    return { extensionElements, inputOutput };
}

// 清理函数
export function disposeBpmnModeler() {
    if (bpmnModeler) {
        bpmnModeler.destroy();
        bpmnModeler = null;
        currentElement = null;
    }

    if (dotnetHelper) {
        dotnetHelper = null;
    }

    // 清理全局函数
    if (window.createBpmnElement) {
        delete window.createBpmnElement;
    }

    console.log('BPMN Modeler disposed');
}

// 设置当前选中元素（由 Blazor 调用）
export function setCurrentElement(elementId) {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return;
    }

    const elementRegistry = bpmnModeler.get('elementRegistry');
    currentElement = elementRegistry.get(elementId);

    if (currentElement) {
        console.log(`Current element set to: ${elementId}`);
    } else {
        console.warn(`Element not found: ${elementId}`);
    }
}

// 获取元素数量
export function getElementCount() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return 0;
    }

    const elementRegistry = bpmnModeler.get('elementRegistry');
    const elements = elementRegistry.getAll();
    const count = elements.filter(e => e.type !== 'bpmn:Process' && e.type !== 'label').length;

    return count;
}

// 条件构建器
export function openConditionBuilder() {
    const builder = document.getElementById('condition-builder');
    if (builder) {
        const isVisible = builder.style.display !== 'none';
        builder.style.display = isVisible ? 'none' : 'block';
        console.log(`Condition builder ${isVisible ? 'closed' : 'opened'}`);
    } else {
        console.warn('Condition builder element not found');
    }
}

// 获取当前缩放级别
export function getCurrentZoom() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return 1.0;
    }

    const canvas = bpmnModeler.get('canvas');
    const viewbox = canvas.viewbox();
    return viewbox.scale;
}

// 获取所有任务元素
export function getAllTasks() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return [];
    }

    const elementRegistry = bpmnModeler.get('elementRegistry');
    const elements = elementRegistry.getAll();

    const tasks = elements.filter(el => {
        return el.type && (
            el.type.includes('Task') ||
            el.type === 'bpmn:SubProcess' ||
            el.type === 'bpmn:CallActivity'
        );
    }).map(el => {
        const bo = el.businessObject;
        return {
            id: bo.id,
            name: bo.name || bo.id,
            type: el.type
        };
    });

    return tasks;
}

// 删除输入参数
export function removeInputParameter(index) {
    if (!bpmnModeler || !currentElement) {
        console.error('Cannot remove input parameter: BPMN Modeler not initialized or no element selected');
        return false;
    }

    try {
        const { extensionElements, inputOutput } = getInputOutputElement();

        if (!inputOutput.inputParameters || index < 0 || index >= inputOutput.inputParameters.length) {
            console.error(`Invalid input parameter index: ${index}`);
            return false;
        }

        // 删除指定索引的参数
        inputOutput.inputParameters.splice(index, 1);

        const modeling = bpmnModeler.get('modeling');
        modeling.updateProperties(currentElement, {
            extensionElements: extensionElements
        });

        console.log(`Input parameter at index ${index} removed successfully`);

        // 通知 Blazor 参数已更新
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnParametersUpdated')
                .catch(err => console.error('Failed to invoke OnParametersUpdated:', err));
        }

        return true;
    } catch (error) {
        console.error('Error removing input parameter:', error);
        return false;
    }
}

// 删除输出参数
export function removeOutputParameter(index) {
    if (!bpmnModeler || !currentElement) {
        console.error('Cannot remove output parameter: BPMN Modeler not initialized or no element selected');
        return false;
    }

    try {
        const { extensionElements, inputOutput } = getInputOutputElement();

        if (!inputOutput.outputParameters || index < 0 || index >= inputOutput.outputParameters.length) {
            console.error(`Invalid output parameter index: ${index}`);
            return false;
        }

        // 删除指定索引的参数
        inputOutput.outputParameters.splice(index, 1);

        const modeling = bpmnModeler.get('modeling');
        modeling.updateProperties(currentElement, {
            extensionElements: extensionElements
        });

        console.log(`Output parameter at index ${index} removed successfully`);

        // 通知 Blazor 参数已更新
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnParametersUpdated')
                .catch(err => console.error('Failed to invoke OnParametersUpdated:', err));
        }

        return true;
    } catch (error) {
        console.error('Error removing output parameter:', error);
        return false;
    }
}

// 更新输入参数
export function updateInputParameter(index, name, type, value) {
    if (!bpmnModeler || !currentElement) {
        console.error('Cannot update input parameter: BPMN Modeler not initialized or no element selected');
        return false;
    }

    try {
        const { extensionElements, inputOutput } = getInputOutputElement();

        if (!inputOutput.inputParameters || index < 0 || index >= inputOutput.inputParameters.length) {
            console.error(`Invalid input parameter index: ${index}`);
            return false;
        }

        // 更新参数
        const param = inputOutput.inputParameters[index];
        param.name = name;
        param.type = type;
        param.$body = value;

        const modeling = bpmnModeler.get('modeling');
        modeling.updateProperties(currentElement, {
            extensionElements: extensionElements
        });

        console.log(`Input parameter at index ${index} updated successfully: ${name}, ${type}, ${value}`);

        // 通知 Blazor 参数已更新
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnParametersUpdated')
                .catch(err => console.error('Failed to invoke OnParametersUpdated:', err));
        }

        return true;
    } catch (error) {
        console.error('Error updating input parameter:', error);
        return false;
    }
}

// 更新输出参数
export function updateOutputParameter(index, name, type, value) {
    if (!bpmnModeler || !currentElement) {
        console.error('Cannot update output parameter: BPMN Modeler not initialized or no element selected');
        return false;
    }

    try {
        const { extensionElements, inputOutput } = getInputOutputElement();

        if (!inputOutput.outputParameters || index < 0 || index >= inputOutput.outputParameters.length) {
            console.error(`Invalid output parameter index: ${index}`);
            return false;
        }

        // 更新参数
        const param = inputOutput.outputParameters[index];
        param.name = name;
        param.type = type;
        param.$body = value;

        const modeling = bpmnModeler.get('modeling');
        modeling.updateProperties(currentElement, {
            extensionElements: extensionElements
        });

        console.log(`Output parameter at index ${index} updated successfully: ${name}, ${type}, ${value}`);

        // 通知 Blazor 参数已更新
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnParametersUpdated')
                .catch(err => console.error('Failed to invoke OnParametersUpdated:', err));
        }

        return true;
    } catch (error) {
        console.error('Error updating output parameter:', error);
        return false;
    }
}

// 获取参数索引的方法
export function getInputParameterIndexByName(name) {
    if (!bpmnModeler || !currentElement) return -1;

    try {
        const { inputOutput } = getInputOutputElement();
        if (!inputOutput.inputParameters) return -1;

        return inputOutput.inputParameters.findIndex(p => p.name === name);
    } catch (error) {
        console.error('Error getting input parameter index:', error);
        return -1;
    }
}

export function getOutputParameterIndexByName(name) {
    if (!bpmnModeler || !currentElement) return -1;

    try {
        const { inputOutput } = getInputOutputElement();
        if (!inputOutput.outputParameters) return -1;

        return inputOutput.outputParameters.findIndex(p => p.name === name);
    } catch (error) {
        console.error('Error getting output parameter index:', error);
        return -1;
    }
}

// 默认导出
export default {
    initBpmnModeler,
    exportBpmn,
    importBpmn,
    saveBpmn,
    loadBpmn,
    undo,
    redo,
    zoomIn,
    zoomOut,
    resetZoom,
    fitViewport,
    updateProp,
    updateDocumentation,
    updateServiceClass,
    addInputParam,
    addOutputParam,
    disposeBpmnModeler,
    setCurrentElement,
    getElementCount,
    getCurrentZoom,
    getAllTasks,
    openConditionBuilder
};