using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public partial class DistanceFunctions
{


    [SqlFunction]
    public static SqlDouble CosineSimilarity(SqlString vector1, SqlString vector2)
    {
        double[] vec1 = Array.ConvertAll(vector1.Value.Split(','), Double.Parse);
        double[] vec2 = Array.ConvertAll(vector2.Value.Split(','), Double.Parse);

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

        return dotProduct / (Math.Sqrt(normVec1) * Math.Sqrt(normVec2));
    }


    [SqlFunction]
    public static SqlDouble EuclideanDistance(SqlString vector1, SqlString vector2)
    {
        double[] vec1 = Array.ConvertAll(vector1.Value.Split(','), Double.Parse);
        double[] vec2 = Array.ConvertAll(vector2.Value.Split(','), Double.Parse);

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
    public static SqlDouble ManhattanDistance(SqlString vector1, SqlString vector2)
    {
        double[] vec1 = Array.ConvertAll(vector1.Value.Split(','), Double.Parse);
        double[] vec2 = Array.ConvertAll(vector2.Value.Split(','), Double.Parse);

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
    public static SqlDouble HammingDistance(SqlString vector1, SqlString vector2)
    {
        char[] vec1 = vector1.Value.ToCharArray();
        char[] vec2 = vector2.Value.ToCharArray();

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
    public static SqlDouble ChebyshevDistance(SqlString vector1, SqlString vector2)
    {
        double[] vec1 = Array.ConvertAll(vector1.Value.Split(','), Double.Parse);
        double[] vec2 = Array.ConvertAll(vector2.Value.Split(','), Double.Parse);

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
    public static SqlDouble MinkowskiDistance(SqlString vector1, SqlString vector2, SqlDouble p)
    {
        if (p.Value < 1)
            return SqlDouble.Null; // p 必須大於等於1

        double[] vec1 = Array.ConvertAll(vector1.Value.Split(','), Double.Parse);
        double[] vec2 = Array.ConvertAll(vector2.Value.Split(','), Double.Parse);

        if (vec1.Length != vec2.Length)
            return SqlDouble.Null;

        double sum = 0.0;
        for (int i = 0; i < vec1.Length; i++)
        {
            sum += Math.Pow(Math.Abs(vec1[i] - vec2[i]), p.Value);
        }

        return Math.Pow(sum, 1.0 / p.Value);
    }
}


