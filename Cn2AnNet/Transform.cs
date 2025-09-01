using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cn2AnNet
{
    public class Transform
    {
        private string allNum;
        private string allUnit;
        private Func<object, string, double> cn2an;
        private Func<object, string, string> an2cn;
        private string cnPattern;
        private string smartCnPattern;

        public Transform()
        {
            allNum = "零一二三四五六七八九";
            allUnit = string.Join("", Conf.UNIT_CN2AN.Keys);
            cn2an = new Cn2An().Cn2AnConvert;
            an2cn = new An2Cn().An2CnConvert;
            cnPattern = $"负?([{allNum}{allUnit}]+点)?[{allNum}{allUnit}]+";
            smartCnPattern = $"-?([0-9]+.)?[0-9]+[{allUnit}]+";
        }

        public string TransformText(string inputs, string method = "cn2an")
        {
            if (method == "cn2an")
            {
                inputs = inputs.Replace("廿", "二十").Replace("半", "0.5").Replace("两", "2");
                // date
                inputs = Regex.Replace(inputs,
                    $@"((({smartCnPattern})|({cnPattern}))年)?([{allNum}十]+月)?([{allNum}十]+日)?",
                    m => SubUtil(m.Value, "cn2an", "date"));
                // fraction
                inputs = Regex.Replace(inputs, $@"{cnPattern}分之{cnPattern}",
                    m => SubUtil(m.Value, "cn2an", "fraction"));
                // percent
                inputs = Regex.Replace(inputs, $@"百分之{cnPattern}",
                    m => SubUtil(m.Value, "cn2an", "percent"));
                // celsius
                inputs = Regex.Replace(inputs, $@"{cnPattern}摄氏度",
                    m => SubUtil(m.Value, "cn2an", "celsius"));
                // number
                string output = Regex.Replace(inputs, cnPattern,
                    m => SubUtil(m.Value, "cn2an", "number"));
                return output;
            }
            else if (method == "an2cn")
            {
                // date
                inputs = Regex.Replace(inputs, @"(\d{2,4}年)?(\d{1,2}月)?(\d{1,2}日)?",
                    m => SubUtil(m.Value, "an2cn", "date"));
                // fraction
                inputs = Regex.Replace(inputs, @"\d+/\d+",
                    m => SubUtil(m.Value, "an2cn", "fraction"));
                // percent
                inputs = Regex.Replace(inputs, @"-?(\d+\.)?\d+%",
                    m => SubUtil(m.Value, "an2cn", "percent"));
                // celsius
                inputs = Regex.Replace(inputs, @"\d+℃",
                    m => SubUtil(m.Value, "an2cn", "celsius"));
                // number
                string output = Regex.Replace(inputs, @"-?(\d+\.)?\d+",
                    m => SubUtil(m.Value, "an2cn", "number"));
                return output;
            }
            else
            {
                throw new ArgumentException($"error method: {method}, only support 'cn2an' and 'an2cn'!");
            }
        }

        private string SubUtil(string inputs, string method = "cn2an", string subMode = "number")
        {
            try
            {
                if (!string.IsNullOrEmpty(inputs))
                {
                    if (method == "cn2an")
                    {
                        if (subMode == "date")
                        {
                            return Regex.Replace(inputs, $@"(({smartCnPattern})|({cnPattern}))",
                                m => cn2an(m.Value, "smart").ToString());
                        }
                        else if (subMode == "fraction")
                        {
                            if (inputs[0] != '百')
                            {
                                string fracResult = Regex.Replace(inputs, cnPattern,
                                    m => cn2an(m.Value, "smart").ToString());
                                string[] parts = fracResult.Split(new[] { "分之" }, StringSplitOptions.None);
                                return $"{parts[1]}/{parts[0]}";
                            }
                            else
                            {
                                return inputs;
                            }
                        }
                        else if (subMode == "percent")
                        {
                            return Regex.Replace(inputs, $"(?<=百分之){cnPattern}",
                                m => cn2an(m.Value, "smart").ToString()).Replace("百分之", "") + "%";
                        }
                        else if (subMode == "celsius")
                        {
                            return Regex.Replace(inputs, $"{cnPattern}(?=摄氏度)",
                                m => cn2an(m.Value, "smart").ToString()).Replace("摄氏度", "℃");
                        }
                        else if (subMode == "number")
                        {
                            return cn2an(inputs, "smart").ToString();
                        }
                        else
                        {
                            throw new Exception($"error sub_mode: {subMode} !");
                        }
                    }
                    else
                    {
                        if (subMode == "date")
                        {
                            inputs = Regex.Replace(inputs, @"\d+(?=年)",
                                m => an2cn(m.Value, "direct"));
                            return Regex.Replace(inputs, @"\d+",
                                m => an2cn(m.Value, "low"));
                        }
                        else if (subMode == "fraction")
                        {
                            string fracResult = Regex.Replace(inputs, @"\d+", m => an2cn(m.Value, "low"));
                            string[] parts = fracResult.Split('/');
                            return $"{parts[1]}分之{parts[0]}";
                        }
                        else if (subMode == "celsius")
                        {
                            return an2cn(inputs.Substring(0, inputs.Length - 1), "low") + "摄氏度";
                        }
                        else if (subMode == "percent")
                        {
                            return "百分之" + an2cn(inputs.Substring(0, inputs.Length - 1), "low");
                        }
                        else if (subMode == "number")
                        {
                            return an2cn(inputs, "low");
                        }
                        else
                        {
                            throw new Exception($"error sub_mode: {subMode} !");
                        }
                    }
                }
                return inputs;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: {e.Message}");
                return inputs;
            }
        }
    }
}