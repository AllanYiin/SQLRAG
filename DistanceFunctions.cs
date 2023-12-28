using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public partial class DistanceFunctions
{



    /// <summary>
    /// Calculates the cosine similarity between two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The cosine similarity between the two vectors.</returns>
    [SqlFunction]
    public static SqlDouble CosineSimilarity(SqlArray vector1, SqlArray vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlDouble.Null;
        }

        double[] vec1 = vector1.Data;
        double[] vec2 = vector2.Data;

        if (vec1.Length != vec2.Length)
            return SqlDouble.Null;

        double dotProduct = 0.0;
        double normVec1 = 0.0;
        double normVec2 = 0.0;

        for (int i = 0; i < vec1.Length; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            normVec1 += vec1[i] * vec1[i];
            normVec2 += vec2[i] * vec2[i];
        }

        if (normVec1 == 0.0 || normVec2 == 0.0)
            return 0;

        return new SqlDouble(dotProduct / (Math.Sqrt(normVec1) * Math.Sqrt(normVec2)));
    }

 
    [SqlFunction]
    public static SqlDouble EuclideanDistance(SqlArray vector1, SqlArray vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlDouble.Null;
        }

        double[] vec1 = vector1.Data;
        double[] vec2 = vector2.Data;

        if (vec1.Length != vec2.Length)
            return SqlDouble.Null;

        double sum = 0.0;
        for (int i = 0; i < vec1.Length; i++)
        {
            sum += Math.Pow(vec1[i] - vec2[i], 2);
        }

        return Math.Sqrt(sum);
    }


    [SqlFunction]
    public static SqlDouble ManhattanDistance(SqlArray vector1, SqlArray vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlDouble.Null;
        }

        double[] vec1 = vector1.Data;
        double[] vec2 = vector2.Data;

        if (vec1.Length != vec2.Length)
            return SqlDouble.Null;

        double sum = 0.0;
        for (int i = 0; i < vec1.Length; i++)
        {
            sum += Math.Abs(vec1[i] - vec2[i]);
        }

        return sum;
    }



    [SqlFunction]
    public static SqlDouble HammingDistance(SqlString InputString1, SqlString InputString2)
    {
        if (InputString1.IsNull || InputString2.IsNull)
        {
            return SqlDouble.Null;
        }

        char[] vec1 = InputString1.Value.ToCharArray();
        char[] vec2 = InputString2.Value.ToCharArray();

        if (vec1.Length != vec2.Length)
            return SqlDouble.Null;

        int distance = 0;
        for (int i = 0; i < vec1.Length; i++)
        {
            if (vec1[i] != vec2[i])
                distance++;
        }

        return distance;
    }

    [SqlFunction]
    public static SqlDouble ChebyshevDistance(SqlArray vector1, SqlArray vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlDouble.Null;
        }

        double[] vec1 = vector1.Data;
        double[] vec2 = vector2.Data;

        if (vec1.Length != vec2.Length)
            return SqlDouble.Null;

        double maxDifference = 0.0;
        for (int i = 0; i < vec1.Length; i++)
        {
            double difference = Math.Abs(vec1[i] - vec2[i]);
            if (difference > maxDifference)
                maxDifference = difference;
        }

        return maxDifference;
    }




    [SqlFunction]
    public static SqlDouble MinkowskiDistance(SqlArray vector1, SqlArray vector2, SqlDouble p)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlDouble.Null;
        }

        if (p.Value < 1)
            return SqlDouble.Null; // p 必須大於等於1

        double[] vec1 = vector1.Data;
        double[] vec2 = vector2.Data;


        if (vec1.Length != vec2.Length)
            return SqlDouble.Null;

        double sum = 0.0;
        for (int i = 0; i < vec1.Length; i++)
        {
            sum += Math.Pow(Math.Abs(vec1[i] - vec2[i]), p.Value);
        }

        return Math.Pow(sum, 1.0 / p.Value);
    }


    [SqlFunction]
    public static SqlInt32 LevenshteinDistance([SqlFacet(MaxSize = -1)] SqlString InputString1, [SqlFacet(MaxSize = -1)] SqlString InputString2)
    {
        if (InputString1.IsNull || InputString2.IsNull)
        {
            return SqlInt32.Null;
        }

        int n = InputString1.Value.Length;
        int m = InputString2.Value.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return new SqlInt32(m);
        }

        if (m == 0)
        {
            return new SqlInt32(n);
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (InputString2.Value[j - 1] == InputString1.Value[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return new SqlInt32(d[n, m]);
    }




}


