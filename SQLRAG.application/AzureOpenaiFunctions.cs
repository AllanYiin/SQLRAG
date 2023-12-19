using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

public partial class AzureOpenaiFunction
{

    private static string GetKey()
    {
        System.Data.SqlClient.SqlConnection.ColumnEncryptionKeyCacheTtl = TimeSpan.Zero;
        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(
                @"Declare @encrytext varbinary(4000)=(SELECT  [KeyValue] FROM [SQLRAG].[dbo].[EncryptedKeys] WHERE  [KeyName]='AZURE_OPENAI_API_KEY')
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

    private static string GetEndpoint()
    {
        System.Data.SqlClient.SqlConnection.ColumnEncryptionKeyCacheTtl = TimeSpan.Zero;
        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(
                @"Declare @encrytext varbinary(4000)=(SELECT  [KeyValue] FROM [SQLRAG].[dbo].[EncryptedKeys] WHERE  [KeyName]='AZURE_OPENAI_ENDPOINT')
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

    private static string GetVersion()
    {
        System.Data.SqlClient.SqlConnection.ColumnEncryptionKeyCacheTtl = TimeSpan.Zero;
        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(
                @"Declare @encrytext varbinary(4000)=(SELECT  [KeyValue] FROM [SQLRAG].[dbo].[EncryptedKeys] WHERE  [KeyName]='OPENAI_API_VERSION')
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
    public static SqlArray GetAzureEmbedding([SqlFacet(MaxSize = -1)] SqlString inputText,SqlString deploymentName)
    {

        string apiKey = GetKey();
        string apiUrl = "";
        string endpoint = GetEndpoint();
        string apiVersion = GetVersion();
        if (!deploymentName.IsNull)
        {
            apiUrl = string.Format("{0}/openai/deployments/{1}/embeddings?api-version={2}", endpoint, deploymentName.Value, apiVersion);
        }
        else
        {
            throw new ArgumentException();
        }

        string requestBody = $"{{\"input\": \"{inputText}\",\"encoding_format\":\"float\"}}";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.Headers["api-key"] = apiKey;
            request.Headers["Accept-Language"] = "zh-TW";
            request.ContentType = "application/json";


            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();

            }
            return new SqlArray(System.Array.ConvertAll(ParseEmbedding(result).Split(','), Double.Parse)); // 返回結果
        }
        catch (Exception ex)
        {
            // 錯誤處理
            throw ex;
        }
    }





    [return: SqlFacet(MaxSize = -1)]
    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlString AzureChatCompletion([SqlFacet(MaxSize = -1)] SqlString inputPrompt, [SqlFacet(MaxSize = -1)] SqlString systemProimpt = default(SqlString), SqlString deploymentName = default(SqlString))
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        string apiKey = GetKey();
        string apiUrl = "";
        string endpoint = GetEndpoint();
        string apiVersion = GetVersion();
        if (!deploymentName.IsNull)
        {
            apiUrl = string.Format("{0}/openai/deployments/{1}/chat/completions?api-version={2}", endpoint, deploymentName.Value, apiVersion);
        }
        else
        {
            throw new ArgumentException();
        }

        
        string requestBody1 = $@"{{""messages"": [{{""role"": ""system"", ""content"": ""{systemProimpt.ToString()}""}},{{""role"": ""user"", ""content"": ""{inputPrompt.ToString()}""}}]}}";
        string requestBody = systemProimpt.IsNull || String.IsNullOrWhiteSpace(systemProimpt.Value) ? $@"{{ ""messages"": [{{""role"": ""user"", ""content"": ""{inputPrompt.ToString()}""}}]}}" : requestBody1;

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.Headers["api-key"] = apiKey;
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
     
        Match match = Regex.Match(jsonResponse, "\"content\":\"(.*?)\"", ExpressionOptions);
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


}
