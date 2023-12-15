using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.VisualBasic;

public partial class UserDefinedFunctions
{
    private static readonly string Prefixes = "(Mr|St|Mrs|Ms|Dr)[.]";
    private static readonly string Websites = "[.](com|net|org|io|gov)";
    private static readonly string Alphabets = "([A-Za-z])";
    private static readonly string Numbers = "([0-9])";
    private static readonly string Suffixes = "(Inc|Ltd|Jr|Sr|Co)";
    private static readonly string Starters = "(Mr|Mrs|Ms|Dr|He\\s|She\\s|It\\s|They\\s|Their\\s|Our\\s|We\\s|But\\s|However\\s|That\\s|This\\s|Where\\s|When\\s|Who\\s|Why\\s)";
    private static readonly string Acronyms = "([A-Z][.][A-Z][.](?:[A-Z][.])?)";

    private static List<string> segAsSentence(string txt)
    {
        // 使用正則表達式處理字符串
        txt = Regex.Replace(txt, Prefixes, "$1<prd>");
        txt = Regex.Replace(txt, Websites, "<prd>$1");
        if (txt.Contains("Ph.D")) txt = txt.Replace("Ph.D.", "Ph<prd>D<prd>");
        txt = Regex.Replace(txt, "\\s" + Alphabets + "[.] ", " $1<prd> ");
        txt = Regex.Replace(txt, Acronyms + " " + Starters, "$1<stop> $2");
        txt = Regex.Replace(txt, Alphabets + "[.]" + Alphabets + "[.]" + Alphabets + "[.]", "$1<prd>$2<prd>$3<prd>");
        txt = Regex.Replace(txt, Alphabets + "[.]" + Alphabets + "[.]", "$1<prd>$2<prd>");
        txt = Regex.Replace(txt, Numbers + "[.]" + Numbers, "$1<prd>$2");
        txt = Regex.Replace(txt, " " + Suffixes + "[.] " + Starters, " $1<stop> $2");
        txt = Regex.Replace(txt, " " + Suffixes + "[.]", " $1<prd>");
        txt = Regex.Replace(txt, " " + Alphabets + "[.]", " $1<prd>");

        // 處理引號和句末標點的組合
        txt = txt.Replace(".”", "”.")
                 .Replace(".\"", "\".")
                 .Replace("!\"", "\"!")
                 .Replace("?\"", "\"?");

        // 替換省略號和接下來的字符
        txt = Regex.Replace(txt, "(\\.\\.\\.)([^”’])", "<prd><prd><prd><stop>");

        // 將句末的點、問號、驚嘆號替換為<stop>
        txt = txt.Replace(".", ".<stop>")
                 .Replace("?", "?<stop>")
                 .Replace("!", "!<stop>")
                 .Replace("<prd>", ".");

        // 對中文的標點進行處理
        txt = Regex.Replace(txt, "([。！？\\?])([^”’])", "$1<stop>$2");
        txt = Regex.Replace(txt, "(\\.{6})([^”’])", "$1<stop>$2");
        txt = Regex.Replace(txt, "([。！？\\?][”’])([^。！？\\?])", "$1<stop>$2");
     

        // 移除尾部空白並按<stop>分割
        txt = txt.Trim();
        List<string> sentences = new List<string>(txt.Split(new string[] { "<stop>" }, StringSplitOptions.RemoveEmptyEntries));
        return sentences;
    }

    [return: SqlFacet(MaxSize = -1)]
    [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true)]
    public static SqlString SplitText([SqlFacet(MaxSize = -1)] SqlString inputText)
    {
        string returnstring = string.Join("\r\n", segAsSentence(inputText.Value));

        return new SqlString (returnstring);
    }

    [return: SqlFacet(MaxSize = -1)]
    [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true)]
    public static SqlString ChineseFull2Half([SqlFacet(MaxSize = -1)] SqlString inputText)
    {
        string returnstring = string.Join("\r\n", segAsSentence(inputText.Value));

        return new SqlString(returnstring);
    }

    [return: SqlFacet(MaxSize = -1)]
    [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true)]
    public static SqlString ChineseHalf2Full([SqlFacet(MaxSize = -1)] SqlString inputText)
    {
        string returnstring = string.Join("\r\n", segAsSentence(inputText.Value));

        return new SqlString(returnstring);
    }


    [Microsoft.SqlServer.Server.SqlFunction]
    [return: SqlFacet(MaxSize = -1)]
    public static SqlString ToTraditionalChinese([SqlFacet(MaxSize = -1)] SqlString InputString)
    {
        // 將程式碼放在此處
        return new SqlString(Microsoft.VisualBasic.Strings.StrConv(InputString.Value, Microsoft.VisualBasic.VbStrConv.TraditionalChinese, 2052));
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    [return: SqlFacet(MaxSize = -1)]
    public static SqlString ToSimplifiedChinese([SqlFacet(MaxSize = -1)] SqlString InputString)
    {

        return new SqlString(Microsoft.VisualBasic.Strings.StrConv(InputString.Value, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 2052));
    }





}
