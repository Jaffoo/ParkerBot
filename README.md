# ParkerBot(帕克机器人)

## 介绍
监听微博，口袋等偶像社交软件，通过人脸识别及对比，自动保存偶像图片至本地或阿里云盘，解放双手，集成消息通知等功能。

## 软件架构
基于.NET 6 winform框架开源UI库[NanUI](https://gitee.com/dotnetchina/NanUI)开发。前端页面使用的是Vue3，Ts，ElementUI。

## 安装教程

1.  进入[发行版](https://gitee.com/jaffoo/ParkerBotV2/releases)下载最新版本。
2.  解压压缩包。
3.  找到ParkerBot.exe，运行即可。

## 配置教程

1.  QQ
- QQ群，后续配置的消息通知默认发送至此处配置的群，可多个，用英文逗号分开。
- 超级管理员，QQ机器人的最高管理员。（向机器人发送#菜单）
- 管理员，顾名思义。（向机器人发送#菜单）
- 启用功能，暂不可用，后续看情况添加娱乐功能，比如ai聊天等。
- 功能分类，将启用的功能分配给置顶角色。
- 敏感词，和下面敏感词操作配套使用。
2.  微博
- 用户ID，需要关注的用户ID，获取方式如图：
![输入图片说明](images/image.png)    
    得到这个链接地址https://weibo.com/u/689280541，其中689280541这个数字就是用户id。
- 吃瓜用户ID，和上面一样。
- 吃瓜微博过滤，填入关键词，当微博中含有此关键词时，会通过QQ发送消息通知。
- 监听间隔，隔过少时间监听一次，默认3分钟。注：要精确到秒自己换算，如半分钟则是0.5分钟，1-3分钟最合适，监听时间过短，可能会导致被限制IP，严重则封IP，从而导致监听失败。
- 转发至qq群，开启后，第一个配置的用户发送了新微博，会通过qq发送消息通知。可单独配置需要通知群，不配置则默认第一项QQ配置中的qq群。注：当监听了多个用户时，只有第一个用户的微博消息会通过qq通知，所以第一个应当配置为需要通过QQ通知的微博用户。
- 转发至qq，和上述群大同小异。
3. B站
- 用户ID，需要关注的用户ID，获取方式如图：![输入图片说明](images/Blibiliimage.png)    
得到连接https://space.bilibili.com/2832224?share_medium=android&share_source=copy_link&bbid=XU94CF99666A8BB964A01C7379DC4B2AC3F95&ts=1689298684776，找到com/后面的数字2832224就是用户ID。
- 监听间隔，和微博一样，不做赘述。
- 转发至qq群和转发至qq好友和微博一样，注意一点，微博是多用户，b站是单用户。
4. 口袋48
- 姓名，小偶像的姓名，如SNH-xxx.
- IMServerId和直播房间Id，打开[小偶像口袋信息](https://fastly.jsdelivr.net/gh/duan602728596/qqtools@main/packages/NIMTest/node/roomId.json)，找到对应小偶像的serverId和liveRoomId后面的数字填入即可。
- IM账号和IMToken，填入自己的账号，不知道的点击【登录口袋48按钮】输入手机号码验证码，可以自动获取完成。
- 转发至qq群和转发至qq好友，可将小偶像口袋房间和直播间发送的消息转发至指定的qq群或好友。
5. 小红书（暂未上线）
6. 抖音（暂未上线）
7. 百度(用于人脸识别)
- appKey和appSeret获取方式，打开[百度云](https://cloud.baidu.com/)右上角登陆，然后进行实名认证，选个人认证，完成后打开[领取免费资源](https://console.bce.baidu.com/ai/#/ai/face/overview/resource/getFree)，按图操作。
![输入图片说明](images/bdimage.png)    领取成功后点击前往应用列表。点击创建应用，勾选红框中的内容或者按照默认全选也行，然后按照要求填入信息后点击立即创建，首页就可以看到你刚刚创建的应用，把相应的apiKey和SecretKey填入配置相应位置即可。
![输入图片说明](images/bd1image.png)
- 开启人脸验证，顾名思义。
- 基础人脸，上传3张人脸照片，尽量找五官清晰的近期照片。
- 人脸相似度，当人脸相似度大于该值，则直接保存。
- 审核相似度，当人脸相似度大于该值，小于【人脸相似度】时，加入审核列表。可在首页进行审核，如果使用qq机器人，发送#菜单 查看审核功能的使用。
- 上传云盘，目前只支持阿里云盘，后续看需添加其他云盘。
- 相册名称，上传到指定的相册，如果相册不存在，则会创建相册。注：由于阿里云盘上传机制的问题，在文件列表中，也会出现和相册一模一样的文件夹，如果后续占用过大，此文件夹内容可以删除，但是每次新上传图片，都会先上传到此文件夹。
## 参与贡献

1.  Fork 本仓库
2.  新建 Feat_xxx 分支
3.  提交代码
4.  新建 Pull Request


## 特技

1.  使用 Readme\_XXX.md 来支持不同的语言，例如 Readme\_en.md, Readme\_zh.md
2.  Gitee 官方博客 [blog.gitee.com](https://blog.gitee.com)
3.  你可以 [https://gitee.com/explore](https://gitee.com/explore) 这个地址来了解 Gitee 上的优秀开源项目
4.  [GVP](https://gitee.com/gvp) 全称是 Gitee 最有价值开源项目，是综合评定出的优秀开源项目
5.  Gitee 官方提供的使用手册 [https://gitee.com/help](https://gitee.com/help)
6.  Gitee 封面人物是一档用来展示 Gitee 会员风采的栏目 [https://gitee.com/gitee-stars/](https://gitee.com/gitee-stars/)
