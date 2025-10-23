// bpmn-interop.js - 修复版
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
                    name: 'InputOutput',
                    superClass: ['Element'],
                    properties: [
                        { name: 'inputParameters', type: 'zhg:InputParameter', isMany: true },
                        { name: 'outputParameters', type: 'zhg:OutputParameter', isMany: true }
                    ]
                },
                {
                    name: 'InputParameter',
                    superClass: ['Element'],
                    properties: [
                        { name: 'name', type: 'String', isAttr: true },
                        { name: 'type', type: 'String', isAttr: true },
                        { name: 'value', type: 'String' }
                    ]
                },
                {
                    name: 'OutputParameter',
                    superClass: ['Element'],
                    properties: [
                        { name: 'name', type: 'String', isAttr: true },
                        { name: 'type', type: 'String', isAttr: true },
                        { name: 'value', type: 'String' }
                    ]
                },
                {
                    name: 'Class',
                    superClass: ['Element'],
                    properties: [
                        { name: 'value', type: 'String' }
                    ]
                }
            ]
        };

        bpmnModeler = new BpmnJS({
            container: '#' + containerId,
            keyboard: { bindTo: window },
            moddleExtensions: {
                zhg: customModdle
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

function setupEventListeners() {
    if (!bpmnModeler) return;

    bpmnModeler.on('selection.changed', (event) => {
        if (event.newSelection && event.newSelection.length === 1) {
            const element = event.newSelection[0];
            currentElement = element;
            const bo = element.businessObject;

            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnElementSelected',
                    element.id,
                    element.type,
                    bo.name || ''
                ).catch(err => console.error('Failed to invoke OnElementSelected:', err));
            }
        } else {
            currentElement = null;
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnElementDeselected')
                    .catch(err => console.error('Failed to invoke OnElementDeselected:', err));
            }
        }
    });

    bpmnModeler.on('element.changed', (event) => {
        if (dotnetHelper && event.element) {
            dotnetHelper.invokeMethodAsync('OnElementChanged', event.element.id, event.element.type)
                .catch(err => console.error('Failed to invoke OnElementChanged:', err));
        }
    });

    bpmnModeler.on('canvas.viewbox.changed', (event) => {
        if (dotnetHelper && event.viewbox) {
            dotnetHelper.invokeMethodAsync('OnZoomChanged', event.viewbox.scale)
                .catch(err => console.error('Failed to invoke OnZoomChanged:', err));
        }
    });
}

// 获取或创建扩展元素
function getOrCreateExtensionElements(element) {
    const moddle = bpmnModeler.get('moddle');
    const bo = element.businessObject;

    let extensionElements = bo.extensionElements;
    if (!extensionElements) {
        extensionElements = moddle.create('bpmn:ExtensionElements');
        bo.extensionElements = extensionElements;
    }

    if (!extensionElements.values) {
        extensionElements.values = [];
    }

    return extensionElements;
}

// 获取或创建 InputOutput 元素
function getOrCreateInputOutput(element) {
    const moddle = bpmnModeler.get('moddle');
    const extensionElements = getOrCreateExtensionElements(element);

    let inputOutput = extensionElements.values.find(v => v.$type === 'zhg:InputOutput');
    if (!inputOutput) {
        inputOutput = moddle.create('zhg:InputOutput');
        inputOutput.inputParameters = [];
        inputOutput.outputParameters = [];
        extensionElements.values.push(inputOutput);
    }

    if (!inputOutput.inputParameters) inputOutput.inputParameters = [];
    if (!inputOutput.outputParameters) inputOutput.outputParameters = [];

    return inputOutput;
}

// 添加输入参数
export function addInputParameter(elementId) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');
        const moddle = bpmnModeler.get('moddle');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const inputOutput = getOrCreateInputOutput(element);

        const param = moddle.create('zhg:InputParameter', {
            name: `param_${Date.now()}`,
            type: 'string',
            value: ''
        });

        inputOutput.inputParameters.push(param);

        modeling.updateProperties(element, {
            extensionElements: element.businessObject.extensionElements
        });

        console.log('Input parameter added successfully');
    } catch (error) {
        console.error('Error adding input parameter:', error);
    }
}

// 添加输出参数
export function addOutputParameter(elementId) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');
        const moddle = bpmnModeler.get('moddle');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const inputOutput = getOrCreateInputOutput(element);

        const param = moddle.create('zhg:OutputParameter', {
            name: `param_${Date.now()}`,
            type: 'string',
            value: ''
        });

        inputOutput.outputParameters.push(param);

        modeling.updateProperties(element, {
            extensionElements: element.businessObject.extensionElements
        });

        console.log('Output parameter added successfully');
    } catch (error) {
        console.error('Error adding output parameter:', error);
    }
}

// 获取输入参数
export function getInputParameters(elementId) {
    if (!bpmnModeler) return [];

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const element = elementRegistry.get(elementId);
        if (!element) return [];

        const extensionElements = element.businessObject.extensionElements;
        if (!extensionElements) return [];

        const inputOutput = extensionElements.values?.find(v => v.$type === 'zhg:InputOutput');
        if (!inputOutput || !inputOutput.inputParameters) return [];

        return inputOutput.inputParameters.map(p => ({
            name: p.name || '',
            type: p.type || 'string',
            value: p.value || ''
        }));
    } catch (error) {
        console.error('Error getting input parameters:', error);
        return [];
    }
}

// 获取输出参数
export function getOutputParameters(elementId) {
    if (!bpmnModeler) return [];

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const element = elementRegistry.get(elementId);
        if (!element) return [];

        const extensionElements = element.businessObject.extensionElements;
        if (!extensionElements) return [];

        const inputOutput = extensionElements.values?.find(v => v.$type === 'zhg:InputOutput');
        if (!inputOutput || !inputOutput.outputParameters) return [];

        return inputOutput.outputParameters.map(p => ({
            name: p.name || '',
            type: p.type || 'string',
            value: p.value || ''
        }));
    } catch (error) {
        console.error('Error getting output parameters:', error);
        return [];
    }
}

// 更新输入参数
export function updateInputParameter(elementId, index, name, type, value) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const inputOutput = getOrCreateInputOutput(element);

        if (index >= 0 && index < inputOutput.inputParameters.length) {
            const param = inputOutput.inputParameters[index];
            param.name = name;
            param.type = type;
            param.value = value;

            modeling.updateProperties(element, {
                extensionElements: element.businessObject.extensionElements
            });

            console.log('Input parameter updated successfully');
        }
    } catch (error) {
        console.error('Error updating input parameter:', error);
    }
}

// 更新输出参数
export function updateOutputParameter(elementId, index, name, type, value) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const inputOutput = getOrCreateInputOutput(element);

        if (index >= 0 && index < inputOutput.outputParameters.length) {
            const param = inputOutput.outputParameters[index];
            param.name = name;
            param.type = type;
            param.value = value;

            modeling.updateProperties(element, {
                extensionElements: element.businessObject.extensionElements
            });

            console.log('Output parameter updated successfully');
        }
    } catch (error) {
        console.error('Error updating output parameter:', error);
    }
}

// 删除输入参数
export function removeInputParameter(elementId, index) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const inputOutput = getOrCreateInputOutput(element);

        if (index >= 0 && index < inputOutput.inputParameters.length) {
            inputOutput.inputParameters.splice(index, 1);

            modeling.updateProperties(element, {
                extensionElements: element.businessObject.extensionElements
            });

            console.log('Input parameter removed successfully');
        }
    } catch (error) {
        console.error('Error removing input parameter:', error);
    }
}

// 删除输出参数
export function removeOutputParameter(elementId, index) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const inputOutput = getOrCreateInputOutput(element);

        if (index >= 0 && index < inputOutput.outputParameters.length) {
            inputOutput.outputParameters.splice(index, 1);

            modeling.updateProperties(element, {
                extensionElements: element.businessObject.extensionElements
            });

            console.log('Output parameter removed successfully');
        }
    } catch (error) {
        console.error('Error removing output parameter:', error);
    }
}

// 更新属性
export function updateProperty(key, value) {
    if (!bpmnModeler || !currentElement) return;

    try {
        const modeling = bpmnModeler.get('modeling');
        modeling.updateProperties(currentElement, { [key]: value });
        console.log(`Property ${key} updated to ${value}`);
    } catch (error) {
        console.error('Error updating property:', error);
    }
}

// 更新文档说明
export function updateDocumentation(elementId, text) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');
        const moddle = bpmnModeler.get('moddle');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const documentation = moddle.create('bpmn:Documentation', { text: text });

        modeling.updateProperties(element, {
            documentation: [documentation]
        });

        console.log('Documentation updated successfully');
    } catch (error) {
        console.error('Error updating documentation:', error);
    }
}

// 更新服务类
export function updateServiceClass(elementId, className) {
    if (!bpmnModeler) return;

    try {
        const elementRegistry = bpmnModeler.get('elementRegistry');
        const modeling = bpmnModeler.get('modeling');
        const moddle = bpmnModeler.get('moddle');

        const element = elementRegistry.get(elementId);
        if (!element) return;

        const extensionElements = getOrCreateExtensionElements(element);

        let classElement = extensionElements.values.find(v => v.$type === 'zhg:Class');
        if (!classElement) {
            classElement = moddle.create('zhg:Class');
            extensionElements.values.push(classElement);
        }

        classElement.value = className;

        modeling.updateProperties(element, {
            extensionElements: element.businessObject.extensionElements
        });

        console.log('Service class updated successfully');
    } catch (error) {
        console.error('Error updating service class:', error);
    }
}

// 导出 BPMN
export function exportBpmn() {
    if (!bpmnModeler) {
        console.error('BPMN Modeler not initialized');
        return Promise.reject('BPMN Modeler not initialized');
    }

    return bpmnModeler.saveXML({ format: true })
        .then(result => {
            const xml = result.xml;
            const blob = new Blob([xml], { type: 'application/bpmn+xml;charset=utf-8' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `bpmn-diagram-${Date.now()}.bpmn`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);

            console.log('BPMN exported successfully');
            return xml;
        })
        .catch(error => {
            console.error('Export failed:', error);
            return Promise.reject(error);
        });
}

// 导入 BPMN
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

// 撤销/重做
export function undo() {
    if (!bpmnModeler) return;
    bpmnModeler.get('commandStack').undo();
}

export function redo() {
    if (!bpmnModeler) return;
    bpmnModeler.get('commandStack').redo();
}

// 缩放控制
export function zoomIn() {
    if (!bpmnModeler) return;
    const canvas = bpmnModeler.get('canvas');
    const viewbox = canvas.viewbox();
    canvas.zoom(viewbox.scale + 0.1);
}

export function zoomOut() {
    if (!bpmnModeler) return;
    const canvas = bpmnModeler.get('canvas');
    const viewbox = canvas.viewbox();
    canvas.zoom(Math.max(0.2, viewbox.scale - 0.1));
}

export function resetZoom() {
    if (!bpmnModeler) return;
    const canvas = bpmnModeler.get('canvas');
    canvas.zoom('fit-viewport');
}

export function fitViewport() {
    if (!bpmnModeler) return;
    const canvas = bpmnModeler.get('canvas');
    canvas.zoom('fit-viewport', 'auto');
}

// 获取元素数量
export function getElementCount() {
    if (!bpmnModeler) return 0;
    const elementRegistry = bpmnModeler.get('elementRegistry');
    const elements = elementRegistry.getAll();
    return elements.filter(e => e.type !== 'bpmn:Process' && e.type !== 'label').length;
}

// 清理
export function disposeBpmnModeler() {
    if (bpmnModeler) {
        bpmnModeler.destroy();
        bpmnModeler = null;
        currentElement = null;
    }
    if (dotnetHelper) {
        dotnetHelper = null;
    }
    if (window.createBpmnElement) {
        delete window.createBpmnElement;
    }
    console.log('BPMN Modeler disposed');
}

export default {
    initBpmnModeler,
    exportBpmn,
    importBpmn,
    undo,
    redo,
    zoomIn,
    zoomOut,
    resetZoom,
    fitViewport,
    updateProperty,
    updateDocumentation,
    updateServiceClass,
    addInputParameter,
    addOutputParameter,
    getInputParameters,
    getOutputParameters,
    updateInputParameter,
    updateOutputParameter,
    removeInputParameter,
    removeOutputParameter,
    disposeBpmnModeler,
    getElementCount
};