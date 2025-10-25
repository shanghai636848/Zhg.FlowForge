// wwwroot/js/fileSystem.js

window.promptDirectory = async (message) => {
    try {
        // 使用 File System Access API (需要用户授权)
        if ('showDirectoryPicker' in window) {
            const dirHandle = await window.showDirectoryPicker();
            return dirHandle.name;
        } else {
            // 降级到 input[type=file]
            return await fallbackDirectoryPicker(message);
        }
    } catch (error) {
        console.error('选择目录失败:', error);
        return null;
    }
};

async function fallbackDirectoryPicker(message) {
    return new Promise((resolve) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.webkitdirectory = true;
        input.directory = true;

        input.onchange = (e) => {
            const files = Array.from(e.target.files);
            if (files.length > 0) {
                const path = files[0].webkitRelativePath.split('/')[0];
                resolve(path);
            } else {
                resolve(null);
            }
        };

        input.click();
    });
}

// 打开文件管理器到指定目录
window.openInExplorer = (path) => {
    try {
        // 在 Electron 或桌面应用中可以实现
        console.log('打开文件管理器:', path);

        // Web 环境下的替代方案：复制路径到剪贴板
        navigator.clipboard.writeText(path).then(() => {
            alert(`路径已复制到剪贴板:\n${path}\n\n请手动打开文件管理器并粘贴路径`);
        });
    } catch (error) {
        console.error('打开文件管理器失败:', error);
        alert(项目路径: \n${ path });
    }
};
// 检查文件系统访问权限
window.checkFileSystemPermission = async () => {
    try {
        if ('showDirectoryPicker' in window) {
            return {
                supported: true,
                message: '浏览器支持 File System Access API'
            };
        } else {
            return {
                supported: false,
                message: '浏览器不支持 File System Access API，将使用备用方案'
            };
        }
    } catch (error) {
        return {
            supported: false,
            message: error.message
        };
    }
};
// 下载项目为 ZIP 文件（备用方案）
window.downloadProjectAsZip = async (projectName, files) => {
    try {
        // 使用 JSZip 库创建 ZIP
        const zip = new JSZip();
        for (const file of files) {
            zip.file(file.path, file.content);
        }

        const blob = await zip.generateAsync({ type: 'blob' });
        const url = URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = `${projectName}.zip`;
        a.click();

        URL.revokeObjectURL(url);

        return true;
    } catch (error) {
        console.error('下载项目失败:', error);
        return false;
    }
};