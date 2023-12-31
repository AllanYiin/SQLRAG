﻿using System;
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
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;



public partial class OpenaiFunction
{



    private static string GetKey()
    {
        //System.Data.SqlClient.SqlConnection.ColumnEncryptionKeyCacheTtl = TimeSpan.Zero;
        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(
                @"Declare @encrytext varbinary(4000)=(SELECT  [KeyValue] FROM [SQLRAG].[dbo].[EncryptedKeys] WHERE  [KeyName]='OPENAI_API_KEY')
                    Declare @decrytext varchar(512)=DecryptByCert(Cert_ID('SqlRAGCertificate'),@encrytext,N'SqlRAGP@ssw0rd')
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
    public static SqlArray GetEmbedding([SqlFacet(MaxSize = -1)] SqlString inputText)
    {
       
        string apiKey = GetKey();
        string apiUrl = "https://api.openai.com/v1/embeddings";
        string requestBody = $"{{\"model\":\"text-embedding-ada-002\",\"input\": \"{inputText}\",\"encoding_format\":\"float\"}}";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
                                "Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
                                ".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
                                "InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";
            request.Method = "POST";
            request.Headers["Authorization"] = $"Bearer {apiKey}";
            request.Headers["Accept-Language"] = "zh-TW";
            request.ContentType = "application/json";

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                return new SqlArray(System.Array.ConvertAll(ParseEmbedding(result).Split(','), Double.Parse)); // 返回結果
            }
        }
        catch (Exception)
        {
            return null;
        }
    }





    [return: SqlFacet(MaxSize = -1)]
    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlString ChatCompletion([SqlFacet(MaxSize = -1)] SqlString inputPrompt, [SqlFacet(MaxSize = -1)] SqlString systemProimpt = default(SqlString), SqlString model = default(SqlString))
    {
       ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        string apiKey = GetKey();
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string modelToUse = model.IsNull || String.IsNullOrWhiteSpace(model.Value) ? "gpt-3.5-turbo" : model.Value;
     
        string requestBody1 =$"{{\"model\": \"{modelToUse}\", \"messages\": [{{\"role\": \"system\", \"content\": \"{systemProimpt.Value}\"}},{{\"role\": \"user\", \"content\": \"{inputPrompt.Value}\"}}]}}";
        string requestBody = systemProimpt.IsNull || String.IsNullOrWhiteSpace(systemProimpt.Value) ? $"{{\"model\": \"{modelToUse}\", \"messages\": [{{\"role\": \"user\", \"content\": \"{inputPrompt.ToString()}\"}}]}}" : requestBody1;

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.UserAgent =
               "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
               "Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
               ".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
               "InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";
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
                return ParseChatting(result); // 返回結果
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

    private const RegexOptions ExpressionOptions = RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase;
    private static SqlString ParseChatting(string jsonResponse)
    {

        Match match = Regex.Match(jsonResponse, "\"content\": \"(.*?)\"", ExpressionOptions);
        // 判斷是否匹配成功
        if (match.Success)
        {
            // 獲取第一個子組的值
            string content = match.Groups[1].Value;
            return new SqlString(content);
        }
        else
        {
            return SqlString.Null;
        }

    }



    //[return: SqlFacet(MaxSize = -1)]
    //[SqlFunction(DataAccess = DataAccessKind.Read)]
    //public static SqlString Translate2zhtw([SqlFacet(MaxSize = -1)] SqlString inputPrompt, SqlString model = default(SqlString))
    //{

    //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    //    string apiKey = GetKey();
    //    string apiUrl = "https://api.openai.com/v1/chat/completions";
    //    string modelToUse = model.IsNull || String.IsNullOrWhiteSpace(model.Value) ? "gpt-3.5-turbo" : model.Value;

    //    string requestBody = $"{{\"model\": \"{modelToUse}\", \"messages\": [{{\"role\": \"user\", \"content\": \"請直接將以下文字內容翻譯為繁體中文:\r\n{inputPrompt.Value}\"}}]}}";
        
    //    try
    //    {
    //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
    //        request.Method = "POST";
    //        request.Headers["Authorization"] = $"Bearer {apiKey}";
    //        request.Headers["Accept-Language"] = "zh-TW";
    //        request.ContentType = "application/json";

    //        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
    //        {
    //            streamWriter.Write(requestBody);
    //        }

    //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
    //        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
    //        {
    //            string result = streamReader.ReadToEnd();
    //            return ParseChatting(result); // 返回結果
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // 錯誤處理
    //        return new SqlString($"Error: {ex.Message}");
    //    }


    //}



}
