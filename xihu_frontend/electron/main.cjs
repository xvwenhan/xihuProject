const { app, BrowserWindow } = require('electron')
const path = require('path')

function createWindow() {
  const win = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      webSecurity: false,
    }

  })

  // 设置 CSP 头
  win.webContents.session.webRequest.onHeadersReceived((details, callback) => {
    callback({
      responseHeaders: {
        ...details.responseHeaders,
        'Content-Security-Policy': [
          "default-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:* https://api.dicebear.com https://ditu.amap.com;",
          "img-src 'self' data: http://localhost:* https://api.dicebear.com https://ditu.amap.com/;",
          "connect-src 'self' http://localhost:* https://api.dicebear.com https://ditu.amap.com/;"
        ]
      }
    })
  })
  // win.webContents.session.webRequest.onHeadersReceived((details, callback) => {
  //   callback({
  //     responseHeaders: {
  //       ...details.responseHeaders,
  //       'Content-Security-Policy': [
  //         "default-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:* https://api.dicebear.com;",
  //         "img-src 'self' data: http://localhost:* https://api.dicebear.com;",
  //         "connect-src 'self' http://localhost:* https://api.dicebear.com ",
  //         "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.staticfile.net;",  // 允许加载外部脚本
  //         "script-src-elem 'self' 'unsafe-inline' https://cdn.staticfile.net;" // 允许通过 script 元素加载外部脚本
  //       ]
  //     }
  //   })
  // })

  // 在开发环境中加载 Vite 开发服务器
  if (process.env.NODE_ENV === 'development') {
    win.loadURL('http://localhost:5173')
    win.webContents.openDevTools()
  } else {
    // 在生产环境中加载打包后的文件
    win.loadFile(path.join(__dirname, '../dist/index.html'))
  }
}

app.whenReady().then(() => {
  createWindow()

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow()
    }
  })
})

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit()
  }
})
