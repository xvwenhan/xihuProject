# 现在后端在本地运行测试，使用coplar内网穿透，就直接用了http，builder.WebHost.UseUrls("http://0.0.0.0:5001");
# coplar内网穿透地址过一会就会变，因此测试需要更换后端appsettings.json的公网网址和前端vite.config.ts处的网址，以及微信公众平台的测试网址
# 微信公众平台的测试网址：https://mp.weixin.qq.com/debug/cgi-bin/sandboxinfo?action=showinfo&t=sandbox/index
# 二维码测试需要先关注公众号
