# grab-vaccine

#### 介绍
##### 抢约苗小程序疫苗
 共享资源，仅能用于学习，请勿用于商业。
#### 依赖框架
基于.NET 5 使用vs2019

#### 界面
##### 主页
![输入图片说明](https://images.gitee.com/uploads/images/2021/0909/192708_ccc79821_1843061.png "屏幕截图.png")
##### 抢苗
![输入图片说明](https://images.gitee.com/uploads/images/2021/0909/192808_b3b17f65_1843061.png "屏幕截图.png")
##### 查询疫苗
![输入图片说明](https://images.gitee.com/uploads/images/2021/0909/192955_e31efcbc_1843061.png "屏幕截图.png")
#### 使用说明
使用前确认已安装.net 5 sdk
1.  使用fiddler等其他抓包程序，抓取微信小程序的包(请求报文),拷贝到项目目录下reqHeader.txt文件中
![输入图片说明](https://images.gitee.com/uploads/images/2021/0909/193207_87b09aa1_1843061.png "屏幕截图.png")
2.  设置appsettings.json相关疫苗信息(通过抓包获取)
3.  设置完毕后直接运行程序即可
