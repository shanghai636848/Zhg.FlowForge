// 创建全局事件系统
window.bpmnEditorEvents = {
    listeners: {},

    subscribe(event, callback) {
        if (!this.listeners[event]) {
            this.listeners[event] = [];
        }
        this.listeners[event].push(callback);
    },

    publish(event, data) {
        if (this.listeners[event]) {
            this.listeners[event].forEach(callback => callback(data));
        }
    }
};

// 组件间通信
export function publishEvent(event, data) {
    window.bpmnEditorEvents.publish(event, data);
}

export function subscribeEvent(event, dotnetHelper, methodName) {
    window.bpmnEditorEvents.subscribe(event, (data) => {
        dotnetHelper.invokeMethodAsync(methodName, data);
    });
}