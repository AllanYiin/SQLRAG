# SQLRAG
## 讓SQL Server也能執行向量相似性查詢

目前已經實現功能:  
- 各種相似性比較CLR函數: CosineSimilarity, EuclideanDistance...  [參考 (https://chat.openai.com/share/40723def-afdd-4f83-adb3-fbcae34b617a)]
- 透過CLR函數調用Openai API(GetEmbedding, ChatCompletion...) 


## 安裝
最小安裝要求:SQL Server 2022, Visual Studio 2022(需安裝SSDT)

1. 首先要啟用SQL Server CLR:   
```sql
	use SQLRAG
	sp_configure 'show advanced options', 1;  
	RECONFIGURE;  
	sp_configure 'clr enabled', 1;  
	RECONFIGURE;  
```

2. 有以下幾種方式可以安裝:
	- 使用release中提供的SQLRAG_CREATE.sql直接執行即可完成安裝(適合首次安裝者)
	- 使用release中提供的dacpac檔，參考[網頁 (https://learn.microsoft.com/zh-tw/sql/relational-databases/data-tier-applications/upgrade-a-data-tier-application?view=sql-server-ver16)]內容安裝。
	- 使用release中提供的原始碼，使用Visual Studio 2022開啟後修改各專案的資料庫連線後直接部署方案

3. 如果要使用OpenaiFunction，之前的做法是在OpenaiFunction.cs中輸入你實際的OPENAI_API_KEY，這樣的做法是不能確保API KEY的安全的。在這一版中，我在SqlRAG資料庫中增加了dbo.EncryptedKeys資料表，其中KeyValue部分透過憑證加密。看起來複雜但我已經將自動調用的部分處理好，開發者只需要透過以下SQL 語法將API KEY加密後寫入資料表中即可。  
```sql
	use SQLRAG
	Declare @cleartext varchar(512)='sk-輸入你的OPENAI_API_KEYAPI KEY'
	Declare @encrytext varbinary(4000)=EncryptByCert(Cert_ID('SqlRAGCertificate'), @cleartext) 

	INSERT INTO [SQLRAG].[dbo].[EncryptedKeys] 
    	VALUES ( N'OPENAI_API_KEY', N'調用OPENAI所用之API KEY',@encrytext );  
```
之後則需要透過顯式的聲明基於哪個憑證以及對應的密碼來進行解密:
```sql

    DecryptByCert(Cert_ID('SqlRAGCertificate'), EncryptByCert(Cert_ID('SqlRAGCertificate'), @cleartext) ,'P@ssw0rd')

```

如果你是Azure Openai Service用戶則需要將api key以及endpoint加密後存入
```sql

	use SQLRAG
	Declare @cleartext varchar(512)='***'
	Declare @encrytext varbinary(4000)=EncryptByCert(Cert_ID('SqlRAGCertificate'), @cleartext) 

	Declare @cleartext2 varchar(512)='https://***.openai.azure.com'
	Declare @encrytext2 varbinary(4000)=EncryptByCert(Cert_ID('SqlRAGCertificate'), @cleartext2) 

	Declare @cleartext3 varchar(512)='***'
	Declare @encrytext3 varbinary(4000)=EncryptByCert(Cert_ID('SqlRAGCertificate'), @cleartext3) 

	INSERT INTO [SQLRAG].[dbo].[EncryptedKeys] 
    	VALUES ( N'AZURE_OPENAI_API_KEY', N'調用Azure Openai Service 所用之API KEY',@encrytext );  
	INSERT INTO [SQLRAG].[dbo].[EncryptedKeys] 
    	VALUES ( N'AZURE_OPENAI_ENDPOINT', N'調用Azure Openai Service 所用之endpoint',@encrytext2 );  
	INSERT INTO [SQLRAG].[dbo].[EncryptedKeys] 
    	VALUES ( N'OPENAI_API_VERSION', N'調用Azure Openai Service 所用之API Version',@encrytext3 );  

```



4. assets中的QueryIntentCache_demo.sql (語意快取範例)以及ChatCompletion_demo.sql(ChatGPT回答範例)   

![ChatCompletion](assets/QueryCache.png)
![ChatCompletion](assets/ChatCompletion.png)


