using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public partial class RagFunctions
{
   
    [return: SqlFacet(MaxSize = -1)]
    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlString GetCachedSQL([SqlFacet(MaxSize = -1)] SqlString question)
    {
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            SqlArray embedded = OpenaiFunction.GetEmbedding(question);
            connection.Open();
            SqlCommand command = new SqlCommand("Declare @embedded SqlArray  \r\nset @embedded=SqlArray::Parse(@embedded_string)    \r\nselect top 1 [GeneratedTSQL]\r\nFROM\r\n(SELECT [QueryIntent]     \r\n,[GeneratedTSQL]\r\n,[CreateDate]\r\n,SqlRAG.dbo.CosineSimilarity(@embedded,VectorizedQueryIntent) as cosine_similarity\r\n,1-SqlRAG.dbo.EuclideanDistance(@embedded,VectorizedQueryIntent) as euclidean_similarity\r\nFROM [SqlRAG].[dbo].[QueryIntentCache]\r\nwhere ExecStatus is null) A\r\nWHERE (cosine_similarity>=0.96) or (cosine_similarity>=0.90 and euclidean_similarity>0.70) or (cosine_similarity=-1 and euclidean_similarity>-1)\r\norder by cosine_similarity desc", connection);
            SqlParameter paraEmbedded = new SqlParameter("embedded_string", SqlDbType.NVarChar,-1);
            paraEmbedded.Value = embedded.ToString();
            command.Parameters.Add(paraEmbedded);
            return new SqlString((string)command.ExecuteScalar());
      



        }
    }
}
