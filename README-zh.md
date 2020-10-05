# STARTGUILDWARS2

## 解决的问题
* 美服国服冲突，互相覆盖配置文件 - 使用不同 Windows 帐户启动。
* 自主选择安装插件 - 使用目前美服的基座插件加载方案。

## 项目技术栈
&emsp;基于 `WPF`，使用 `MVVMLight` 框架和 `Handy Control` UI 库。

## 运行项目
* 使用 `Visual Studio 2019` 打开 `StartGuildwars2.sln`；
* 等待 `IDE` 左下角显示依赖安装完毕；
* 常规 `Debug AnyCpu` 进行调试，`Release X64` 进行发布打包；
* 确保系统已安装 `NodeJS`，使用 IDE 进行 `Release X64` 编译后，在项目根目录执行 `npm run build`；
* `./Dist/StartGuildwars2-Setup-0.0.0.0.exe` 即是安装包；

<br>
有任何疑问可以发邮件到 keenghost@163.com，欢迎交流！
