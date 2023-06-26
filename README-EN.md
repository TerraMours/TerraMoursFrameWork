<div align="center">
	<h1>TerraMours Admin</h1>
</div>

[![license](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE) ![](https://img.shields.io/github/stars/TerraMours/TerraMours_Admin_Web) ![](https://img.shields.io/github/forks/TerraMours/TerraMours_Admin_Web)

[中文简介](README.md) | English

## Introduction

[TerraMours Gpt Api](https://github.com/TerraMours/TerraMours_Gpt_Api) is an intelligent assistant project developed and improved based on [TerraMoursFrameWork](https://github.com/TerraMours/TerraMoursFrameWork). The technology includes Net7+MinimalApi+EF Core+Postgresql+Seq+FluentApi...

Core features:

1. Refactoring based on Semantic Kernel of https://ai.terramours.site/, realizing multi-AI model chat.
2. Image generation of dallE model of Stable Diffusion and chatgpt.

## Features

1. SignalR+Hangfire to implement background task queues and real-time communication.
2. Automapper for model auto-mapping.
3. Encapsulation of ApiResponse{code, message, data} for uniform return results of interfaces.
4. Semantic Kernel calling chatgpt.
5. Log service Seq.
6. Stable Diffusion image generation.

## Developed Features

- **AI Chat**: Initiate chat based on Semantic Kernel. Currently, chatgpt is written. Common models: gpt-3.5-turbo, gpt-3.5-turbo-16K, gpt-4 are supported.
- **AI Drawing**: Image generation based on dallE model of Stable Diffusion and chatgpt.
- **Chat Records**: Manage chat records and query user session information. (Todo: Create fine-tuning model)
- **Sensitive Word Management**: Manage sensitive words, customize sensitive word filtering, and enhance system security.
- **Key Pool Management**: Manage key pools, support administrators to add multiple keys to form a key pool, and perform polling when calling AI interfaces to enhance system stability.
- **System Prompts**: Add various role prompts to help users better use AI for conversations.

## Online Preview

- [TerraMours Admin Preview Address](https://demo.terramours.site/)

## Documentation

- [Project Documentation Address](https://terramours.site/)

## Front-End Services

- [TerraMours_Gpt_Web](https://github.com/TerraMours/TerraMours_Gpt_Web)

## Installation and Usage

- Environment configuration: **.net7 SDK**
- Clone the code:

```bash
git clone https://github.com/TerraMours/TerraMours_Admin_Web.git
```

## Docker Deployment

* Build image:

```bash
docker build -t terramours_gpt_server .
```

* Create a mounting directory (directory is customizable):

```bash
mkdir /data/terramoursgpt/server/images
```

- Deploy terramoursweb using Docker:

```bash
docker run --name terramours_gpt_server -v /data/terramoursgpt/server/images:/app/images  -p 3115:80  -d terramours_gpt_server
```

- Access swagger:

Open the local browser and access `http://localhost/swagger/index.html`

## Open-Source Authors

[@Raokun](https://github.com/raokun)

[@firstsaofan](https://github.com/orgs/TerraMours/people/firstsaofan)

## Communication

`TerraMours Admin` is a completely open-sourced and free-to-use project that helps developers to more conveniently develop medium to large-scale management systems, and also provides WeChat and QQ communication groups. If you have any questions about usage, please feel free to ask in the group.

  <div style="display:flex;">
  	<div style="padding-right:24px;">
  		<p>QQ Communication Group</p>
      <img src="https://www.raokun.top/upload/2023/06/%E4%BA%A4%E6%B5%81%E7%BE%A4.png" style="width:200px" />
  	</div>
		<div>
			<p>Add me to WeChat for technical exchange and business consultation.</p>
			<img src="https://www.raokun.top/upload/2023/04/%E5%BE%AE%E4%BF%A1%E5%9B%BE%E7%89%87_20230405192146.jpg" style="width:180px" />
		</div>
  </div>

## Donation

If you find this project helpful, you can buy a drink for the TerraMours team to show your support. The motivation for TerraMours to be open-sourced comes from everyone's support and encouragement.

  <div style="display:flex;">
  	<div style="padding-right:24px;">
  		<p>WeChat</p>
      <img src="https://www.raokun.top/upload/2023/04/%E5%BE%AE%E4%BF%A1%E6%94%B6%E6%AC%BE.jpg" style="width:200px" />
  	</div>
	  <div style="padding-right:24px;">
  		<p>AliPay</p>
      <img src="https://www.raokun.top/upload/2023/04/%E6%94%AF%E4%BB%98%E5%AE%9D%E6%94%B6%E6%AC%BE.jpg" style="width:200px" />
  	</div>
  </div>

## License

[Apache License © TerraMours-2023](./LICENSE)
