using System;
using System.Runtime;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.IO;


[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
public struct SqlArray : INullable, IBinarySerialize
{
    //  Private 成員
    private bool _null;
    private double[] _data;

    public static SqlArray Null
    {
        get
        {
            SqlArray h = new SqlArray();
            h._null = true;
            return h;
        }
    }


    public double[] Data
    {
        get
        {
            // 將程式碼放在此處
            return _data;
        }
        private set { _data = value; }

    }

    public SqlArray(double[] dataArray)
    {
        _null = false;
        _data = dataArray;
    }

    public static SqlArray operator +(SqlArray a) => a;
    public static SqlArray operator -(SqlArray a) => new SqlArray { _data = a._data };

    public static SqlArray operator +(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.GetLength(0)];
        for (int i = 0; i < a._data.GetLength(0); i++)
        {
            result[i] = a._data[i] + b._data[i];
        }
        return new SqlArray(result);
    }

    public static SqlArray operator -(SqlArray a, SqlArray b)
        => a + (-b);

    public static SqlArray operator *(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.GetLength(0)];
        for (int i = 0; i < a._data.GetLength(0); i++)
        {
            result[i] = a._data[i] * b._data[i];
        }
        return new SqlArray(result);
    }

    public static SqlArray operator /(SqlArray a, SqlArray b)
    {
        if (System.Array.Exists(b._data, x => x == 0))
        {
            throw new DivideByZeroException();
        }
        var result = new double[a._data.GetLength(0)];
        for (int i = 0; i < a._data.GetLength(0); i++)
        {
            result[i] = a._data[i] / b._data[i];
        }
        return new SqlArray(result);
    }


    public override string ToString()
    {

        return _data.ToString();
    }

    public bool IsNull
    {
        get
        {
            // 將程式碼放在此處
            return _null;
        }
    }



    public int Rank
    {
        get
        {
            // 將程式碼放在此處
            return _data.Rank;
        }
    }

    public int Length
    {
        get
        {
            // 將程式碼放在此處
            return _data.Length;
        }
    }

    public long LongLength
    {
        get
        {
            // 將程式碼放在此處
            return _data.LongLength;
        }
    }





    [SqlMethod(OnNullCall = true)]
    public static SqlArray Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        SqlArray returnArray = new SqlArray();
        returnArray._data = System.Array.ConvertAll(s.Value.Split(','), Double.Parse);

        // 將程式碼放在此處
        return returnArray;
    }


    public SqlArray Concate(List<SqlArray> arrays)
    {
        List<double> data_list = new List<double>();
        foreach (SqlArray a in arrays)
        {
            data_list.AddRange(a.Data);
        }
        return new SqlArray(data_list.ToArray());
    }



    /// 實現IBinarySerialize
    public void Read(BinaryReader r)
    {
        if (r == null) throw new ArgumentNullException("r");
        var tmp_data = new List<double>();
        int count = r.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            tmp_data.Add(r.ReadDouble());
        }
        _data = tmp_data.ToArray();
    }

    public void Write(BinaryWriter w)
    {
        if (w == null) throw new ArgumentNullException("w");
        w.Write(_data.Length);
        foreach (double d in _data)
        {
            w.Write(d);
        }
    }


}