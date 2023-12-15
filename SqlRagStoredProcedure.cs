﻿using System;
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
      
            connection.Open();
            SqlCommand command = new SqlCommand("Declare @embedded nvarchar(max)\r\nset @embedded=SqlRAG.dbo.GetEmbedding(@question)\r\n\r\nSELECT top 1 [GeneratedTSQL]\r\nFROM (\r\nSELECT [QueryIntent]\r\n      ,[GeneratedTSQL]\r\n\t  ,[CreateDate]\r\n\t  ,SqlRAG.dbo.CosineSimilarity(@embedded,VectorizedQueryIntent) as cosine_similarity\r\n\t  ,1-SqlRAG.dbo.EuclideanDistance(@embedded,VectorizedQueryIntent) as euclidean_similarity\r\n\t  ,1-SqlRAG.dbo.MinkowskiDistance(@embedded,VectorizedQueryIntent,2.5) as minkowski_similarity\r\n  FROM [SqlRAG].[dbo].[QueryIntentCache] \r\n  where ExecStatus is null) A\r\n  WHERE (cosine_similarity>=0.96) or (cosine_similarity>=0.90 and euclidean_similarity>0.70)\r\n  order by cosine_similarity desc", connection);
            SqlParameter paraQuestion = new SqlParameter("question", question.Value);
            command.Parameters.Add(paraQuestion);
            SqlContext.Pipe.ExecuteAndSend(command);
            SqlDataReader r = command.ExecuteReader();
            SqlContext.Pipe.Send(r);
        }
    }
}
