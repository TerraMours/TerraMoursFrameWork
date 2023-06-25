<div align="center">
	<h1>TerraMours Gpt Api</h1>
</div>

![](https://img.shields.io/github/stars/TerraMours/TerraMours_Gpt_Api) ![](https://img.shields.io/github/forks/TerraMours/TerraMours_Gpt_Api)

中文简介 | [English](README-EN.md)

## 简介

[TerraMours Gpt Api](https://github.com/TerraMours/TerraMours_Gpt_Api) 是基于[TerraMoursFrameWork](https://github.com/TerraMours/TerraMoursFrameWork) 开发完善的智能助手项目。技术包括 Net7+MinimalApi+EF Core+Postgresql+Seq+FluentApi ...... 

核心功能：

1.对https://ai.terramours.site/基于Semantic Kernel的重构，实现多AI模型的聊天

2.Stable Diffusion和chatgpt的dallE模型的图片生成

## 特性

##### 1.SignalR+Hangfire 实现后台任务队列和实时通讯

##### 2.automapper 模型自动映射

##### 3.对接口统一返回结果中间件ApiResponse{code,message,data}封装

##### 4.Semantic Kernel 调用chatgpt

##### 5.日志服务Seq

##### 6.Stable Diffusion 图片生成

## 开发功能
- **AI聊天**：发起聊天，基于Semantic Kernel，目前写了chatgpt，常用模型：gpt-3.5-turbo，可支持gpt-3.5-turbo-16K,gpt-4
- **AI绘图**：基于Stable Diffusion和chatgpt的dallE模型的图片生成
- **聊天记录**：聊天记录管理，查询使用者会话信息。（todo：创建微调模型）
- **敏感词管理**: 敏感词管理，自定义敏感词过滤，加强系统安全
- **Key池管理**：Key池管理，支持管理者添加多个key组成Key池，调用ai接口时轮询，加强稳定性
- **系统提示词**：系统提示词，添加各种角色提示词，让使用者能更好的使用ai对话。


## 在线预览

- [TerraMours Admin 预览地址](https://demo.terramours.site/)

## 文档

- [项目文档地址](https://terramours.site/)



## 前端服务

- [TerraMours_Gpt_Web](https://github.com/TerraMours/TerraMours_Gpt_Web)


## 安装使用

- 环境配置
  **.net7 SDK**

- 克隆代码

```bash
git clone https://github.com/TerraMours/TerraMours_Admin_Web.git
```



## Docker 部署

* 构建镜像

```bash
docker build -t terramours_gpt_server .
```

* 创建挂载目录（目录自定义）

```bash
mkdir /data/terramoursgpt/server/images
```

- Docker 部署 terramoursweb

```bash
docker run --name terramours_gpt_server -v /data/terramoursgpt/server/images:/app/images  -p 3115:80  -d terramours_gpt_server
```

- 访问 swagger

打开本地浏览器访问`http://localhost/swagger/index.html`



## 开源作者

[@Raokun](https://github.com/raokun)

[@firstsaofan](https://github.com/orgs/TerraMours/people/firstsaofan)



## 交流

`TerraMours Admin` 是完全开源免费的项目，在帮助开发者更方便地进行中大型管理系统开发，同时也提供微信和 QQ 交流群，使用问题欢迎在群内提问。

  <div style="display:flex;">
  	<div style="padding-right:24px;">
  		<p>QQ交流群</p>
      <img src="https://www.raokun.top/upload/2023/06/qq.png" style="width:200px" />
  	</div>
		<div>
			<p>添加本人微信，欢迎来技术交流，业务咨询</p>
			<img src="https://www.raokun.top/upload/2023/04/%E5%BE%AE%E4%BF%A1%E5%9B%BE%E7%89%87_20230405192146.jpg" style="width:180px" />
		</div>
  </div>

## 捐赠

如果你觉得这个项目对你有帮助，可以请 TerraMours 组员喝杯咖啡表示支持，TerraMours 开源的动力离不开各位的支持和鼓励。

  <div style="display:flex;">
  	<div style="padding-right:24px;">
  		<p>微信</p>
      <img src="https://www.raokun.top/upload/2023/04/%E5%BE%AE%E4%BF%A1%E6%94%B6%E6%AC%BE.jpg" style="width:200px" />
  	</div>
  </div>

## License

[MIT © TerraMours-2023](./LICENSE)
