using System;
using System.Runtime;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Data.Linq;



[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1, IsByteOrdered = true, IsFixedLength = false, Name = "SqlArray")]
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

    internal SqlArray(bool isNull) { _null = isNull; _data = Array.Empty<double>(); }

    //public SqlArray()
    //{
    //    _null = true;
    //    _data = [];
    //}

    public SqlArray(double[] dataArray)
    {
        _null = false;
        _data = dataArray;
    }

    //public static SqlArray operator +(SqlArray a) => a;

    public static SqlArray operator -(SqlArray a)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = -a._data[i];
        }
        return new SqlArray(result);
    }

    public static SqlBoolean Equals(SqlArray a, SqlArray b)
    {
        if (a.Length != b.Length)
        {
            return new SqlBoolean(false);
        }
        else
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a._data[i] != b._data[i])
                {
                    return new SqlBoolean(false);
                }
            }
            return new SqlBoolean(true);
        }
    }


    public static SqlBoolean operator ==(SqlArray a, SqlArray b)
    {
        if (a.Length != b.Length)
        {
            return new SqlBoolean(false);
        }
        else
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a._data[i] != b._data[i])
                {
                    return new SqlBoolean(false);
                }
            }
            return new SqlBoolean(true);
        }
    }

    public static SqlBoolean operator !=(SqlArray a, SqlArray b)
    {
        if (a.Length != b.Length)
        {
            return new SqlBoolean(true);
        }
        else
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a._data[i] != b._data[i])
                {
                    return new SqlBoolean(true);
                }
            }
            return new SqlBoolean(false);
        }
    }

    public override int GetHashCode()
       => _data.GetHashCode();

    public override bool Equals(object other)
        => other is SqlArray otherArray && _data == otherArray._data;

    public bool Equals(SqlArray other)
       => _data == other._data;


    [SqlMethod(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Add(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] + b._data[i];
        }
        return new SqlArray(result);
    }
    public static SqlArray operator +(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] + b._data[i];
        }
        return new SqlArray(result);
    }

    [SqlMethod(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Subtract(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] - b._data[i];
        }
        return new SqlArray(result);
    }
    public static SqlArray operator -(SqlArray a, SqlArray b)
    {

        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] - b._data[i];
        }
        return new SqlArray(result);
    }

    [SqlMethod(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Divide(SqlArray a, SqlArray b)
    {
        if (System.Array.Exists(b._data, x => x == 0))
        {
            throw new DivideByZeroException();
        }
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] / b._data[i];
        }
        return new SqlArray(result);
    }
    public static SqlArray operator /(SqlArray a, SqlArray b)
    {
        if (System.Array.Exists(b._data, x => x == 0))
        {
            throw new DivideByZeroException();
        }
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] / b._data[i];
        }
        return new SqlArray(result);
    }

    [SqlMethod(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Multiply(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] * b._data[i];
        }
        return new SqlArray(result);
    }
    [SqlMethod(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Dot(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] * b._data[i];
        }
        return new SqlArray(result);
    }
    public static SqlArray operator *(SqlArray a, SqlArray b)
    {
        // 檢查維度是否匹配，實際實現中應該添加更多錯誤處理
        var result = new double[a._data.Length];
        for (int i = 0; i < a._data.Length; i++)
        {
            result[i] = a._data[i] * b._data[i];
        }
        return new SqlArray(result);
    }


    public static explicit operator SqlArray(SqlString s)
    {
        if (s.IsNull)
            return Null;

        // 將程式碼放在此處
        return new SqlArray(System.Array.ConvertAll(s.Value.Split(','), Double.Parse));
    }
    public static implicit operator SqlArray(string s)
    {
        if (string.IsNullOrEmpty(s))
            return Null;

        // 將程式碼放在此處
        return new SqlArray(System.Array.ConvertAll(s.Split(','), Double.Parse));
    }
    public static explicit operator string(SqlArray x)
    {
        return string.Join(",", x._data);
    }















    [return: SqlFacet(MaxSize = -1)]
    [SqlMethod(IsDeterministic = true, IsPrecise = false)]
    public override string ToString()
    {

        string _str = string.Join(",", _data);
        return _str;

    }

    public bool IsNull
    {
        get
        {
            return _null;
        }
    }

    public int Rank
    {
        get
        {

            return _data.Rank;
        }
    }

    public int Length
    {
        get
        {
            return _data.Length;
        }
    }

    public long LongLength
    {
        get
        {
            return _data.LongLength;
        }
    }





    [SqlMethod(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;

        // 將程式碼放在此處
        return new SqlArray(System.Array.ConvertAll(s.Value.Split(','), Double.Parse));
    }

    //[SqlMethod(IsDeterministic = true, IsPrecise = true)]
    //public static SqlArray add(SqlArray a, SqlArray b)
    //{
    //    if (a._data.Length != b._data.Length)
    //    {
    //        throw new ArgumentException("維度不匹配");
    //    }
    //    var result = new double[a._data.Length];
    //    for (int i = 0; i < a._data.Length; i++)
    //    {
    //        result[i] = a._data[i] + b._data[i];
    //    }
    //    return new SqlArray(result);
    //}

    //[SqlMethod(IsDeterministic = true, IsPrecise = true)]
    //public static SqlArray subtract(SqlArray a, SqlArray b)
    //{
    //    if (a._data.Length != b._data.Length)
    //    {
    //        throw new ArgumentException("維度不匹配");
    //    }
    //    var result = new double[a._data.Length];
    //    for (int i = 0; i < a._data.Length; i++)
    //    {
    //        result[i] = a._data[i] - b._data[i];
    //    }
    //    return new SqlArray(result);
    //}



    public SqlArray Concate(List<SqlArray> arrays)
    {
        List<double> data_list = new List<double>();
        foreach (SqlArray a in arrays)
        {
            data_list.AddRange(a.Data);
        }
        return new SqlArray(data_list.ToArray());
    }
    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 ArgMax(SqlArray arr)
    {
        int idx = 0;
        double max_value = arr._data[0];
        for(int i=0;i< arr._data.Length;i++)
        {
            double d = arr._data[i];
            if (d > max_value)
            {
                idx = i;
                max_value = d;
            }
        }
        return new SqlInt32(idx);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 ArgMin(SqlArray arr)
    {
        int idx = 0;
        double min_value = arr._data[0];
        for (int i = 0; i < arr._data.Length; i++)
        {
            double d = arr._data[i];
            if (d <min_value)
            {
                idx = i;
                min_value = d;
            }
        }
        return new SqlInt32(idx);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Negate(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        for(int i=0;i<arr._data.Length;i++)
        {
            _new_data[i] = arr._data[i];
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Square(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = Math.Pow(arr._data[i],2);
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Sqrt(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] =Math.Sqrt(arr._data[i]);
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Abs(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = Math.Abs(arr._data[i]);
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Exp(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = Math.Exp(arr._data[i]);
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Log(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = Math.Log(arr._data[i]);
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Log10(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = Math.Log10(arr._data[i]);
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Pow(SqlArray arr,SqlDouble p)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = Math.Pow(arr._data[i],p.Value);
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray Clip(SqlArray arr,SqlDouble min_value, SqlDouble max_value)
    {
        var _new_data = new double[arr._data.Length];
        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = Math.Min(max_value.Value,Math.Max(min_value.Value,arr._data[i]));
        }
        return new SqlArray(_new_data);
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public static SqlArray L2Norm(SqlArray arr)
    {
        var _new_data = new double[arr._data.Length];
        var _norm = 0.0;
        foreach (double d in arr._data)
        {
            _norm += Math.Pow(d, 2);
        }
        _norm = Math.Sqrt(_norm / arr._data.Length);

        for (int i = 0; i < arr._data.Length; i++)
        {
            _new_data[i] = arr._data[i]/ _norm;
        }
        return new SqlArray(_new_data);
    }


    /// 實現IBinarySerialize
    public void Read(BinaryReader r)
    {
        if (r == null) throw new ArgumentNullException("r");
        _null = r.ReadBoolean();
        int len = r.ReadInt32();
        _data = new double[len];
        for (int i = 0; i < len; i++)
        {
            _data[i] = r.ReadDouble();
        }
    }

    [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
    public void Write(BinaryWriter w)
    {
        if (w == null) throw new ArgumentNullException("w");
        w.Write(_null);
        w.Write(_data.Length);
        foreach (double d in _data)
        {
            w.Write(d);
        }
    }

    public static SqlArray Deserialize(SqlBytes bytes)
    {
        if (bytes is null)
            throw new ArgumentNullException(nameof(bytes));
        using (var r = new BinaryReader(bytes.Stream))
        {
            var _new_null = r.ReadBoolean();
            var _new_len = r.ReadInt32();
            var _new_data = new double[_new_len];
            for (int i = 0; i < _new_len; i++)
            {
                _new_data[i] = r.ReadDouble();
            }

            return new SqlArray(_new_data);
        }
    }

    public SqlBytes Serialize()
    {
        using (var ms = new MemoryStream())
        {
            Write(new BinaryWriter(ms));
            return new SqlBytes(ms.ToArray());
        }
    }

}