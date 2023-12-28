using System;
using System.CodeDom;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public partial class StoredProcedures
{

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void CheckQueryIntentCache(SqlString question)
    {
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            SqlArray embedded = OpenaiFunction.GetEmbedding(question);
            connection.Open();
            SqlCommand command = new SqlCommand("SELECT top 1 [GeneratedTSQL]\r\nFROM (\r\nSELECT [QueryIntent]\r\n      ,[GeneratedTSQL]\r\n\t  ,[CreateDate]\r\n\t  ,SqlRAG.dbo.CosineSimilarity(@embedded,VectorizedQueryIntent) as cosine_similarity\r\n\t  ,1-SqlRAG.dbo.EuclideanDistance(@embedded,VectorizedQueryIntent) as euclidean_similarity\r\n\t  ,1-SqlRAG.dbo.MinkowskiDistance(@embedded,VectorizedQueryIntent,2.5) as minkowski_similarity\r\n  FROM [SqlRAG].[dbo].[QueryIntentCache] \r\n  where ExecStatus is null) A\r\n  WHERE (cosine_similarity>=0.96) or (cosine_similarity>=0.90 and euclidean_similarity>0.70)\r\n  order by cosine_similarity desc", connection);
            SqlParameter paraEmbedded = new SqlParameter("embedded", embedded);
            command.Parameters.Add(paraEmbedded);
            SqlContext.Pipe.ExecuteAndSend(command);
            SqlDataReader r = command.ExecuteReader();
   
        }
    }


    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void InsertQueryIntentCache(SqlString query_intent, [SqlFacet(MaxSize = -1)] SqlString embedded, [SqlFacet(MaxSize = -1)] SqlString tsql, [SqlFacet(MaxSize = -1)] SqlString data, SqlString exec_status)
    {
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {

            connection.Open();
            SqlCommand command = new SqlCommand("INSERT INTO SQLRAG.[dbo].[QueryIntentCache]\r\n           ([QueryIntent]\r\n           ,[VectorizedQueryIntent]\r\n           ,[GeneratedTSQL]\r\n           ,[GeneratedData]\r\n           ,[ExecStatus])\r\n     VALUES\r\n           (@query_intent\r\n           ,SqlArray::Parse(@embedded)\r\n           ,@tsql\r\n           ,@data\r\n           ,@exec_status)", connection);
            SqlParameter paraQuestion = new SqlParameter("query_intent", query_intent.Value);
            SqlParameter paraEmbedded = new SqlParameter("embedded", embedded.Value);
            SqlParameter paraTSql = new SqlParameter("tsql", tsql.Value);
            SqlParameter paraData = new SqlParameter("data", data.Value);
            SqlParameter paraExecStatus = new SqlParameter("exec_status", exec_status.Value);


            command.Parameters.Add(paraQuestion);
            command.Parameters.Add(paraEmbedded);
            command.Parameters.Add(paraTSql);
            command.Parameters.Add(paraData);
            command.Parameters.Add(paraExecStatus);
            command.ExecuteNonQuery();

        }
    }

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void InsertKnowledgeBase(SqlGuid id, SqlGuid parent_id, SqlInt32 ordinal, SqlBoolean is_rewrite, SqlInt16 source_type, SqlString url, [SqlFacet(MaxSize = -1)] SqlString text_content, [SqlFacet(MaxSize = -1)] SqlString raw)
    {
       
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            try
            {
                SqlArray embedded=SqlArray.Null;
                try
                {
                    embedded = OpenaiFunction.GetEmbedding(text_content);
                }
                catch 
                {

                }
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO SQLRAG.[dbo].[KnowledgeBase]\r\n           ([PartId]\r\n           ,[ParentId]\r\n    ,[Ordinal]\r\n    ,[IsRewrite]\r\n         ,[SourceType]\r\n           ,[Url]\r\n           ,[TextContent]\r\n   ,[Embeddings]\r\n             ,[Raw])\r\n     VALUES\r\n           (@id ,@parent_id, @ordinal , @is_rewrite, @source_type,@url, @text_content,@embedded,  @raw)", connection);
                SqlParameter para_id = new SqlParameter("id", id);
                SqlParameter para_parentid = new SqlParameter("parent_id", parent_id);
                SqlParameter para_ordinal = new SqlParameter("ordinal", ordinal);
                SqlParameter para_isrewrite = new SqlParameter("is_rewrite", is_rewrite);
                SqlParameter para_source_type = new SqlParameter("source_type", source_type);
                SqlParameter para_url = new SqlParameter("url", url);
                SqlParameter para_text_content = new SqlParameter("text_content", text_content);
                SqlParameter paraEmbedded = new SqlParameter("embedded", embedded);
                SqlParameter para_raw = new SqlParameter("raw", raw);

                para_url.SqlDbType = SqlDbType.NVarChar;
                para_text_content.SqlDbType = SqlDbType.NVarChar;
                para_raw.SqlDbType = SqlDbType.NVarChar;
                paraEmbedded.SqlDbType = SqlDbType.Udt;
                paraEmbedded.UdtTypeName = "SqlArray";

                command.Parameters.Add(para_id);
                command.Parameters.Add(para_parentid);
                command.Parameters.Add(para_ordinal);
                command.Parameters.Add(para_isrewrite);
                command.Parameters.Add(para_source_type);
                command.Parameters.Add(para_url);
                command.Parameters.Add(para_text_content);
                command.Parameters.Add(paraEmbedded);
                command.Parameters.Add(para_raw);
                command.ExecuteNonQuery();

            }
            catch (System.Net.WebException we)
            {
                throw new Exception(we.Message + "\r\n" + we.InnerException.Message + "\r\n" + we.Response + "\r\n" + we.StackTrace);
            }


        }
    }



}
