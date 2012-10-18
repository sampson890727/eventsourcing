eventsourcing
=============

A framework which implement the event sourcing pattern and CQRS architecture and is suitable for developing DDD based applications.

1. 运行源代码前先执行命令build nuget以获取外部依赖的程序集。具体步骤：按住shit键，在当前菜单鼠标右键，在弹出的菜单项中选择“在此处打开命令窗口”，然后输入：build nuget，然后回车即可。
2. 如果要调试代码，需要先新建一个空的SQL数据库，然后执行SQL脚本，脚本地址：src\Sample\EventSourcing.Sample.Model\GenerateTableScripts.sql
3. 数据库连接字符串配置文件：src\Sample\EventSourcing.Sample.Test\ConfigFiles\properties.config