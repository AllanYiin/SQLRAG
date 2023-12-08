# SQLRAG
## 讓SQL Server也能執行向量相似性查詢

目前已經實現功能:  
- 各種相似性比較CLR函數: CosineSimilarity, EuclideanDistance...  [參考 (https://chat.openai.com/share/40723def-afdd-4f83-adb3-fbcae34b617a)]
- 透過CLR函數調用Openai API(GetEmbedding, ChatCompletion...) 


## 安裝
1. 首先要啟用SQL Server CLR:   
```sql
	sp_configure 'show advanced options', 1;  
	RECONFIGURE;  
	sp_configure 'clr enabled', 1;  
	RECONFIGURE;  
```

2. 如果要使用OpenaiFunction，之前的做法是在OpenaiFunction.cs中輸入你實際的OPENAI_API_KEY，這樣的做法是不能確保API KEY的安全的。在這一版中，我在SqlRAG資料庫中增加了dbo.EncryptedKeys資料表，其中KeyValue部分透過憑證加密。看起來複雜但我已經將自動調用的部分處理好，開發者只需要透過以下SQL 語法將API KEY加密後寫入資料表中即可。  
```sql
	Declare @cleartext varchar(512)='sk-輸入你的OPENAI_API_KEYAPI KEY'
	Declare @encrytext varbinary(4000)=EncryptByCert(Cert_ID('SqlRAGCertificate'), @cleartext) 

	INSERT INTO [SQLRAG].[dbo].[EncryptedKeys] 
    	VALUES ( N'OPENAI_API_KEY', N'用於調用OPENAI所用之API KEY',@encrytext );  
```
或者是像透過顯式的聲明基於哪個憑證以及對應的密碼來進行解密:
```sql

DecryptByCert(Cert_ID('SqlRAGCertificate'), EncryptByCert(Cert_ID('SqlRAGCertificate'), @cleartext) ,'P@ssw0rd')

```
3. 建置專案，發行至指定資料庫 (專案中的SQLRAG.publish.xml為發行設定檔範例，請改指向至你實際的資料庫，在assets中有SqlRAG資料庫的備份可以直接還原)   
4. assets中的QueryIntentCache_demo.sql (語意快取範例)以及ChatCompletion_demo.sql(ChatGPT回答範例)   

![ChatCompletion](assets/QueryCache.png)
![ChatCompletion](assets/ChatCompletion.png)


