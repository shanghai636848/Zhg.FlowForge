window.downloadFile = function (filename, content) {
    const element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(content));
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
};

window.scrollToBottom = function (selector) {
    const element = document.querySelector(selector);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.openFolder = function (path) {
    // 在支持的平台上打开文件夹
    if (window.showDirectoryPicker) {
        window.showDirectoryPicker().then(() => {
            console.log('Folder opened:', path);
        });
    }
};