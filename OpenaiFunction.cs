using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Data.SqlTypes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.IO;
using System.Reflection;
using System.Data.Common;
using System.Collections.ObjectModel;


public partial class OpenaiFunction
{



    private static string GetKey()
    {
        System.Data.SqlClient.SqlConnection.ColumnEncryptionKeyCacheTtl = TimeSpan.Zero;
        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(
                @"Declare @encrytext varbinary(4000)=(SELECT  [KeyValue] FROM [SQLRAG].[dbo].[EncryptedKeys] WHERE  [KeyName]='OPENAI_API_KEY')
                    Declare @decrytext varchar(512)=DecryptByCert(Cert_ID('SqlRAGCertificate'),@encrytext,N'P@ssw0rd')
                    select @decrytext ", conn);
        
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return (string)reader[0];
                }
            }
            return string.Empty;
        }
    }

    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlString GetEmbedding(SqlString inputText)
    {
       
        string apiKey = GetKey();
        string apiUrl = "https://api.openai.com/v1/embeddings";
        string requestBody = $"{{\"model\":\"text-embedding-ada-002\",\"input\": \"{inputText}\",\"encoding_format\":\"float\"}}";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.Headers["Authorization"] = $"Bearer {apiKey}";
            request.ContentType = "application/json";

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();
                return new SqlString(ParseEmbedding(result)); // 返回結果
            }
        }
        catch (Exception ex)
        {
            // 錯誤處理
            return new SqlString($"Error: {ex.Message}");
        }
    }

    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlString ChatCompletion(SqlString inputPrompt, SqlString systemProimpt = default(SqlString), SqlString model = default(SqlString))
    {
       ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        string apiKey = GetKey();
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string modelToUse = model.IsNull || String.IsNullOrWhiteSpace(model.Value) ? "gpt-3.5-turbo" : model.Value;
        string requestBody1 = $@"{{""model"": ""{modelToUse}"", ""messages"": [{{""role"": ""system"", ""content"": ""{systemProimpt.ToString()}""}},{{""role"": ""user"", ""content"": ""{inputPrompt.ToString()}""}}]}}";
        string requestBody = systemProimpt.IsNull || String.IsNullOrWhiteSpace(systemProimpt.Value) ? $@"{{""model"": ""{modelToUse}"", ""messages"": [{{""role"": ""user"", ""content"": ""{inputPrompt.ToString()}""}}]}}" : requestBody1;

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.Headers["Authorization"] = $"Bearer {apiKey}";
            request.Headers["Accept-Language"] = "zh-TW";
            request.ContentType = "application/json";

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();
                return new SqlString(ParseChatting(result)); // 返回結果
            }
        }
        catch (Exception ex)
        {
            // 錯誤處理
            return new SqlString($"Error: {ex.Message}");
        }
    }



    private static string ParseEmbedding(string jsonResponse)
    {
        // 簡單的 JSON 解析來提取 embedding
        string startPattern = "\"embedding\": [";
        string endPattern = "]";
        int startIndex = jsonResponse.IndexOf(startPattern) + startPattern.Length;
        int endIndex = jsonResponse.IndexOf(endPattern, startIndex);
        string embeddingString = jsonResponse.Substring(startIndex, endIndex - startIndex);

        // 清理和格式化數據
        embeddingString = embeddingString.Replace("\n", "").Replace("\r", "").Replace(" ", "");
        return embeddingString;
    }

    private static string ParseChatting(string jsonResponse)
    {
        // 簡單的 JSON 解析來提取 embedding
        string startPattern = "\"content\": ";
        string endPattern = "finish_reason";
        int startIndex = jsonResponse.IndexOf(startPattern) + startPattern.Length+1;
        int endIndex = jsonResponse.IndexOf(endPattern, startIndex);
        string responseString = jsonResponse.Substring(startIndex, endIndex - startIndex-17).TrimEnd(" ".ToCharArray());
        //endIndex = responseString.IndexOf("\"   ", responseString.Length - 20);
        //responseString= responseString.Substring(0, endIndex).TrimEnd(" ".ToCharArray());
        // 清理和格式化數據
        return responseString;
    }

}
