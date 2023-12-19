using System;
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
}
