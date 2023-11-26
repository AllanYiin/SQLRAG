Declare @question varchar(max)
set @question = SqlRAG.dbo.GetEmbedding('2013年度各產品大分類的毛利率')

SELECT [QueryIntent]
      ,[GeneratedTSQL]
	  ,SqlRAG.dbo.CosineSimilarity(@question,VectorizedQueryIntent) as cosine_similarity
	  ,1-SqlRAG.dbo.EuclideanDistance(@question,VectorizedQueryIntent) as euclidean_similarity
	  ,1-SqlRAG.dbo.MinkowskiDistance(@question,VectorizedQueryIntent,2.5) as minkowski_similarity
  FROM [SqlRAG].[dbo].[QueryIntentCache]
  order by 5 desc
