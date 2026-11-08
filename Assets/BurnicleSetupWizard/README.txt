项目初始化向导（模板自带，可整体删除）

- 打开工程时如果没有初始化过会自动弹出；也可以从菜单 Tools > 项目初始化向导 手动打开。
- 应用后会改 PlayerSettings、重命名启动场景、替换代码命名空间根，并触发重新编译。
- 完成标记写在 ProjectSettings/BurnicleProjectSetup.txt（纯文本，记录上次应用的值）；
  删掉该文件后，下次打开工程会重新弹向导。
- 不需要向导时，删除 Assets/BurnicleSetupWizard 整个文件夹即可：
  工程里没有其它代码引用它，删除后不会留下编译错误或其它残留行为。
